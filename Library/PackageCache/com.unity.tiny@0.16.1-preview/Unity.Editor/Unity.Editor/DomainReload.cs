using System;
using System.Collections.Generic;
using System.IO;
using Unity.Authoring;
using Unity.Collections;
using Unity.Editor.Extensions;
using Unity.Editor.Persistence;
using Unity.Editor.Serialization;
using Unity.Entities;
using Unity.Properties;
using Unity.Serialization;
using Unity.Serialization.Json;
using Unity.Tiny.Scenes;
using UnityEditor;

namespace Unity.Editor
{
    [InitializeOnLoad]
    internal static class DomainReload
    {
        private const int k_TempVersion = 2;
        private static bool m_ProgressBarDisplayed;

        /// <summary>
        /// Serializable temp data.
        /// </summary>
        private struct SessionState
        {
            public string ProjectAssetGuid;
            public IList<Scene> Scenes;
            public EntityManager EntityManager;

            internal class PropertyBag : PropertyBag<SessionState>
            {
                private static readonly Property<SessionState, int> s_SerializedVersion = new Property<SessionState, int>(
                    "SerializedVersion",
                    (ref SessionState c) => k_TempVersion);

                private static readonly Property<SessionState, string> s_ProjectPath = new Property<SessionState, string>(
                    nameof(ProjectAssetGuid),
                    (ref SessionState c) => c.ProjectAssetGuid);

                private static readonly ListProperty<SessionState, Scene> s_Scenes = new ListProperty<SessionState, Scene>(
                    nameof(Scenes),
                    (ref SessionState c) => c.Scenes,
                    (ref SessionState c, IList<Scene> v) => c.Scenes = v
                    );

                private struct EntitiesProperty : ICollectionProperty<SessionState, IEnumerable<EntityContainer>>
                {
                    private readonly NativeArray<Entity> m_Entities;

                    public EntitiesProperty(NativeArray<Entity> entities)
                    {
                        m_Entities = entities;
                    }

                    public string GetName() => "Entities";
                    public bool IsReadOnly => true;
                    public bool IsContainer => false;
                    public IPropertyAttributeCollection Attributes => null;
                    public IEnumerable<EntityContainer> GetValue(ref SessionState container) => null;
                    public int GetCount(ref SessionState container) => m_Entities.Length;

                    public void SetValue(ref SessionState container, IEnumerable<EntityContainer> value) => throw new Exception("Property is ReadOnly");
                    public void SetCount(ref SessionState container, int count) => throw new Exception("Property is ReadOnly");
                    public void Clear(ref SessionState container) => throw new Exception("Property is ReadOnly");

                    public void GetPropertyAtIndex<TGetter>(ref SessionState container, int index, ref ChangeTracker changeTracker, TGetter getter)
                        where TGetter : ICollectionElementGetter<SessionState>
                    {
                        var entity = new EntityContainer(container.EntityManager, m_Entities[index]);
                        getter.VisitProperty<EntityElementProperty, EntityContainer>(new EntityElementProperty(entity, index), ref container);
                    }
                }

                private struct EntityElementProperty : ICollectionElementProperty<SessionState, EntityContainer>
                {
                    private readonly EntityContainer m_Entity;

                    public EntityElementProperty(EntityContainer entity, int index)
                    {
                        m_Entity = entity;
                        Index = index;
                    }

                    public int Index { get; }
                    public string GetName() => $"[{Index}]";

                    public bool IsReadOnly => true;
                    public bool IsContainer => true;
                    public IPropertyAttributeCollection Attributes => null;

                    public EntityContainer GetValue(ref SessionState container) => m_Entity;
                    public void SetValue(ref SessionState container, EntityContainer value) => throw new Exception("Property is ReadOnly");
                }

                public override void Accept<TVisitor>(ref SessionState container, TVisitor visitor, ref ChangeTracker changeTracker)
                {
                    visitor.VisitProperty<Property<SessionState, int>, SessionState, int>(s_SerializedVersion, ref container, ref changeTracker);
                    visitor.VisitProperty<Property<SessionState, string>, SessionState, string>(s_ProjectPath, ref container, ref changeTracker);
                    visitor.VisitCollectionProperty<ListProperty<SessionState, Scene>, SessionState, IList<Scene>>(s_Scenes, ref container, ref changeTracker);
                    using (var entities = container.EntityManager.GetAllEntities(Allocator.TempJob))
                    {
                        visitor.VisitCollectionProperty<EntitiesProperty, SessionState, IEnumerable<EntityContainer>>(new EntitiesProperty(entities), ref container, ref changeTracker);
                    }
                }

                public override bool FindProperty<TAction>(string name, ref SessionState container, ref ChangeTracker changeTracker, ref TAction action)
                    => throw new NotImplementedException();
            }
        }

        private static readonly FileInfo s_TempFile = new FileInfo("Library/DotsWorkspace.temp");

        public static bool IsDomainReloading { get; private set; }

        static DomainReload()
        {
            // Save and close the project during an assembly reload or application shutdown
            AssemblyReloadEvents.beforeAssemblyReload += HandleDomainWillUnload;
            EditorApplication.quitting += HandleDomainWillUnload;

            // Register to unity application events
            EditorApplication.update += HandleUpdate;

            PropertyBagResolver.Register(new SessionState.PropertyBag());
        }

        private static void HandleDomainWillUnload()
        {
            if (Application.AuthoringProject == null)
            {
                return;
            }

            SaveState(s_TempFile, Application.AuthoringProject);
        }

        private static void HandleUpdate()
        {
            var projectToReOpen = UnityEditor.SessionState.GetString(Project.k_OpenNewlyCreatedProjectSessionKey, string.Empty);
            if (!string.IsNullOrEmpty(projectToReOpen))
            {
                if (!m_ProgressBarDisplayed)
                {
                    EditorUtility.DisplayProgressBar("Opening Project", "First-time script compilation, please wait!", 0f);
                    m_ProgressBarDisplayed = true;
                }

                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                {
                    return;
                }

                EditorUtility.ClearProgressBar();
                m_ProgressBarDisplayed = false;
                UnityEditor.SessionState.EraseString(Project.k_OpenNewlyCreatedProjectSessionKey);

                var project = Project.Open(new FileInfo(projectToReOpen));
                Application.SetAuthoringProject(project);
                return;
            }

            if (File.Exists(s_TempFile.FullName))
            {
                try
                {
                    IsDomainReloading = true;
                    var project = RestoreState(s_TempFile);
                    Application.SetAuthoringProject(project);
                }
                finally
                {
                    IsDomainReloading = false;
                    File.Delete(s_TempFile.FullName);
                }
            }
        }

        private static void SaveState(FileInfo file, Project project)
        {
            var editorSceneManager = project.Session.GetManager<IEditorSceneManager>();

            var scenes = new List<Scene>();
            for (var i = 0; i < editorSceneManager.LoadedSceneCount; i++)
            {
                scenes.Add(editorSceneManager.GetLoadedSceneAtIndex(i));
            }

            var assetGuid = AssetDatabaseUtility.GetAssetGuid(Application.AuthoringProject.GetProjectFile());
            var entityManager = project.Session.GetManager<IWorldManager>().EntityManager;

            // This does not scale!
            // @TODO Stream API
            var json = JsonSerialization.Serialize(new SessionState
            {
                ProjectAssetGuid = assetGuid,
                EntityManager = entityManager,
                Scenes = scenes
            }, new EntityJsonVisitor(entityManager));

            file.WriteAllText(json);
        }

        private static Project RestoreState(FileInfo file)
        {
            var config = new SerializedObjectReaderConfiguration
            {
                UseReadAsync = false,
                BlockBufferSize = 256 << 10, // 256 KB
                TokenBufferSize = 1024,
                NodeBufferSize = JsonFrontEnd.EntityBatchSize,
                ValidationType = JsonValidationType.Standard,
                OutputBufferSize = 1024 << 10 // 1 MB
            };

            Project project;

            using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, config.BlockBufferSize, FileOptions.Asynchronous))
            using (var reader = new SerializedObjectReader(stream, config))
            {
                if (reader.Step(out var root) != NodeType.BeginObject)
                {
                    Debug.LogWarning("Temp file was not in the correct format. Domain was not reloaded.");
                    return null;
                }

                // ASSUMPTION: All members are written BEFORE the blob of entities.
                // Read until we hit "Entities":
                NodeType node;

                while ((node = reader.Step(out var view)) != NodeType.None)
                {
                    if ((node & NodeType.ObjectKey) == 0)
                    {
                        continue;
                    }

                    if (view.AsStringView().Equals("Entities"))
                    {
                        break;
                    }
                }

                var sessionState = root.AsObjectView();

                if (!sessionState.TryGetValue("SerializedVersion", out var versionView) ||
                    versionView.AsInt64() != k_TempVersion)
                {
                    Debug.LogWarning($"Temp file version has changed. Domain was not reloaded");
                    return null;
                }

                var projectAssetGuid = sessionState[nameof(SessionState.ProjectAssetGuid)].AsStringView().ToString();
                var projectPath = AssetDatabase.GUIDToAssetPath(projectAssetGuid);

                if (!File.Exists(projectPath))
                {
                    return null;
                }

                project = Project.Open(new FileInfo(projectPath));

                var world = new World(nameof(DomainReload));
                try
                {
                    JsonFrontEnd.Accept(world.EntityManager, reader);
                    project.Session.GetManager<WorldManager>().EntityManager.MoveEntitiesFrom(world.EntityManager);
                }
                finally
                {
                    world.Dispose();
                }

                var worldManager = project.Session.GetManager<IWorldManager>();
                worldManager.EntityManager.World.GetOrCreateSystem<EntityReferenceRemapSystem>().Update();
                worldManager.EntityManager.World.GetOrCreateSystem<RemoveRemapInformationSystem>().Update();
            }

            return project;
        }
    }
}
