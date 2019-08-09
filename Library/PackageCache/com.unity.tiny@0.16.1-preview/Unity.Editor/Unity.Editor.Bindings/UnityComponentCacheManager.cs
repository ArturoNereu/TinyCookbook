using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Collections;
using Unity.Editor.Extensions;
using Unity.Editor.Undo;
using Unity.Entities;
using Unity.Tiny.Core2D;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Editor.Bindings
{
    internal interface IUnityComponentCacheManager : ISessionManager
    {
        EntityReference GetEntityReference(Entity entity);
        EntityReference GetEntityReference(Guid guid);
    }

    [InitializeOnLoad]
    internal class UnityComponentCacheManager : ISessionManagerInternal, IUnityComponentCacheManager
    {
        private const string k_EditorCacheFolderName = "EditorCache";
        private const string k_SceneScratchPadName = "DotsScratchpad-DoNotEdit.unity";
        private const string k_ScratchPadDirectory = "Assets/" + k_EditorCacheFolderName;
        private const string k_ScratchPadPath = k_ScratchPadDirectory + "/" + k_SceneScratchPadName;
        private static bool ReloadingAssemblies;

        private readonly Dictionary<Guid, EntityReference> k_References = new Dictionary<Guid, EntityReference>();
        private readonly Dictionary<Guid, List<Component>> k_ComponentCache = new Dictionary<Guid, List<Component>>();
        private IWorldManager m_WorldManager;
        private EntityManager m_EntityManager;
        private IEditorSceneManagerInternal m_SceneManager;
        private IChangeManager m_ChangeManager;
        private IEditorUndoManager m_Undo;
        private UnityEngine.SceneManagement.Scene m_Scene;

        static UnityComponentCacheManager()
        {
            AssemblyReloadEvents.beforeAssemblyReload += HandleAssemblyWillUnload;
            AssemblyReloadEvents.afterAssemblyReload += HandleAssemblyReloaded;
        }

        public void Load(Session session)
        {
            LoadUnityScratchPadScene();
            m_WorldManager = session.GetManager<IWorldManager>();
            m_EntityManager = m_WorldManager.EntityManager;
            m_SceneManager = session.GetManager<IEditorSceneManagerInternal>();
            m_ChangeManager = session.GetManager<IChangeManager>();
            m_ChangeManager.RegisterChangeCallback(HandleChanges, int.MinValue);
            m_Undo = session.GetManager<IEditorUndoManager>();
            m_Undo.UndoRedoBatchEnded += HandleUndoEnded;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpening += HandleUnitySceneOpening;
        }

        public void Unload(Session session)
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpening -= HandleUnitySceneOpening;

            m_Undo.UndoRedoBatchEnded -= HandleUndoEnded;

            m_ChangeManager.UnregisterChangeCallback(HandleChanges);

            foreach (var go in new HashSet<GameObject>(k_References.Values.Select(r => r.transform.root.gameObject)))
            {
                Object.DestroyImmediate(go);
            }
            k_References.Clear();
            k_ComponentCache.Clear();

            if (!ReloadingAssemblies)
            {
                UnityEditor.SceneManagement.EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        public EntityReference GetEntityReference(Entity entity)
        {
            var guid = m_WorldManager.GetEntityGuid(entity);
            if (guid == Guid.Empty)
            {
                return null;
            }

            k_References.TryGetValue(guid, out var reference);
            return reference;
        }

        public EntityReference GetEntityReference(Guid guid)
        {
            k_References.TryGetValue(guid, out var reference);
            return reference;
        }

        private void DeleteLinks(NativeArray<Guid> guids)
        {
            using (var pooled = ListPool<EntityReference>.GetDisposable())
            {
                var references = pooled.List;
                foreach (var guid in guids)
                {
                    if (k_References.TryGetValue(guid, out var reference))
                    {
                        references.Add(reference);
                    }
                }

                Delete(references);
            }
        }

        private void ReparentLinks(NativeArray<Guid> guids)
        {
            foreach (var guid in guids)
            {
                var entity = m_WorldManager.GetEntityFromGuid(guid);
                if (!m_EntityManager.Exists(entity))
                {
                    continue;
                }
                var reference = GetEntityReference(guid);
                if (!reference || null == reference)
                {
                    continue;
                }

                if (m_EntityManager.HasComponent<Parent>(entity))
                {
                    var parent = m_EntityManager.GetComponentData<Parent>(entity).Value;
                    var parentReference = GetEntityReference(parent);
                    if (parentReference && null != parentReference)
                    {
                        reference.transform.SetParent(parentReference.transform, true);
                    }
                    else
                    {
                        reference.transform.SetParent(null, true);
                    }
                }
                else
                {
                    reference.transform.SetParent(null, true);
                }
            }
        }

        private void SetEnabledState(NativeArray<Guid> guids)
        {
            foreach (var guid in guids)
            {
                var entity = m_WorldManager.GetEntityFromGuid(guid);
                if (!m_EntityManager.Exists(entity))
                {
                    continue;
                }

                var reference = GetEntityReference(guid);
                if (!reference || null == reference)
                {
                    continue;
                }

                reference.gameObject.SetActive(!m_EntityManager.HasComponent<Disabled>(entity));
            }
        }
        
        public void CreateLink(Guid guid, EntityReference reference)
        {
            if (k_References.TryGetValue(guid, out var existingReference))
            {
                if (object.ReferenceEquals(reference, existingReference))
                    return;

                throw new ArgumentException("Duplicate entity reference on GUID " + guid.ToString("N"));
            }

            reference.OnDestroyed += OnEntityReferenceDestroyed;
            reference.gameObject.hideFlags |= HideFlags.DontSaveInEditor;
            k_References.Add(guid, reference);
        }

        public void CreateLink(Entity entity, Guid guid)
        {
            if (k_References.TryGetValue(guid, out var reference))
            {
                return;
            }

            var go = new GameObject(m_WorldManager.GetEntityName(entity));
            reference = go.AddComponent<EntityReference>();

            reference.Guid = guid;
            reference.OnDestroyed += OnEntityReferenceDestroyed;

            go.hideFlags |= HideFlags.DontSaveInEditor;
            k_References.Add(guid, reference);
        }

        public void CreateLink(Guid guid)
        {
            if (k_References.TryGetValue(guid, out var reference))
            {
                return;
            }

            var entity = m_WorldManager.GetEntityFromGuid(guid);
            if (entity != Entity.Null)
            {
                CreateLink(entity, guid);
            }
        }

        public TComponent AddComponent<TComponent>(Entity entity)
            where TComponent : Component
        {
            var guid = m_WorldManager.GetEntityGuid(entity);
            if (guid == Guid.Empty)
            {
                return null;
            }

            if (!k_References.TryGetValue(guid, out var reference))
            {
                // Wuuut
                return null;
            }

            var component = reference.gameObject.AddComponent<TComponent>();
            if (!k_ComponentCache.TryGetValue(guid, out var list))
            {
                k_ComponentCache[guid] = list = new List<Component>();
            }
            list.Add(component);
            return component;
        }

        public TComponent GetComponent<TComponent>(Entity entity)
            where TComponent : Component
        {
            var guid = m_WorldManager.GetEntityGuid(entity);
            if (guid == Guid.Empty)
            {
                return null;
            }

            if (!k_References.TryGetValue(guid, out var reference))
            {
                // Wuuut
                return null;
            }

            TComponent component = default;
            if (!k_ComponentCache.TryGetValue(guid, out var list))
            {
                component = reference.GetComponent<TComponent>();
                if (!IsValid(component))
                {
                    return null;
                }
                k_ComponentCache[guid] = new List<Component> { component };
                return component;
            }

            for (var i = 0; i < list.Count; ++i)
            {
                var c = list[i];
                if (c is TComponent typed)
                {
                    component = typed;
                }
            }

            if (IsValid(component))
            {
                return component;
            }

            if (null != component) // fake-null
            {
                list.Remove(component);
            }

            component = reference.GetComponent<TComponent>();
            if (IsValid(component))
            {
                list.Add(component);
            }

            return component;
        }

        public void RemoveComponent<TComponent>(Entity entity)
            where TComponent : Component
        {
            var guid = m_WorldManager.GetEntityGuid(entity);
            if (guid == Guid.Empty)
            {
                return;
            }

            if (!k_References.TryGetValue(guid, out var reference))
            {
                // Wuuut
                return;
            }
            if (!k_ComponentCache.TryGetValue(guid, out var list))
            {
                return;
            }

            var component = list.OfType<TComponent>().FirstOrDefault();
            if (IsValid(component))
            {
                if (list.Remove(component) && list.Count == 0)
                {
                    k_ComponentCache.Remove(guid);
                }

                // We can't remove a Transform.
                if (component is Transform)
                {
                    return;
                }
                Object.DestroyImmediate(component, false);
            }
        }

        private void OnEntityReferenceDestroyed(Guid guid)
        {
            k_References.Remove(guid);
            k_ComponentCache.Remove(guid);
            var entity = m_WorldManager.GetEntityFromGuid(guid);
            if (m_EntityManager.Exists(entity))
            {
                m_EntityManager.DestroyEntity(entity);
            }
        }


        private void HandleChanges(Changes changes)
        {
            foreach (var guid in changes.CreatedEntities())
            {
                CreateLink(guid);
            }

            using(var entities = new NativeArray<Guid>(changes.DeletedEntities().ToArray(), Allocator.Temp))
            {
                DeleteLinks(entities);
            }

            using (var entities = new NativeArray<Guid>(changes.ReparentedEntities().ToArray(), Allocator.Temp))
            {
                ReparentLinks(entities);
            }

            using (var entities = new NativeArray<Guid>(changes.EnabledStateChanged().ToArray(), Allocator.Temp))
            {
                SetEnabledState(entities);
            }
        }

        private static bool IsValid<TComponent>(TComponent component)
            where TComponent : Component
        {
            return component && null != component;
        }

        private void HandleUndoEnded()
        {
            RemoveBrokenEntityReferences(m_Scene);
        }

        private void LoadUnityScratchPadScene()
        {
            if ((!m_Scene.isLoaded || !m_Scene.IsValid()) && !Bridge.EditorApplication.IsQuitting)
            {
                m_Scene = GetOrGenerateScratchPad();
            }
                
            // Make sure the scratch pad is clean if we somehow have saved gameObjects.
            foreach (var obj in m_Scene.GetRootGameObjects())
            {
                Object.DestroyImmediate(obj);
            }
        }

        private static UnityEngine.SceneManagement.Scene GetOrGenerateScratchPad()
        {
            if (!AssetDatabase.IsValidFolder(k_ScratchPadDirectory))
            {
                AssetDatabase.CreateFolder("Assets", k_EditorCacheFolderName);
            }

            UnityEngine.SceneManagement.Scene scene;
            var asset = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(k_ScratchPadPath);
            if (null != asset)
            {
                scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(k_ScratchPadPath);
                if (!scene.isLoaded)
                {
                    scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(k_ScratchPadPath, UnityEditor.SceneManagement.OpenSceneMode.Single);
                }
                return scene;
            }
            else
            {
                scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(k_ScratchPadPath);
                if (scene.isLoaded)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, k_ScratchPadPath);
                    return scene;
                }
            }

            scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, k_ScratchPadPath);
            return scene;
        }


        private static void HandleAssemblyWillUnload()
        {
            ReloadingAssemblies = true;
        }

        private static void HandleAssemblyReloaded()
        {
            ReloadingAssemblies = false;
        }

        private static void HandleUnitySceneOpening(string path, OpenSceneMode mode)
        {
            Application.SetAuthoringProject(null);
        }

        private static void RemoveBrokenEntityReferences(UnityEngine.SceneManagement.Scene scene)
        {
            if (!scene.isLoaded || !scene.IsValid())
            {
                return;
            }

            using (var pooledToDelete = ListPool<EntityReference>.GetDisposable())
            using (var pooledRoots = ListPool<GameObject>.GetDisposable())
            {
                var toDelete = pooledToDelete.List;
                var roots = pooledRoots.List;
                scene.GetRootGameObjects(roots);
                foreach (var root in roots)
                {
                    using (var pooledReferences = ListPool<EntityReference>.GetDisposable())
                    {
                        var references = pooledReferences.List;
                        root.GetComponentsInChildren(true, references);
                        toDelete.AddRange(references.Where(r => r.Guid == Guid.Empty));
                    }
                }

                Delete(toDelete);
            }
        }

        private static void Delete(List<EntityReference> toDelete)
        {
            using (var pooledGo = ListPool<GameObject>.GetDisposable())
            {
                pooledGo.List.AddRange(toDelete.Select(r => r.gameObject));
                foreach (var go in pooledGo.List)
                {
                    if (go && null != go)
                    {
                        Object.DestroyImmediate(go);
                    }
                }
            }
        }
    }
}
