using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Authoring;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Editor.Build;
using Unity.Editor.Extensions;
using Unity.Editor.Persistence;
using Unity.Entities;
using Unity.Serialization.Json;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Scenes;
using UnityEditor;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Editor
{
    /// <summary>
    /// Encapsulates an authoring Project in the Editor.
    /// </summary>
    public class Project : IDisposable
    {
        private const string k_ConfigurationFileExtension = "configuration";
        private const string k_AssemblyDefinitionFileExtension = "asmdef";

        private static readonly List<Project> s_Projects = new List<Project>();

        internal static event Action<Project> ProjectCreated = delegate { };
        internal static event Action<Project> ProjectOpened = delegate { };
        internal static event Action<Project> ProjectDisposing = delegate { };
        internal static event Action<Project> ProjectDisposed = delegate { };

        internal const string OpenNewlyCreatedProjectSessionKey = "OpenNewlyCreatedProject";

        internal static IReadOnlyCollection<Project> Projects => s_Projects.AsReadOnly();

        /// <summary>
        /// The session in which the Project lives.
        /// </summary>
        public Session Session { get; private set; }

        /// <summary>
        /// Returns the manager that allows interaction with the world.
        /// </summary>
        public IWorldManager WorldManager { get; }

        internal IArchetypeManager ArchetypeManager { get; }
        internal IPersistenceManager PersistenceManager { get; }
        internal IEditorSceneManager EditorSceneManager { get; }

        /// <summary>
        /// Returns the <see cref="EntityManager"/> associated with the current editing world.
        /// </summary>
        public EntityManager EntityManager => WorldManager.EntityManager;

        /// <summary>
        /// Returns the directory where the Project is saved.
        /// </summary>
        public DirectoryInfo Directory => GetProjectFile()?.Directory;

        /// <summary>
        /// Returns the Project Settings associated with the Project.
        /// </summary>
        public ProjectSettings Settings { get; set; }

        /// <summary>
        /// Returns the <see cref="System.Guid"/> of the project.
        /// </summary>
        public Guid Guid { get; private set; }

        /// <summary>
        /// Returns the name of the Project.
        /// </summary>
        public string Name => Path.GetFileNameWithoutExtension(GetProjectFile().Name);

        private Project()
        {
            Session = Session.Create();
            WorldManager = Session.GetManager<IWorldManager>();
            ArchetypeManager = Session.GetManager<IArchetypeManager>();
            PersistenceManager = Session.GetManager<IPersistenceManager>();
            EditorSceneManager = Session.GetManager<IEditorSceneManager>();
        }

        public void Dispose()
        {
            ProjectDisposing(this);

            s_Projects.Remove(this);

            Session.Dispose();
            Session = null;

            ProjectDisposed(this);
        }

        internal FileInfo GetProjectFile() => new FileInfo(AssetDatabase.GUIDToAssetPath(Guid.ToString("N")));
        internal FileInfo GetAssemblyDefinitionFile() => new FileInfo(AssetDatabase.GUIDToAssetPath(Settings.MainAsmdef.ToString("N")));
        internal FileInfo GetConfigurationFile() => new FileInfo(AssetDatabase.GUIDToAssetPath(Settings.Configuration.ToString("N")));

        internal static Project Create(DirectoryInfo directory, string name)
        {
            const string progressBarTitle = "Generating new DOTS project";

            EditorUtility.DisplayProgressBar(progressBarTitle, string.Empty, 0);

            Project project;

            try
            {
                var projectDirectory = directory.Combine(name);
                projectDirectory.EnsureExists();

                var projectFile = projectDirectory.GetFile(name).ChangeExtension("project");

                AssetDatabase.StartAssetEditing();

                try
                {
                    project = new Project
                    {
                        Settings = ProjectSettings.Default
                    };

                    EditorUtility.DisplayProgressBar(progressBarTitle, "Creating assemblies", 0.2f);
                    GenerateNewProjectAsmdef(projectDirectory, name);
                    EditorUtility.DisplayProgressBar(progressBarTitle, "Creating assets", 0.6f);
                    GenerateNewProjectAssets(projectDirectory, name, project.WorldManager, project.ArchetypeManager, project.PersistenceManager);
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }

                SessionState.SetString(OpenNewlyCreatedProjectSessionKey, projectFile.FullName);

                EditorUtility.DisplayProgressBar(progressBarTitle, "Compiling project assemblies", 0.8f);

                // Force a re-import to trigger a script recompilation for the new assemblies.
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

                // All generated assets are now imported.
                // Patch up any guid references for the `Project` settings.
                project.Settings.Configuration = new Guid(projectFile.ChangeExtension(k_ConfigurationFileExtension).ToAssetGuid());
                project.Settings.MainAsmdef = new Guid(projectFile.ChangeExtension(k_AssemblyDefinitionFileExtension).ToAssetGuid());

                // Save the .project file
                Persist(projectFile, project);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            s_Projects.Add(project);
            ProjectCreated(project);

            return project;
        }

        private static void GenerateNewProjectAsmdef(DirectoryInfo projectDirectory, string name)
        {
            var asmdef = new AssemblyDefinition
            {
                name = name.ToIdentifier(),
                references = new[]
                {
                    "Unity.Collections",
                    "Unity.Entities",
                    "Unity.Mathematics",
                    "Unity.Tiny.Audio",
                    "Unity.Tiny.AudioHTML",
                    "Unity.Tiny.AudioNative",
                    "Unity.Tiny.Core",
                    "Unity.Tiny.Core2D",
                    "Unity.Tiny.Core2DTypes",
                    "Unity.Tiny.EntryPoint",
                    "Unity.Tiny.GLFW",
                    "Unity.Tiny.HitBox2D",
                    "Unity.Tiny.HTML",
                    "Unity.Tiny.Image2D",
                    "Unity.Tiny.Image2DIOHTML",
                    "Unity.Tiny.Image2DIOSTB",
                    "Unity.Tiny.Input",
                    "Unity.Tiny.InputGLFW",
                    "Unity.Tiny.InputHTML",
                    "Unity.Tiny.IO",
                    "Unity.Tiny.Particles",
                    "Unity.Tiny.PointQuery",
                    "Unity.Tiny.Renderer",
                    "Unity.Tiny.RendererCanvas",
                    "Unity.Tiny.RendererGL",
                    "Unity.Tiny.RendererGLES2",
                    "Unity.Tiny.RendererGLNative",
                    "Unity.Tiny.Scenes",
                    "Unity.Tiny.Sprite2D",
                    "Unity.Tiny.Text",
                    "Unity.Tiny.TextHTML",
                    "Unity.Tiny.Tweens",
                    "Unity.Tiny.UIControls",
                    "Unity.Tiny.UILayout",
                    "Unity.Tiny.Video",
                    "Unity.Tiny.VideoHTML",
                    "Unity.Tiny.VideoNative",
                    "Unity.Tiny.Watchers"
                }
            };

            // Add one C# file so the compilation pipeline doesn't complain
            var assemblyInfoTemplate = Application.PackageDirectory.Combine("Unity.Editor", "Unity.Editor").GetFile("AssemblyInfo.cs.in");
            var assemblyInfoFile = projectDirectory.Combine("Scripts").GetFile("AssemblyInfo.cs");
            assemblyInfoTemplate.CopyTo(assemblyInfoFile);

            var assemblyDefinitionFile = projectDirectory.GetFile(name).ChangeExtension(k_AssemblyDefinitionFileExtension);
            asmdef.Serialize(assemblyDefinitionFile);
        }

        private static void GenerateNewProjectAssets(
            DirectoryInfo projectDirectory,
            string name,
            IWorldManager worldManager,
            IArchetypeManager archetypeManager,
            IPersistenceManager persistenceManager
        )
        {
            // Create config entity
            var configEntity = worldManager.CreateEntity(archetypeManager.Config);
            worldManager.EntityManager.SetComponentData(configEntity, DisplayInfo.Default);

            // Generate main scene
            var cameraEntity = worldManager.CreateEntity("Camera", archetypeManager.Camera);
            worldManager.EntityManager.SetComponentData(cameraEntity, Camera2D.Default);
            worldManager.EntityManager.SetComponentData(cameraEntity, DomainCache.GetDefaultValue<Camera2D>());
            worldManager.EntityManager.SetComponentData(cameraEntity, DomainCache.GetDefaultValue<Rotation>());
            worldManager.EntityManager.SetComponentData(cameraEntity, DomainCache.GetDefaultValue<NonUniformScale>());

            using (var entities = new NativeArray<Entity>(cameraEntity.AsArray(), Allocator.Temp))
            {
                var scene = SceneManager.Create(worldManager.EntityManager, entities, Guid.NewGuid());
                var sceneFile = projectDirectory.Combine("Scenes").GetFile("MainScene.scene");
                sceneFile.Directory.EnsureExists();
                persistenceManager.SaveScene(worldManager.EntityManager, scene, sceneFile.FullName);

                var sceneReference = new SceneReference { SceneGuid = scene.SceneGuid.Guid };

                AddScene(worldManager.EntityManager, worldManager.GetConfigEntity(), sceneReference);
                AddStartupScene(worldManager.EntityManager, worldManager.GetConfigEntity(), sceneReference);
            }

            // Generate configuration scene
            using (var entities = new NativeArray<Entity>(configEntity.AsArray(), Allocator.Temp))
            {
                var configurationScene = SceneManager.Create(worldManager.EntityManager, entities, ConfigurationScene.Guid);
                var configurationFile = projectDirectory.GetFile(name).ChangeExtension("configuration");
                configurationFile.Directory.EnsureExists();
                persistenceManager.SaveScene(worldManager.EntityManager, configurationScene, configurationFile.FullName);

                // Hack: remove scene guid/instance id and persistence id
                worldManager.EntityManager.RemoveComponent<SceneGuid>(configEntity);
                worldManager.EntityManager.RemoveComponent<SceneInstanceId>(configEntity);
            }
        }

        internal static Project Open(FileInfo projectFile)
        {
            var project = new Project
            {
                Settings = JsonSerialization.Deserialize<ProjectSettings>(projectFile.FullName)
            };

            project.Guid = new Guid(projectFile.ToAssetGuid());

            // Patch up any guid references if they are missing.
            if (Guid.Empty == project.Settings.Configuration)
            {
                project.Settings.Configuration = new Guid(AssetDatabaseUtility.GetAssetGuid(projectFile.ChangeExtension(k_ConfigurationFileExtension)));
            }

            if (Guid.Empty == project.Settings.MainAsmdef)
            {
                project.Settings.MainAsmdef = new Guid(AssetDatabaseUtility.GetAssetGuid(projectFile.ChangeExtension(k_AssemblyDefinitionFileExtension)));
            }

            if (!DomainReload.IsDomainReloading)
            {
                var entityManager = project.EntityManager;
                var configFile = project.GetConfigurationFile();

                if (configFile.Exists)
                {
                    project.PersistenceManager.LoadScene(entityManager, configFile.FullName);

                    // Hack: remove scene guid/instance id
                    var configEntity = project.WorldManager.GetConfigEntity();
                    entityManager.RemoveComponent<SceneGuid>(configEntity);
                    entityManager.RemoveComponent<SceneInstanceId>(configEntity);
                }
                else
                {
                    var configEntity = project.WorldManager.CreateEntity(project.ArchetypeManager.Config);
                    entityManager.SetComponentData(configEntity, DisplayInfo.Default);
                }
            }

            s_Projects.Add(project);
            ProjectOpened(project);

            DomainCache.CacheIncludedAssemblies(project);
            return project;
        }

        internal static void CloseAll()
        {
            var projects = s_Projects.ToArray();

            for (var i = 0; i < projects.Length; i++)
            {
                projects[i].Dispose();
            }

            s_Projects.Clear();
        }

        /// <summary>
        /// Saves the project to disk.
        /// </summary>
        public void Save()
        {
            SaveAs(GetProjectFile());
        }

        /// <summary>
        /// Saves the project to disk as a new file.
        /// </summary>
        /// <param name="projectFile">The path where to save the project.</param>
        public void SaveAs(FileInfo projectFile)
        {
            AssetDatabase.SaveAssets();

            if (projectFile == null)
            {
                return;
            }

            // Save the .project file
            Persist(projectFile, this);

            // Serialize configuration scene
            var configEntity = WorldManager.GetConfigEntity();
            using (var entities = new NativeArray<Entity>(configEntity.AsArray(), Allocator.Temp))
            {
                var configScene = SceneManager.Create(EntityManager, entities, ConfigurationScene.Guid);
                var configFile = GetConfigurationFile();
                PersistenceManager.SaveScene(EntityManager, configScene, configFile.FullName);

                // Hack: remove scene guid/instance id
                EntityManager.RemoveComponent<SceneGuid>(configEntity);
                EntityManager.RemoveComponent<SceneInstanceId>(configEntity);
            }

            // Serialize loaded scenes
            if (projectFile.Exists)
            {
                for (var i = 0; i < EditorSceneManager.LoadedSceneCount; i++)
                {
                    var scene = EditorSceneManager.GetLoadedSceneAtIndex(i);
                    var assetPath = PersistenceManager.GetSceneAssetPath(scene);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        PersistenceManager.SaveScene(EntityManager, scene, assetPath);
                    }
                }
            }
        }

        private static void Persist(FileInfo file, Project project)
        {
            file.WriteAllText(JsonSerialization.Serialize(project.Settings));

            var projectAssetPath = file.ToAssetPath();

            // Import asset since we just created it, to refresh the asset database
            AssetDatabase.ImportAsset(projectAssetPath, ImportAssetOptions.ForceSynchronousImport);

            // Now we can safely get the asset guid
            project.Guid = new Guid(AssetDatabase.AssetPathToGUID(projectAssetPath));
        }

        private static bool IsAssemblyIncluded(AssemblyDefinition asmdef, Platform platform)
        {
            var emptyIncludes = (asmdef.includePlatforms == null || asmdef.includePlatforms.Length == 0);
            var emptyExcludes = (asmdef.excludePlatforms == null || asmdef.excludePlatforms.Length == 0);
            if (emptyIncludes && emptyExcludes)
            {
                // any platform
                return true;
            }

            var platformName = platform.GetUnityPlatformName();

            if (!emptyIncludes && !asmdef.includePlatforms.Contains(platformName))
            {
                return false;
            }

            if (!emptyExcludes && asmdef.excludePlatforms.Contains(platformName))
            {
                return false;
            }

            return true;
        }

        internal IEnumerable<AssemblyDefinition> IncludedAssemblyDefinitions()
        {
            var projectDir = Directory;
            if (projectDir == null || !projectDir.Exists)
            {
                yield break;
            }

            var assemblyNames = new HashSet<string>();
            var assemblies = new Stack<AssemblyDefinition>();
            var platform = Session.GetManager<WorkspaceManager>().ActivePlatform;

            // TODO: improve rather weak heuristic to find main project assemblies from a Project
            foreach (var asmDefFile in projectDir.EnumerateFiles("*.asmdef", SearchOption.AllDirectories))
            {
                var asmdef = AssemblyDefinition.Deserialize(asmDefFile);
                if (asmDefFile.Directory.GetFiles("*.project").Any())
                {
                    if (IsAssemblyIncluded(asmdef, platform))
                    {
                        assemblyNames.Add(asmdef.name);
                        assemblies.Push(asmdef);
                    }
                }
            }

            // walk the assembly tree from main assemblies
            while (assemblies.Count > 0)
            {
                var assembly = assemblies.Pop();
                yield return assembly;

                if (assembly.references != null)
                {
                    foreach (var reference in assembly.references)
                    {
                        var asmDefPath = UnityEditor.Compilation.CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyReference(reference);
                        if (string.IsNullOrEmpty(asmDefPath))
                        {
                            continue;
                        }

                        var asmDef = AssemblyDefinition.Deserialize(new FileInfo(asmDefPath));
                        if (assemblyNames.Add(asmDef.name) && IsAssemblyIncluded(asmDef, platform))
                        {
                            assemblies.Push(asmDef);
                        }
                    }
                }
            }
        }

        internal IEnumerable<System.Reflection.Assembly> IncludedAssemblies()
        {
            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToDictionary(a => a.GetName().Name);
            foreach (var assemblyDefinition in IncludedAssemblyDefinitions())
            {
                if (domainAssemblies.TryGetValue(assemblyDefinition.name, out var assembly))
                {
                    yield return assembly;
                }
            }
        }

        internal NativeArray<SceneReference> GetScenes(Allocator allocator = Allocator.Temp)
        {
            return GetScenes(EntityManager, WorldManager.GetConfigEntity(), allocator);
        }

        internal static NativeArray<SceneReference> GetScenes(EntityManager entityManager, Entity configEntity, Allocator allocator = Allocator.Temp)
        {
            Assert.AreNotEqual(Entity.Null, configEntity);
            Assert.IsTrue(entityManager.HasComponent<ConfigurationTag>(configEntity));
            Assert.IsTrue(entityManager.HasComponent<Scenes>(configEntity));
            return entityManager.GetBufferRO<Scenes>(configEntity).Reinterpret<SceneReference>().ToNativeArray(allocator);
        }

        internal void AddScene(SceneReference sceneReference)
        {
            AddScene(EntityManager, WorldManager.GetConfigEntity(), sceneReference);
        }

        internal static void AddScene(EntityManager entityManager, Entity configEntity, SceneReference sceneReference)
        {
            Assert.AreNotEqual(Entity.Null, configEntity);
            Assert.IsTrue(entityManager.HasComponent<ConfigurationTag>(configEntity));
            Assert.IsTrue(entityManager.HasComponent<Scenes>(configEntity));
            var scenes = entityManager.GetBuffer<Scenes>(configEntity).Reinterpret<SceneReference>();
            if (!scenes.Contains(sceneReference))
            {
                scenes.Add(sceneReference);
            }
        }

        internal void RemoveScene(SceneReference sceneReference)
        {
            RemoveScene(EntityManager, WorldManager.GetConfigEntity(), sceneReference);
        }

        internal static void RemoveScene(EntityManager entityManager, Entity configEntity, SceneReference sceneReference)
        {
            Assert.AreNotEqual(Entity.Null, configEntity);
            Assert.IsTrue(entityManager.HasComponent<ConfigurationTag>(configEntity));
            Assert.IsTrue(entityManager.HasComponent<Scenes>(configEntity));
            entityManager.GetBuffer<Scenes>(configEntity).Reinterpret<SceneReference>().Remove(sceneReference);
            Assert.IsTrue(entityManager.HasComponent<StartupScenes>(configEntity));
            entityManager.GetBuffer<StartupScenes>(configEntity).Reinterpret<SceneReference>().Remove(sceneReference);
        }

        internal NativeArray<SceneReference> GetStartupScenes(Allocator allocator = Allocator.Temp)
        {
            return GetStartupScenes(EntityManager, WorldManager.GetConfigEntity(), allocator);
        }

        internal static NativeArray<SceneReference> GetStartupScenes(EntityManager entityManager, Entity configEntity, Allocator allocator = Allocator.Temp)
        {
            Assert.AreNotEqual(Entity.Null, configEntity);
            Assert.IsTrue(entityManager.HasComponent<ConfigurationTag>(configEntity));
            Assert.IsTrue(entityManager.HasComponent<StartupScenes>(configEntity));
            return entityManager.GetBuffer<StartupScenes>(configEntity).Reinterpret<SceneReference>().ToNativeArray(allocator);
        }

        internal void AddStartupScene(SceneReference sceneReference)
        {
            AddStartupScene(EntityManager, WorldManager.GetConfigEntity(), sceneReference);
        }

        internal static void AddStartupScene(EntityManager entityManager, Entity configEntity, SceneReference sceneReference)
        {
            Assert.AreNotEqual(Entity.Null, configEntity);
            Assert.IsTrue(entityManager.HasComponent<ConfigurationTag>(configEntity));
            Assert.IsTrue(entityManager.HasComponent<StartupScenes>(configEntity));
            var startupScenes = entityManager.GetBuffer<StartupScenes>(configEntity).Reinterpret<SceneReference>();
            if (!startupScenes.Contains(sceneReference))
            {
                startupScenes.Add(sceneReference);
            }
        }

        internal void RemoveStartupScene(SceneReference sceneReference)
        {
            RemoveStartupScene(EntityManager, WorldManager.GetConfigEntity(), sceneReference);
        }

        internal static void RemoveStartupScene(EntityManager entityManager, Entity configEntity, SceneReference sceneReference)
        {
            Assert.AreNotEqual(Entity.Null, configEntity);
            Assert.IsTrue(entityManager.HasComponent<ConfigurationTag>(configEntity));
            Assert.IsTrue(entityManager.HasComponent<StartupScenes>(configEntity));
            entityManager.GetBuffer<StartupScenes>(configEntity).Reinterpret<SceneReference>().Remove(sceneReference);
        }
    }
}
