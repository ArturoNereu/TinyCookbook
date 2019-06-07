using System;
using System.Collections.Generic;
using Unity.Authoring;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Editor.Assets;
using Unity.Editor.Extensions;
using Unity.Editor.Persistence;
using Unity.Editor.Serialization;
using Unity.Entities;
using Unity.Properties;
using Assert = UnityEngine.Assertions.Assert;
using Object = UnityEngine.Object;

namespace Unity.Editor
{
    public interface IAssetManager : ISessionManager
    {
        Entity GetEntity(Object obj);
        T GetUnityObject<T>(Entity entity) where T : Object;
        Object GetUnityObject(Entity entity, Type type);
    }
    
    internal interface IAssetManagerInternal : IAssetManager
    {
        IReadOnlyDictionary<AssetInfo, Entity> EnumerateAssets(Project project);
        void Refresh();
    }

    internal class AssetManager : SessionManager, IAssetManagerInternal
    {
        private class AssetReferenceCache : IDisposable
        {
            private readonly NativeHashMap<AssetReference, Entity> m_Cache;

            public AssetReferenceCache()
            {
                m_Cache = new NativeHashMap<AssetReference, Entity>(32, Allocator.Persistent);
            }

            public bool TryGet(AssetReference reference, out Entity e)
            {
                return m_Cache.TryGetValue(reference, out e);
            }

            public bool TryAdd(AssetReference reference, Entity e)
            {
                return m_Cache.TryAdd(reference, e);
            }

            public void Dispose()
            {
                m_Cache.Dispose();
            }
        }

        private AssetReferenceCache m_Cache;
        private NativeHashMap<Entity, Entity> m_PostProcessRemap;

        private IWorldManager WorldManager { get; set; }
        private AssetImporter AssetImporter { get; set; }
        private EntityManager EntityManager => WorldManager.EntityManager;

        public AssetManager(Session session) : base(session)
        {
        }

        public override void Load()
        {
            WorldManager = Session.GetManager<IWorldManager>();
            Assert.IsNotNull(WorldManager);

            AssetImporter = new AssetImporter(Session);

            m_PostProcessRemap = new NativeHashMap<Entity, Entity>(8, Allocator.Persistent);

            foreach (var type in DomainCache.AssetImporterTypes.Keys)
            {
                AssetPostprocessorCallbacks.RegisterAssetImportedHandlerForType(type, HandleImportAsset);
            }

            AssetPostprocessorCallbacks.RegisterToPostProcessStarted(HandleBeginPostprocess);
            AssetPostprocessorCallbacks.RegisterToPostProcessEnded(HandleEndPostprocess);
        }

        public override void Unload()
        {
            m_PostProcessRemap.Dispose();

            foreach (var type in DomainCache.AssetImporterTypes.Keys)
            {
                AssetPostprocessorCallbacks.UnregisterAssetImportedHandlerForType(type, HandleImportAsset);
            }

            AssetPostprocessorCallbacks.UnregisterFromPostProcessStarted(HandleBeginPostprocess);
            AssetPostprocessorCallbacks.UnregisterFromPostProcessEnded(HandleEndPostprocess);
        }

        #region IAssetManager

        public IReadOnlyDictionary<AssetInfo, Entity> EnumerateAssets(Project project)
        {
            var assetInfos = AssetEnumerator.GetAllReferencedAssets(project);
            var assetEntities = new Dictionary<AssetInfo, Entity>();
            foreach (var assetInfo in assetInfos)
            {
                assetEntities.Add(assetInfo, GetEntity(assetInfo.Object));
            }
            return assetEntities;
        }

        public Entity GetEntity(Object obj)
        {
            if (obj == null || !obj)
            {
                return Entity.Null;
            }

            var reference = obj.ToAssetReference();

            if (TryGetEntity(reference, out var entity))
            {
                return entity;
            }

            entity = AssetImporter.Import(obj);
            m_Cache?.TryAdd(reference, entity);

            return entity;
        }

        public T GetUnityObject<T>(Entity entity) where T : Object
        {
            return (T)GetUnityObject(entity, typeof(T));
        }

        public Object GetUnityObject(Entity entity, Type type)
        {
            if (!EntityManager.Exists(entity) ||
                !EntityManager.HasComponent<AssetReference>(entity))
            {
                return null;
            }

            var obj = EntityManager.GetComponentData<AssetReference>(entity).ToUnityObject();
            return type.IsInstanceOfType(obj) ? obj : null;
        }

        public void Refresh()
        {
            if (GetAssetEntityCount() == 0)
            {
                return;
            }

            using (m_Cache = new AssetReferenceCache())
            using (var query = GetAssetReferenceQueryRO())
            using (var entities = query.ToEntityArray(Allocator.TempJob))
            using (var references = query.ToComponentDataArray<AssetReference>(Allocator.TempJob))
            using (var remap = new NativeHashMap<Entity, Entity>(entities.Length, Allocator.Temp))
            {
                // 1. Cleanup cache
                EntityManager.DestroyEntity(entities);

                for (var i = 0; i < references.Length; i++)
                {
                    var asset = references[i].ToUnityObject();

                    if (!asset || asset == null)
                    {
                        continue;
                    }

                    // Importing an asset will cause sub-assets to import as well.
                    // Make sure we are importing what is not already imported.
                    if (!m_Cache.TryGet(references[i], out var entity))
                    {
                        entity = AssetImporter.Import(asset);
                    }

                    remap.TryAdd(entities[i], entity);
                }

                // Remap the world
                RemapEntityReferences(remap);
            }

            m_Cache = null;
        }

        private bool TryGetEntity(AssetReference reference, out Entity entity)
        {
            entity = GetEntity(reference);
            return entity != Entity.Null;
        }

        private Entity GetEntity(AssetReference reference)
        {
            if (null != m_Cache)
            {
                return m_Cache.TryGet(reference, out var entity) ? entity : Entity.Null;
            }

            using (var query = GetAssetReferenceQueryRO())
            using (var entities = query.ToEntityArray(Allocator.TempJob))
            using (var references = query.ToComponentDataArray<AssetReference>(Allocator.TempJob))
            {
                for (var i = 0; i < references.Length; i++)
                {
                    if (reference == references[i])
                    {
                        return entities[i];
                    }
                }
            }

            return Entity.Null;
        }

        private int GetAssetEntityCount()
        {
            using (var query = GetAssetReferenceQueryRO())
            {
                return query.CalculateLength();
            }
        }

        internal EntityQuery GetAssetReferenceQueryRO()
        {
            var queryDesc = new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<AssetReference>() },
                Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled
            };

            return EntityManager.CreateEntityQuery(queryDesc);
        }

        private void HandleBeginPostprocess()
        {
            m_PostProcessRemap.Clear();
        }

        private void HandleImportAsset(Object asset, PostprocessEventArgs args)
        {
            var assets = new List<Object>();

            AddChildrenRecursive(assets, new AssetEnumerator(Session).GetAssetInfo(asset));

            var entities = new NativeArray<Entity>(assets.Count, Allocator.Temp);

            for (var i = 0; i < assets.Count; i++)
            {
                var obj = assets[i];

                if (TryGetEntity(obj.ToAssetReference(), out var entity))
                {
                    EntityManager.DestroyEntity(entity);
                }

                entities[i] = entity;
            }

            for (var i = 0; i < assets.Count; i++)
            {
                var entity = AssetImporter.Import(assets[i]);

                if (entities[i] != Entity.Null)
                {
                    m_PostProcessRemap.TryAdd(entities[i], entity);
                }
            }
        }

        private void HandleDeleteAsset(Object asset, PostprocessEventArgs args)
        {
            var assetWithChildren = new List<Object>();

            AddChildrenRecursive(assetWithChildren, new AssetEnumerator(Session).GetAssetInfo(asset));

            foreach (var obj in assetWithChildren)
            {
                if (TryGetEntity(obj.ToAssetReference(), out var entity))
                {
                    EntityManager.DestroyEntity(entity);
                }
            }
        }

        private void HandleEndPostprocess()
        {
            RemapEntityReferences(m_PostProcessRemap);
        }

        private static void AddChildrenRecursive(ICollection<Object> list, AssetInfo info)
        {
            list.Add(info.Object);

            foreach (var child in info.Children)
            {
                AddChildrenRecursive(list, child);
            }
        }

        private void RemapEntityReferences(NativeHashMap<Entity, Entity> remap)
        {
            if (remap.Length == 0)
            {
                return;
            }

            using (var entities = EntityManager.GetAllEntities(Allocator.TempJob))
            {
                var visitor = new RemapVisitor<Entity>(remap);

                foreach (var entity in entities)
                {
                    PropertyContainer.Visit(new EntityContainer(EntityManager, entity, false), visitor);
                }
            }
        }

        #endregion
    }
}
