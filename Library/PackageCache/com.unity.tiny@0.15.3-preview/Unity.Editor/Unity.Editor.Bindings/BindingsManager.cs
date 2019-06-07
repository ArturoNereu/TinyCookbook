using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Authoring.Core;
using Unity.Authoring.Hashing;
using Unity.Authoring.Undo;
using Unity.Collections;
using Unity.Editor.Hierarchy;
using Unity.Editor.Undo;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Editor.Bindings
{
    internal class BindingsManager : SessionManager
    {
        private static class Cache
        {
            private static readonly BindingProfile[] s_AllProfiles;
            private static readonly Dictionary<BindingProfile, List<BindingProfile>> s_ProfileDependencies;

            public static BindingProfile[] AllProfiles => s_AllProfiles;
            public static Dictionary<BindingProfile, List<BindingProfile>> Dependencies => s_ProfileDependencies;

            static Cache()
            {
                s_ProfileDependencies = new Dictionary<BindingProfile, List<BindingProfile>>();
                var allProfiles = new List<BindingProfile>();

                var partialBindingTypeToInstance = new Dictionary<Type, IEntityBinding>();

                var types = new HashSet<Type>();
                var subTypes = new HashSet<Type>();
                var bindingsDependencies = new List<IEntityBinding>();
                var profileDependencies = new Dictionary<BindingProfile, List<IEntityBinding>>();
                var bindingInstanceToProfile = new Dictionary<IEntityBinding, BindingProfile>();

                var collection = UnityEditor.TypeCache.GetTypesDerivedFrom<IEntityBinding>()
                    .Where(t => !t.IsAbstract && !t.ContainsGenericParameters && !t.IsGenericType).ToList();
                RegisterBindingProfiles(collection, partialBindingTypeToInstance);

                for (var i = 0; i < collection.Count; ++i)
                {
                    var type = collection[i];

                    if (type.IsAbstract || type.ContainsGenericParameters)
                    {
                        continue;
                    }

                    var interfaces = type.GetInterfaces();
                    foreach (var @interface in interfaces)
                    {
                        if (!@interface.IsGenericType)
                        {
                            continue;
                        }

                        var genericTypeDefinition = @interface.GetGenericTypeDefinition();
                        var arguments = @interface.GetGenericArguments();
                        if (IsComponentBindingType(genericTypeDefinition))
                        {
                            foreach (var argument in arguments)
                            {
                                types.Add(argument);
                            }
                        }

                        if (IsSubtractiveBindingType(genericTypeDefinition))
                        {
                            foreach (var argument in arguments)
                            {
                                subTypes.Add(argument);
                            }
                        }

                        if (IsDependencyType(genericTypeDefinition))
                        {
                            foreach (var argument in arguments)
                            {
                                if (partialBindingTypeToInstance.TryGetValue(argument, out var instance))
                                {
                                    bindingsDependencies.Add(instance);
                                }
                            }
                        }
                    }

                    var profile = new BindingProfile(partialBindingTypeToInstance[type],
                        types.Select(TypeManager.GetTypeIndex).ToArray(), subTypes.Select(TypeManager.GetTypeIndex).ToArray());
                    allProfiles.Add(profile);
                    bindingInstanceToProfile.Add(partialBindingTypeToInstance[type], profile);
                    profileDependencies.Add(profile, new List<IEntityBinding>(bindingsDependencies));

                    // TODO: Check for test data
                    // ...

                    types.Clear();
                    subTypes.Clear();
                    bindingsDependencies.Clear();
                }

                // Set up dependencies
                foreach (var kvp in profileDependencies)
                {
                    var profile = kvp.Key;
                    var dependencies = kvp.Value;

                    s_ProfileDependencies.Add(profile,
                        new List<BindingProfile>(dependencies.Select(d => bindingInstanceToProfile[d])));
                }

                s_AllProfiles = allProfiles.ToArray();
            }

            private static bool IsComponentBindingType(Type type)
            {
                return type == typeof(IComponentBinding<>)
                       || type == typeof(IComponentBinding<,>)
                       || type == typeof(IComponentBinding<,,>)
                       || type == typeof(IComponentBinding<,,,>)
                       || type == typeof(IComponentBinding<,,,,>)
                       || type == typeof(IComponentBinding<,,,,,>)
                       || type == typeof(IComponentBinding<,,,,,,>)
                       || type == typeof(IComponentBinding<,,,,,,,>);
            }

            private static bool IsSubtractiveBindingType(Type type)
            {
                return type == typeof(IExcludeComponentBinding<>)
                       || type == typeof(IExcludeComponentBinding<,>)
                       || type == typeof(IExcludeComponentBinding<,,>)
                       || type == typeof(IExcludeComponentBinding<,,,>)
                       || type == typeof(IExcludeComponentBinding<,,,,>)
                       || type == typeof(IExcludeComponentBinding<,,,,,>)
                       || type == typeof(IExcludeComponentBinding<,,,,,,>)
                       || type == typeof(IExcludeComponentBinding<,,,,,,,>);
            }

            private static bool IsDependencyType(Type type)
            {
                return type == typeof(IBindingDependency<>)
                       || type == typeof(IBindingDependency<,>)
                       || type == typeof(IBindingDependency<,,>)
                       || type == typeof(IBindingDependency<,,,>)
                       || type == typeof(IBindingDependency<,,,,>)
                       || type == typeof(IBindingDependency<,,,,,>)
                       || type == typeof(IBindingDependency<,,,,,,>)
                       || type == typeof(IBindingDependency<,,,,,,,>);
            }

            private static void RegisterBindingProfiles(List<Type> collection,
                Dictionary<Type, IEntityBinding> typeToBindings)
            {
                foreach (var type in collection)
                {
                    var profile = (IEntityBinding)Activator.CreateInstance(type);
                    typeToBindings.Add(type, profile);
                }
            }
        }

        private struct Pool
        {
            public static void Clear()
            {
                MatchingProfiles.Clear();
                Dependencies.Clear();
                OrderedProfiles.Clear();
                DistinctProfiles.Clear();
                Result.Clear();
                ValidProfiles.Clear();
                KeptProfiles.Clear();
            }
            public static readonly List<BindingProfile> MatchingProfiles = new List<BindingProfile>(20);
            public static readonly List<BindingProfile> Dependencies = new List<BindingProfile>(20);
            public static readonly List<BindingProfile> OrderedProfiles = new List<BindingProfile>(20);
            public static readonly HashSet<BindingProfile> DistinctProfiles = new HashSet<BindingProfile>();
            public static readonly List<BindingProfile> ValidProfiles = new List<BindingProfile>(20);
            public static readonly List<BindingProfile> KeptProfiles = new List<BindingProfile>(20);
            public static readonly List<BindingProfile> Result = new List<BindingProfile>(20);
        }

        private Dictionary<EntityGuid, BindingConfiguration> EntityToBindingConfiguration { get; } = new Dictionary<EntityGuid, BindingConfiguration>();
        private List<BindingConfiguration> TransferConfigurations { get; } = new List<BindingConfiguration>();
        private HashSet<EntityGuid> DirtyConfigurations { get; } = new HashSet<EntityGuid>();

        private UnityComponentCacheManager m_ComponentCache;
        private IWorldManagerInternal m_WorldManager;
        private IArchetypeManager m_ArchetypeManager;
        private IEditorSceneManagerInternal m_SceneManager;
        private IEditorUndoManager m_Undo;
        private BindingContext m_BindingsContext;
        private NativeHashMap<EntityGuid, EntityGuid> m_TransferMap;

        private bool m_UndoRedoing;
        //private bool m_IncludeTestTypes;

        public BindingsManager(Session session) : base(session)
        {
        }

        public override void Load()
        {
            m_BindingsContext = new BindingContext(Session);
            m_Undo = Session.GetManager<IEditorUndoManager>();
            m_Undo.UndoRedoBatchStarted += HandleUndoRedoBatchStarted;
            m_Undo.UndoRedoBatchEnded += HandleUndoRedoBatchEnded;

            m_WorldManager = Session.GetManager<IWorldManagerInternal>();
            m_ArchetypeManager = Session.GetManager<IArchetypeManager>();
            m_SceneManager = Session.GetManager<IEditorSceneManagerInternal>();
            m_ComponentCache = Session.GetManager<UnityComponentCacheManager>();
            Session.GetManager<IChangeManager>().RegisterChangeCallback(HandleChanges, int.MaxValue);
            UnityEditor.Undo.postprocessModifications += HandleInvertedChanges;
            EditorApplication.hierarchyChanged += HandleHierarchyChanges;
            m_TransferMap = new NativeHashMap<EntityGuid, EntityGuid>(1024, Allocator.Persistent);
        }

        public override void Unload()
        {
            m_Undo.UndoRedoBatchStarted -= HandleUndoRedoBatchStarted;
            m_Undo.UndoRedoBatchEnded -= HandleUndoRedoBatchEnded;
            m_TransferMap.Dispose();
            Session.GetManager<IChangeManager>().UnregisterChangeCallback(HandleChanges);

            m_WorldManager = null;
            UnityEditor.Undo.postprocessModifications -= HandleInvertedChanges;
            EditorApplication.hierarchyChanged -= HandleHierarchyChanges;
        }

        private void Transfer(NativeArray<EntityGuid> guids)
        {
            var entities = new NativeArray<Entity>(guids.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            try
            {
                for (var i = 0; i < guids.Length; ++i)
                {
                    entities[i] = m_WorldManager.GetEntityFromGuid(guids[i].ToGuid());
                }
                Transfer(entities, guids);
            }
            finally
            {
                entities.Dispose();
            }
        }

        private void Transfer(NativeArray<Entity> entities, NativeArray<EntityGuid> guids)
        {
            TransferConfigurations.Clear();
            if (entities.Length > TransferConfigurations.Capacity)
            {
                TransferConfigurations.Capacity = entities.Length;
            }

            for (var i = 0; i < entities.Length; ++i)
            {
                var entity = entities[i];
                var guid = guids[i];

                if (!EntityToBindingConfiguration.TryGetValue(guid, out var current))
                {
                }
                var configuration = current;

                if (default == configuration || DirtyConfigurations.Contains(guid))
                {
                    configuration = GenerateBindingConfiguration(entity);
                    DirtyConfigurations.Remove(guid);
                    EntityToBindingConfiguration[guid] = configuration;
                }

                TransferConfigurations.Add(configuration);
                if (default != current && !configuration.Equals(current))
                {
                    for (var inverted = current.Bindings.Length - 1; inverted >= 0; --inverted)
                    {
                        var profile = current.Bindings[inverted];
                        if (Array.IndexOf(configuration.Bindings, profile) < 0)
                        {
                            profile.UnloadBinding(entity, m_BindingsContext);
                        }
                    }
                }
            }

            for (var i = 0; i < entities.Length; ++i)
            {
                var entity = entities[i];
                var configuration = TransferConfigurations[i];
                foreach (var profile in configuration.Bindings)
                {
                    profile.LoadBinding(entity, m_BindingsContext);
                }
            }

            for (var i = 0; i < entities.Length; ++i)
            {
                var entity = entities[i];
                var configuration = TransferConfigurations[i];
                foreach (var profile in configuration.Bindings)
                {
                    profile.TransferToUnity(entity, m_BindingsContext);
                }
            }
        }

        public void TransferBack(NativeArray<Guid> guids)
        {
            var entities = new NativeArray<Entity>(guids.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            try
            {
                for (var i = 0; i < guids.Length; ++i)
                {
                    entities[i] = m_WorldManager.GetEntityFromGuid(guids[i]);
                }
                TransferBack(entities, guids);
            }
            finally
            {
                entities.Dispose();
            }
        }

        public void TransferBack(NativeArray<Entity> entities, NativeArray<Guid> guids)
        {
            for (var i = 0; i < entities.Length; ++i)
            {
                var entity = entities[i];
                if (!m_WorldManager.EntityManager.Exists(entity))
                {
                    continue;
                }
                var guid = guids[i].ToEntityGuid();
                if (!EntityToBindingConfiguration.TryGetValue(guid, out var configuration))
                {
                    continue;
                }
                foreach (var profile in configuration.Bindings)
                {
                    profile.TransferFromUnity(entity, m_BindingsContext);
                }
            }
        }

        private BindingConfiguration GenerateBindingConfiguration(Entity entity)
        {
            Pool.Clear();

            // Get all the matching profiles
            GetMatchingProfiles(entity, Pool.MatchingProfiles);

            // In order
            foreach (var profile in Pool.MatchingProfiles)
            {
                EnumerateDependencies(profile, Pool.Dependencies);

                for (var i = Pool.Dependencies.Count - 1; i >= 0; --i)
                {
                    var orderedProfile = Pool.Dependencies[i];
                    Pool.OrderedProfiles.Add(orderedProfile);
                }
            }

            // Use each one only once
            foreach (var profile in Pool.OrderedProfiles)
            {
                if (Pool.DistinctProfiles.Add(profile))
                {
                    Pool.Result.Add(profile);
                }
            }

            return new BindingConfiguration(Pool.Result.ToArray());
        }

        private void GetMatchingProfiles(Entity entity, List<BindingProfile> result)
        {
            GetProfiles(entity, Pool.ValidProfiles/*, m_IncludeTestTypes*/);

            bool removed;
            do
            {
                removed = false;
                foreach (var profile in Pool.ValidProfiles)
                {
                    var keep = true;
                    foreach (var dependency in GetDependencies(profile))
                    {
                        if (!Pool.ValidProfiles.Contains(dependency))
                        {
                            keep = false;
                            removed = true;
                        }
                    }

                    if (keep)
                    {
                        Pool.KeptProfiles.Add(profile);
                    }
                }

                if (removed)
                {
                    Pool.ValidProfiles.Clear();
                    Pool.ValidProfiles.AddRange(Pool.KeptProfiles);
                    Pool.KeptProfiles.Clear();
                }
            } while (removed);

            result.AddRange(Pool.KeptProfiles);
        }

        private void GetProfiles(Entity entity, List<BindingProfile> result, bool includeTestBindings = false)
        {
            foreach (var profile in Cache.AllProfiles)
            {
                // TODO: Bring back test types
                //                if (!includeTestBindings && s_ContainsTestData.Contains(profile))
                //                {
                //                    continue;
                //                }
                if (profile.IsValidBinding(m_WorldManager.EntityManager, entity))
                {
                    result.Add(profile);
                }
            }
        }

        private static void EnumerateDependencies(BindingProfile profile, List<BindingProfile> dependencies)
        {
            dependencies.Add(profile);
            foreach (var dependence in GetDependencies(profile))
            {
                EnumerateDependencies(dependence, dependencies);
            }
        }

        private static IEnumerable<BindingProfile> EnumerateDependencies(BindingProfile profile)
        {
            yield return profile;

            foreach (var dependence in GetDependencies(profile).SelectMany(EnumerateDependencies))
            {
                yield return dependence;
            }
        }

        private static List<BindingProfile> GetDependencies(BindingProfile profile)
        {
            if (!Cache.Dependencies.TryGetValue(profile, out var dependencies))
            {
                dependencies = new List<BindingProfile>();
            }

            return dependencies;
        }

        private void HandleChanges(Changes changes)
        {
            m_TransferMap.Clear();

            var worldDiff = changes.WorldDiff;
            foreach (var diff in worldDiff.SetCommands)
            {
                var guid = worldDiff.Entities[diff.EntityIndex];
                m_TransferMap.TryAdd(guid, guid);
            }

            foreach (var diff in worldDiff.AddComponents)
            {
                var guid = worldDiff.Entities[diff.EntityIndex];
                DirtyConfigurations.Add(guid);
                m_TransferMap.TryAdd(guid, guid);
            }

            foreach (var diff in worldDiff.RemoveComponents)
            {
                var guid = worldDiff.Entities[diff.EntityIndex];
                DirtyConfigurations.Add(guid);
                m_TransferMap.TryAdd(guid, guid);
            }

            if (!m_UndoRedoing)
            {
                TransferCurrentMap();
            }
        }

        private void HandleUndoRedoBatchStarted()
        {
            m_UndoRedoing = true;
        }

        private void HandleUndoRedoBatchEnded()
        {
            m_UndoRedoing = false;
            TransferCurrentMap();
        }

        private void TransferCurrentMap()
        {
            using (var guidArray = m_TransferMap.GetValueArray(Allocator.TempJob))
            {
                Transfer(guidArray);
            }
        }

        private UndoPropertyModification[] HandleInvertedChanges(UndoPropertyModification[] mods)
        {
            using (var pooledModifications = ListPool<Guid>.GetDisposable())
            using (var pooledNewReferences = ListPool<Guid>.GetDisposable())
            {
                var modifications = pooledModifications.List;
                var newReferences = pooledNewReferences.List;

                modifications.AddRange(mods.Select(m => m.currentValue?.target)
                    .Select(o =>
                    {
                        if (!o)
                        {
                            return Guid.Empty;
                        }

                        var go = (o is GameObject gameObj) ? gameObj : ((o is Component comp) ? comp.gameObject : null);
                        if (go)
                        {
                            var reference = go.GetComponent<EntityReference>();
                            // 
                            if (!reference || null == reference)
                            {
                                reference = go.AddComponent<EntityReference>();
                                reference.Guid = Guid.NewGuid();
                                CreateEntityFromGameObject(go, reference);
                                newReferences.Add(reference.Guid);
                                return reference.Guid;
                            }

                            return reference.Guid;
                        }

                        return Guid.Empty;
                    })
                    .Where(g => g != Guid.Empty)
                    .Distinct());

                if (modifications.Count > 0)
                {
                    m_WorldManager.RebuildGuidCache();

                    var array = new NativeArray<Guid>(modifications.Count, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);
                    try
                    {
                        for (int i = 0; i < modifications.Count; ++i)
                        {
                            array[i] = modifications[i];
                        }

                        TransferBack(array);
                    }
                    finally
                    {
                        array.Dispose();
                    }

                    if (newReferences.Count > 0)
                    {
                        EntityHierarchyWindow.SelectOnNextPaint(newReferences);
                    }
                }

                return mods;
            }
        }

        private void CreateEntityFromGameObject(GameObject go, EntityReference reference)
        {
            // create the entity
            var entity = m_WorldManager.CreateEntity(go.name, m_ArchetypeManager.FromGameObject(go));
            m_WorldManager.SetEntityGuid(entity, reference.Guid);
            m_WorldManager.EntityManager.SetComponentData(entity, DomainCache.GetDefaultValue<SiblingIndex>());

            // add it to the currently active scene
            var scene = m_SceneManager.GetActiveScene();
            scene.AddEntityReference(m_WorldManager.EntityManager, entity);

            // prime the entity binding configuration (usually primed when running bindings the other way around)
            var bindingConfig = GenerateBindingConfiguration(entity);
            EntityToBindingConfiguration[reference.Guid.ToEntityGuid()] = bindingConfig;

            // create the link between the existing GO and the new entity
            m_ComponentCache.CreateLink(reference.Guid, reference);
        }

        private void HandleHierarchyChanges()
        {
            for (var i = 0; i < SceneManager.sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || !scene.IsValid())
                {
                    continue;
                }

                var transforms = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Where(t => null != t && t)
                    // Dealing with prefabs
                    .Where(t =>  (t.root.gameObject.hideFlags & HideFlags.HideInHierarchy) != HideFlags.HideInHierarchy)
                    .Where(t =>
                    {
                        var reference = t.GetComponent<EntityReference>();
                        return null == reference || !reference || reference.Guid == Guid.Empty;
                    });

                ProcessNewGameObjects(transforms);
            }
        }

        private void ProcessNewGameObjects(IEnumerable<Transform> transforms)
        {
            using (var pooledNewReferences = ListPool<Guid>.GetDisposable())
            {
                var newReferences = pooledNewReferences.List;
                foreach (var t in transforms)
                {
                    var go = t.gameObject;
                    var status = PrefabUtility.GetPrefabInstanceStatus(go);
                    if (status != PrefabInstanceStatus.NotAPrefab)
                    {
                        // TODO: handle dragging prefabs
                    }
                    else
                    {
                        var reference = go.GetComponent<EntityReference>();
                        if (!reference)
                            reference = go.AddComponent<EntityReference>();

                        reference.Guid = Guid.NewGuid();
                        
                        CreateEntityFromGameObject(go, reference);

                        newReferences.Add(reference.Guid);
                    }
                }

                m_WorldManager.RebuildGuidCache();

                var array = new NativeArray<Guid>(newReferences.Count, Allocator.Temp,
                    NativeArrayOptions.UninitializedMemory);
                try
                {
                    for (int i = 0; i < newReferences.Count; ++i)
                    {
                        array[i] = newReferences[i];
                    }

                    TransferBack(array);
                }
                finally
                {
                    array.Dispose();
                }

                if (newReferences.Count > 0)
                {
                    EntityHierarchyWindow.SelectOnNextPaint(newReferences);
                }
            }
        }
    }
}
