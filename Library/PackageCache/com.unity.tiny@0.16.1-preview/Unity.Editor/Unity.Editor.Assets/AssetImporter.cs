using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Authoring;
using Unity.Authoring.Core;
using Unity.Editor.Extensions;
using Unity.Entities;
using Unity.Tiny.Core;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Editor.Assets
{
    internal interface IAssetImporter
    {
        Entity GetEntity(UnityEngine.Object obj);
        Entity CreateEntity(params ComponentType[] types);
        void AddComponentData<T>(Entity entity, T componentData) where T : struct, IComponentData;
        T GetComponentData<T>(Entity entity) where T : struct, IComponentData;
        void SetComponentData<T>(Entity entity, T componentData) where T : struct, IComponentData;
        bool HasComponent<T>(Entity entity) where T : struct, IComponentData;
        void RemoveComponent<T>(Entity entity) where T : struct, IComponentData;
        DynamicBuffer<T> AddBuffer<T>(Entity entity) where T : struct, IBufferElementData;
        DynamicBuffer<T> GetBuffer<T>(Entity entity) where T : struct, IBufferElementData;
        void AddBufferFromString<T>(Entity entity, string value) where T : struct, IBufferElementData;
        string GetBufferAsString<T>(Entity entity) where T : struct, IBufferElementData;
        void SetBufferFromString<T>(Entity entity, string value) where T : struct, IBufferElementData;
    }

    internal class AssetImporter : IAssetImporter
    {
        private readonly Dictionary<Type, IUnityObjectAssetImporter> m_AssetImporters = new Dictionary<Type, IUnityObjectAssetImporter>();

        private IWorldManager WorldManager { get; }
        private IAssetManager AssetManager { get; }
        private EntityManager EntityManager => WorldManager.EntityManager;

        internal AssetImporter(Session session)
        {
            WorldManager = session.GetManager<IWorldManager>();
            Assert.IsNotNull(WorldManager);

            AssetManager = session.GetManager<IAssetManager>();
            Assert.IsNotNull(AssetManager);

            foreach (var pair in DomainCache.AssetImporterTypes)
            {
                m_AssetImporters[pair.Key] = (IUnityObjectAssetImporter)Activator.CreateInstance(pair.Value);
            }
        }

        internal Entity Import(UnityEngine.Object obj)
        {
            if (obj == null || !obj)
            {
                return Entity.Null;
            }

            var entity = Entity.Null;
            var assetImporter = m_AssetImporters.Values.FirstOrDefault(x => x.CanImport(obj));
            if (assetImporter != null)
            {
                entity = assetImporter.Import(this, obj);
                if (entity != Entity.Null)
                {
                    var guid = obj.GetGuid();
                    Assert.IsTrue(guid != Guid.Empty);
                    Assert.IsFalse(EntityManager.HasComponent<EntityGuid>(entity));
                    WorldManager.SetEntityGuid(entity, guid);

                    Assert.IsFalse(EntityManager.HasComponent<AssetReference>(entity));
                    EntityManager.AddComponentData(entity, obj.ToAssetReference());
                }
            }
            return entity;
        }

        internal bool CanImport(UnityEngine.Object obj)
        {
            if (obj == null || !obj)
            {
                return false;
            }

            return m_AssetImporters.Values.FirstOrDefault(x => x.CanImport(obj)) != null;
        }

        #region IAssetImporter

        public Entity GetEntity(UnityEngine.Object obj)
        {
            return AssetManager.GetEntity(obj);
        }

        public Entity CreateEntity(params ComponentType[] types)
        {
            return EntityManager.CreateEntity(types);
        }

        public void AddComponentData<T>(Entity entity, T componentData) where T : struct, IComponentData
        {
            EntityManager.AddComponentData(entity, componentData);
        }

        public T GetComponentData<T>(Entity entity) where T : struct, IComponentData
        {
            return EntityManager.GetComponentData<T>(entity);
        }

        public void SetComponentData<T>(Entity entity, T componentData) where T : struct, IComponentData
        {
            EntityManager.SetComponentData(entity, componentData);
        }

        public bool HasComponent<T>(Entity entity) where T : struct, IComponentData
        {
            return EntityManager.HasComponent<T>(entity);
        }

        public void RemoveComponent<T>(Entity entity) where T : struct, IComponentData
        {
            EntityManager.RemoveComponent<T>(entity);
        }

        public DynamicBuffer<T> AddBuffer<T>(Entity entity) where T : struct, IBufferElementData
        {
            return EntityManager.AddBuffer<T>(entity);
        }

        public DynamicBuffer<T> GetBuffer<T>(Entity entity) where T : struct, IBufferElementData
        {
            return EntityManager.GetBuffer<T>(entity);
        }

        public void AddBufferFromString<T>(Entity entity, string value) where T : struct, IBufferElementData
        {
            EntityManager.AddBufferFromString<T>(entity, value);
        }

        public string GetBufferAsString<T>(Entity entity) where T : struct, IBufferElementData
        {
            return EntityManager.GetBufferAsString<T>(entity);
        }

        public void SetBufferFromString<T>(Entity entity, string value) where T : struct, IBufferElementData
        {
            EntityManager.SetBufferFromString<T>(entity, value);
        }

        #endregion
    }

    internal interface IUnityObjectAssetImporter
    {
        bool CanImport(UnityEngine.Object obj);
        Entity Import(IAssetImporter context, UnityEngine.Object obj);
    }

    internal abstract class UnityObjectAssetImporter<T> : IUnityObjectAssetImporter
        where T : UnityEngine.Object
    {
        public bool CanImport(UnityEngine.Object obj)
        {
            return obj is T;
        }

        public Entity Import(IAssetImporter context, UnityEngine.Object obj)
        {
            return Import(context, obj as T);
        }

        public abstract Entity Import(IAssetImporter context, T obj);
    }
}
