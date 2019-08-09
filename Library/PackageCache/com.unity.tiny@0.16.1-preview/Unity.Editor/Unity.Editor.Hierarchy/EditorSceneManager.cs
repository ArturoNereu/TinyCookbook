using System;
using System.Collections.Generic;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Editor.Hierarchy;
using Unity.Editor.Persistence;
using Unity.Editor.Undo;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Scenes;
using UnityEditor;

namespace Unity.Editor
{
    internal interface IEditorSceneManager : ISessionManager
    {
        /// <summary>
        /// The current number of loaded scenes in the editor session.
        /// </summary>
        int LoadedSceneCount { get; }

        /// <summary>
        /// Returns the <see cref="Scene"/> at the given index in the EditorSceneManager
        ///
        /// This is the order that appears in the Hierarchy.
        /// </summary>
        Scene GetLoadedSceneAtIndex(int index);

        Scene GetActiveScene();
        Scene GetScene(Entity entity);

        Scene CreateNewScene();
        Scene LoadScene(string path);
        void UnloadScene(Scene scene);
        void UnloadSceneWithDialog(Scene scene);
        void SetActiveScene(Scene scene);

        string GetSceneName(Scene scene);
    }

    internal interface IEditorSceneManagerInternal : IEditorSceneManager, ISessionManagerInternal
    {
        SceneGraph GetGraphForScene(Scene scene);
        SceneGraph GetGraphForScene(SceneGuid guid);
        void LoadScene(Scene scene);

        void MoveSceneUp(Scene scene);
        void MoveSceneDown(Scene scene);
        
        void UnloadAllScenesByGuid(SceneGuid sceneGuid);
        EditorSceneManager.SceneSaveScope IgnoreSceneImport();

        bool IsSceneChanged(Scene scene);
        bool IsAnyInstanceOfSceneLoaded(Guid sceneGuid);
        Scene GetFirstInstanceOfSceneLoaded(Guid sceneGuid);
    }

    internal class EditorSceneManager : ISessionManagerInternal, IEditorSceneManagerInternal
    {
        public readonly struct SceneSaveScope : IDisposable
        {
            private readonly EditorSceneManager m_SceneManager;

            public SceneSaveScope(EditorSceneManager sceneManager)
            {
                m_SceneManager = sceneManager;
                m_SceneManager.m_SceneSaveScope++;
            }

            public void Dispose()
            {
                m_SceneManager.m_SceneSaveScope--;
            }
        }

        private readonly HashSet<Scene> m_ChangedScenes = new HashSet<Scene>();
        private readonly HashSet<Scene> m_RebuildScenes = new HashSet<Scene>();
        private readonly Dictionary<Guid, SceneGraph> m_Graphs = new Dictionary<Guid, SceneGraph>();

        private IPersistenceManager m_Persistence;
        private IWorldManager m_WorldManager;
        private IEditorUndoManager m_EditorUndoManager;
        private int m_SceneSaveScope;
        private Session m_Session;

        /// <summary>
        /// Workaround since we need to rely on the existence of the ConfigEntity
        /// </summary>
        private void EnsureComponentDataExists()
        {
            var entityManager = m_WorldManager.EntityManager;
            var entity = m_WorldManager.GetConfigEntity();
            if (!entityManager.HasComponent<WorkspaceScenes>(entity))
            {
                entityManager.AddBuffer<WorkspaceScenes>(entity);
            }
            if (!entityManager.HasComponent<ActiveScene>(entity))
            {
                entityManager.AddComponentData(entity, new ActiveScene());
            }
        }
        
        private DynamicBuffer<WorkspaceScenes> GetWorkspaceScenesRW()
        {
            EnsureComponentDataExists();
            var entityManager = m_WorldManager.EntityManager;
            var entity = m_WorldManager.GetConfigEntity();
            return entityManager.GetBuffer<WorkspaceScenes>(entity);
        }
        
        private DynamicBuffer<WorkspaceScenes> GetWorkspaceScenesRO()
        {
            EnsureComponentDataExists();
            var entityManager = m_WorldManager.EntityManager;
            var entity = m_WorldManager.GetConfigEntity();
            return entityManager.GetBufferRO<WorkspaceScenes>(entity);
        }

        public Scene GetActiveScene()
        {
            EnsureComponentDataExists();
            var entityManager = m_WorldManager.EntityManager;
            var entity = m_WorldManager.GetConfigEntity();
            return entityManager.GetComponentData<ActiveScene>(entity).Scene;
        }
        
        public void SetActiveScene(Scene scene)
        {
            EnsureComponentDataExists();
            var entityManager = m_WorldManager.EntityManager;
            var entity = m_WorldManager.GetConfigEntity();
            entityManager.SetComponentData(entity, new ActiveScene { Scene = scene });
            Bridge.EditorApplication.UpdateMainWindowTitle();
        }

        public void Load(Session session)
        {
            m_Session = session;

            AssetPostprocessorCallbacks.RegisterAssetImportedHandlerForType<SceneAsset>(HandleSceneImported);

            m_Persistence = session.GetManager<IPersistenceManager>();
            m_WorldManager = session.GetManager<IWorldManager>();
            m_EditorUndoManager = session.GetManager<IEditorUndoManager>();
            session.GetManager<IChangeManager>().BeginChangeTracking += HandleBeginChangeTracking;
            session.GetManager<IChangeManager>().EndChangeTracking += HandleEndChangeTracking;
            session.GetManager<IChangeManager>().RegisterChangeCallback(HandleChanges);
            m_EditorUndoManager.UndoRedoBatchEnded += HandleUndo;
        }

        public void Unload(Session session)
        {
            AssetPostprocessorCallbacks.UnregisterAssetImportedHandlerForType<SceneAsset>(HandleSceneImported);
            session.GetManager<IChangeManager>().UnregisterChangeCallback(HandleChanges);
            session.GetManager<IChangeManager>().BeginChangeTracking -= HandleBeginChangeTracking;
            session.GetManager<IChangeManager>().EndChangeTracking -= HandleEndChangeTracking;
            m_EditorUndoManager.UndoRedoBatchEnded -= HandleUndo;
        }

        /// <inheritdoc />
        public int LoadedSceneCount => GetWorkspaceScenesRO().Length;

        /// <inheritdoc />
        public Scene GetLoadedSceneAtIndex(int index)
        {
            return GetWorkspaceScenesRO()[index].Scene;
        }

        public Scene GetScene(Entity entity)
        {
            var entityManager = m_WorldManager.EntityManager;
            if (entity == Entity.Null
                || !entityManager.Exists(entity)
                || !entityManager.HasComponent<SceneInstanceId>(entity)
                || !entityManager.HasComponent<SceneGuid>(entity))
            {
                return Scene.Null;
            }
            
            var sceneId = entityManager.GetSharedComponentData<SceneInstanceId>(entity);
            var sceneGuid = entityManager.GetSharedComponentData<SceneGuid>(entity);
                
            return new Scene(sceneGuid, sceneId);
        }

        public Scene CreateNewScene()
        {
            var scene = SceneManager.Create(Guid.NewGuid());
            LoadScene(scene);
            return scene;
        }

        public SceneGraph GetGraphForScene(Scene scene)
        {
            var guid = scene.SceneGuid.Guid;
            if (!m_Graphs.TryGetValue(guid, out var graph) || null == graph)
            {
                m_Graphs[guid] = graph = new SceneGraph(m_Session, scene);
            }
            return graph;
        }

        public SceneGraph GetGraphForScene(SceneGuid sceneGuid)
        {
            var workspaceScenes = GetWorkspaceScenesRO();
            for (var i = 0; i < workspaceScenes.Length; i++)
            {
                if (workspaceScenes[i].Scene.SceneGuid.Guid == sceneGuid.Guid)
                {
                    return GetGraphForScene(workspaceScenes[i].Scene);
                }
            }

            return null;
        }

        public Scene LoadScene(string path)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            var sceneGuid = new SceneGuid {Guid = new Guid(sceneAsset.Guid)};

            if (IsAnyInstanceOfSceneLoaded(sceneGuid.Guid))
            {
                using (var scenes = GetAllLoadedScenesByGuid(sceneGuid))
                {
                    for (var i = 0; i < scenes.Length; i++)
                    {
                        UnloadScene(scenes[i]);
                    }
                }
            }

            var scene = m_Persistence.LoadScene(m_WorldManager.EntityManager, path);
            LoadScene(scene);
            return scene;
        }

        public void LoadScene(Scene scene)
        {
            var workspaceScenes = GetWorkspaceScenesRW();
            
            if (Contains(workspaceScenes, scene))
            {
                return;
            }

            // After the initial load. If we have entities this will trigger the change tracking to bump the version
            // Any scene with a version of 0 is considered being loaded.
            // With no entities we will never get that version bump so we start at version 1.
            var initialVersion = scene.EntityCount(m_WorldManager.EntityManager) == 0 ? 1 : 0;
            workspaceScenes.Add(new WorkspaceScenes { Scene = scene, ChangeVersion = initialVersion });
            SetActiveScene(scene);
        }

        public void MoveSceneUp(Scene scene)
        {
            var workspaceScenes = GetWorkspaceScenesRW();

            var index = IndexOf(workspaceScenes, scene);
            if (index < 0 || index == 0 || workspaceScenes.Length <= 1)
            {
                return;
            }
            Swap(workspaceScenes, index, index - 1);
        }

        public void MoveSceneDown(Scene scene)
        {
            var workspaceScenes = GetWorkspaceScenesRW();
            var index = IndexOf(workspaceScenes, scene);
            if (index < 0 || index == workspaceScenes.Length - 1 || workspaceScenes.Length <= 1)
            {
                return;
            }
            Swap(workspaceScenes, index, index + 1);
        }
        
        public void UnloadSceneWithDialog(Scene scene)
        {
            if (IsSceneChanged(scene))
            {
                var dialogResult = EditorUtility.DisplayDialogComplex(
                    $"Save Scene",
                    $"There are unsaved changes in the Scene, do you want to save?",
                    "Yes",
                    "No",
                    "Cancel");
            
                switch (dialogResult)
                {
                    case 0:
                        // Yes: Save and continue closing the project
                        var assetPath = m_Persistence.GetSceneAssetPath(scene);
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            m_Persistence.SaveScene(m_WorldManager.EntityManager, scene, assetPath);
                        }
                        break;
                    case 1:
                        // No: Don't save and continue closing the project
                        break;
                        
                    case 2: 
                        // Cancel: Opt out, the user has canceled the operation
                        return;
                }

            }
            UnloadScene(scene);
        }
        
        public void UnloadScene(Scene scene)
        {
            var workspaceScenes = GetWorkspaceScenesRW();
            if (!Contains(workspaceScenes, scene))
            {
                return;
            }
            
            m_Graphs.Remove(scene.SceneGuid.Guid);

            SceneManager.Destroy(m_WorldManager.EntityManager, scene);

            // !! IMPORTANT !!
            // SceneManager.Destroy will trigger a structural change on the world. 
            // This invalidates the DynamicBuffer and it must be be reacquired.
            workspaceScenes = GetWorkspaceScenesRW();
            
            var currentIndex = IndexOf(workspaceScenes, scene);
            workspaceScenes.RemoveAt(currentIndex);

            if (GetActiveScene() == scene)
            {
                SetActiveScene(workspaceScenes.Length == 0 ? Scene.Null : workspaceScenes[workspaceScenes.Length - 1].Scene);
            }
        }

        public void UnloadAllScenesByGuid(SceneGuid sceneGuid)
        {
            using (var scenes = GetAllLoadedScenesByGuid(sceneGuid))
            {
                for (var i = 0; i < scenes.Length; i++)
                {
                    UnloadScene(scenes[i]);
                }
            }
        }

        public string GetSceneName(Scene scene)
        {
            return m_Persistence.GetSceneAssetName(scene);
        }

        /// <summary>
        /// Returns true if any instance of the given <see cref="SceneGuid"/> are loaded.
        /// </summary>
        public bool IsAnyInstanceOfSceneLoaded(Guid sceneGuid)
        {
            var workspaceScenes = GetWorkspaceScenesRO();
            for (var i = 0; i < workspaceScenes.Length; i++)
            {
                if (workspaceScenes[i].Scene.SceneGuid.Guid == sceneGuid)
                {
                    return true;
                }
            }

            return false;
        }

        public Scene GetFirstInstanceOfSceneLoaded(Guid sceneGuid)
        {
            var workspaceScenes = GetWorkspaceScenesRO();
            for (var i = 0; i < workspaceScenes.Length; i++)
            {
                if (workspaceScenes[i].Scene.SceneGuid.Guid == sceneGuid)
                {
                    return workspaceScenes[i].Scene;
                }
            }

            return Scene.Null;
        }

        public bool IsSceneChanged(Scene scene)
        {
            var workspaceScenes = GetWorkspaceScenesRO();
            var index = IndexOf(workspaceScenes, scene);
            if (index == -1)
            {
                return false;
            }

            return workspaceScenes[index].ChangeVersion > 1;
        }

        /// <summary>
        /// Returns all loaded scenes with the given <see cref="SceneGuid"/>.
        /// </summary>
        private NativeList<Scene> GetAllLoadedScenesByGuid(SceneGuid sceneGuid, Allocator allocator = Allocator.Temp)
        {
            var workspaceScenes = GetWorkspaceScenesRO();
            var list = new NativeList<Scene>(1, allocator);

            for (var i = 0; i < workspaceScenes.Length; i++)
            {
                if (workspaceScenes[i].Scene.SceneGuid.Guid == sceneGuid.Guid)
                {
                    list.Add(workspaceScenes[i].Scene);
                }
            }

            return list;
        }

        public SceneSaveScope IgnoreSceneImport() => new SceneSaveScope(this);

        private void HandleSceneImported(SceneAsset asset, PostprocessEventArgs args)
        {
            if (Entity.Null == m_WorldManager.GetConfigEntity())
            {
                return;
            }
            
            if (m_SceneSaveScope > 0)
            {
                var sceneGuid = new SceneGuid {Guid = new Guid(asset.Guid)};
                var workspaceScenes = GetWorkspaceScenesRW();
                
                for (var i = 0; i < workspaceScenes.Length; i++)
                {
                    var workspaceScene = workspaceScenes[i];
                    if (workspaceScene.Scene.SceneGuid == sceneGuid)
                    {
                        workspaceScene.ChangeVersion = 1;
                    }
                    workspaceScenes[i] = workspaceScene;
                } 
                
                EntityHierarchyWindow.RepaintAll();
                return;
            }
            
            if (!IsAnyInstanceOfSceneLoaded(new Guid(asset.Guid)))
            {
                return;
            }

            if (EditorUtility.DisplayDialog(
                $"Scene asset has been changed",
                $"'{args.AssetPath}' has been changed. Would you like to reload the file?",
                "Yes",
                "No"))
            {
                LoadScene(args.AssetPath);
            }
        }

        private void HandleBeginChangeTracking()
        {
            m_ChangedScenes.Clear();
            m_RebuildScenes.Clear();
        }

        private void HandleEndChangeTracking()
        {
            foreach (var scene in m_RebuildScenes)
            {
                m_Graphs.Remove(scene.SceneGuid.Guid);	
                m_Graphs[scene.SceneGuid.Guid] = GetGraphForScene(scene.SceneGuid);
            }
        }

        private void HandleChanges(Changes changes)
        {
            if (m_EditorUndoManager.IsUndoRedoing || DomainReload.IsDomainReloading)
            {
                return;
            }

            var workspaceScenes = GetWorkspaceScenesRW();

            using (var pooled = HashSetPool<Scene>.GetDisposable())
            {
                var sceneSet = pooled.Set;
            
                foreach (var entityGuid in changes.AllChangedEntities())
                {
                    var entity = m_WorldManager.GetEntityFromGuid(entityGuid);

                    if (m_WorldManager.EntityManager.HasComponent<SceneGuid>(entity) &&
                        m_WorldManager.EntityManager.HasComponent<SceneInstanceId>(entity))
                    {
                        var sceneGuid = m_WorldManager.EntityManager.GetSharedComponentData<SceneGuid>(entity);
                        var sceneInstanceId = m_WorldManager.EntityManager.GetSharedComponentData<SceneInstanceId>(entity);
                    
                        if (ConfigurationScene.Guid == sceneGuid.Guid)
                        {
                            continue;
                        }

                        sceneSet.Add(new Scene(sceneGuid, sceneInstanceId));
                    }
                }

                if (sceneSet.Count > 0)
                {
                    foreach (var scene in sceneSet)
                    {
                        if (m_ChangedScenes.Add(scene))
                        {
                            var index = IndexOf(workspaceScenes, scene);
                            var workspaceScene = workspaceScenes[index];
                            workspaceScene.ChangeVersion++;
                            workspaceScenes[index] = workspaceScene;
                        }
                    }
                }
            }
            
            if (changes.EntitiesWereDeleted)
            {
                // Use the created inverse entities. Since we have component data for them.
                foreach (var scene in GetScenesForCreatedEntities(changes.InverseDiff))
                {
                    m_RebuildScenes.Add(scene);
                    
                    if (m_ChangedScenes.Add(scene))
                    {
                        var index = IndexOf(workspaceScenes, scene);
                        if (index != -1)
                        {
                            var workspaceScene = workspaceScenes[index];
                            workspaceScene.ChangeVersion++;
                            workspaceScenes[index] = workspaceScene;
                        }
                        else
                        {
                            m_Graphs.Remove(scene.SceneGuid.Guid);
                        }
                    }
                }
            }
            
            using (var pooledGuids = ListPool<Guid>.GetDisposable())
            {
                var guids = pooledGuids.List;
                guids.AddRange(changes.ChangedEntitiesWithSetComponent<SiblingIndex>());
                guids.AddRange(changes.ChangedEntitiesWithSetComponent<Parent>());

                if (guids.Count > 0)
                {
                    var entityManager = m_WorldManager.EntityManager;
                    
                    for (var i = 0; i < guids.Count; ++i)
                    {
                        var entityGuid = guids[i];
                        var entity = m_WorldManager.GetEntityFromGuid(entityGuid);
                        if (entityManager.HasComponent<SceneGuid>(entity) &&
                            entityManager.HasComponent<SceneInstanceId>(entity))
                        {
                            var sceneGuid = entityManager.GetSharedComponentData<SceneGuid>(entity);
                            var sceneInstanceId = entityManager.GetSharedComponentData<SceneInstanceId>(entity);

                            m_RebuildScenes.Add(new Scene(sceneGuid, sceneInstanceId));
                        }
                    }
                }
            }
        }

        private IEnumerable<Scene> GetScenesForCreatedEntities(WorldDiff diff)
        {
            var sharedSetCommands = diff.SharedSetCommands;
            
            for (var packedEntityIndex = 0; packedEntityIndex < diff.NewEntityCount; packedEntityIndex++)
            {
                var sceneGuid = new SceneGuid();
                var sceneInstanceId = new SceneInstanceId();

                for (var i = 0; i < sharedSetCommands.Length; i++)
                {
                    var cmd = sharedSetCommands[i];
                    
                    if (cmd.EntityIndex != packedEntityIndex)
                    {
                        continue;
                    }

                    var stableTypeHash = diff.TypeHashes[cmd.TypeHashIndex];

                    if (stableTypeHash == TypeManager.GetTypeInfo<SceneGuid>().StableTypeHash)
                    {
                        sceneGuid = (SceneGuid) cmd.BoxedSharedValue;
                    }
                    
                    if (stableTypeHash == TypeManager.GetTypeInfo<SceneInstanceId>().StableTypeHash)
                    {
                        sceneInstanceId = (SceneInstanceId) cmd.BoxedSharedValue;
                    }
                }

                if (sceneGuid.Guid != Guid.Empty)
                {
                    yield return new Scene(sceneGuid, sceneInstanceId);
                }
            }
        }

        private void HandleUndo()
        {
            RecreateAllGraphs();
        }

        private void RecreateAllGraphs()
        {
            m_Graphs.Clear();
            var workspaceScenes = GetWorkspaceScenesRO();
            for (var i = 0; i < workspaceScenes.Length; ++i)
            {
                var workspaceScene = workspaceScenes[i];
                m_Graphs[workspaceScene.Scene.SceneGuid.Guid] = new SceneGraph(m_Session, workspaceScene.Scene);
            }
        }

        private static void Swap(DynamicBuffer<WorkspaceScenes> scenes, int first, int second)
        {
            var temp = scenes[first];
            scenes[first] = scenes[second];
            scenes[second] = temp;
        }
        
        public static bool Contains(DynamicBuffer<WorkspaceScenes> buffer, Scene item)
        {
            for (var i = 0; i < buffer.Length; ++i)
            {
                if (buffer[i].Scene.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }
        
        public static int IndexOf(DynamicBuffer<WorkspaceScenes> buffer, Scene item)
        {
            for (var i = 0; i < buffer.Length; ++i)
            {
                if (buffer[i].Scene.Equals(item))
                {
                    return i;
                }
            }

            return -1;
        }
        
    }
}
