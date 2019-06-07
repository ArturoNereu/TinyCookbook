using System;
using Unity.Authoring;
using Unity.Entities;
using Unity.Tiny.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Editor.Bindings
{
    internal sealed class BindingContext : IBindingContext
    {
        private readonly IWorldManager m_WorldManager;
        private readonly EntityManager m_EntityManager;
        private readonly IAssetManager m_AssetManager;
        private readonly UnityComponentCacheManager m_ComponentManager;

        internal BindingContext(Session session)
        {
            m_WorldManager = session.GetManager<IWorldManager>();
            m_EntityManager = m_WorldManager.EntityManager;
            m_AssetManager = session.GetManager<IAssetManager>();
            m_ComponentManager = session.GetManager<UnityComponentCacheManager>();
        }

        public bool HasComponent<TComponentData>(Entity entity) where TComponentData : struct
        {
            return m_EntityManager.HasComponent<TComponentData>(entity);
        }

        public TComponentData GetComponentData<TComponentData>(Entity entity) where TComponentData : struct, IComponentData
        {
            return m_EntityManager.GetComponentData<TComponentData>(entity);
        }

        public void AddComponent<TComponentData>(Entity entity) where TComponentData : struct, IComponentData
        {
            m_EntityManager.AddComponentData<TComponentData>(entity, default);
        }

        public void AddComponentData<TComponentData>(Entity entity, TComponentData data) where TComponentData : struct, IComponentData
        {
            m_EntityManager.AddComponentData<TComponentData>(entity, data);
        }

        public void SetComponentData<TComponentData>(Entity entity, TComponentData data) where TComponentData : struct, IComponentData
        {
            m_EntityManager.SetComponentData<TComponentData>(entity, data);
        }

        public DynamicBuffer<TElementType> AddBuffer<TElementType>(Entity entity) where TElementType : struct, IBufferElementData
        {
            return m_EntityManager.AddBuffer<TElementType>(entity);
        }

        public DynamicBuffer<TElementType> GetBuffer<TElementType>(Entity entity) where TElementType : struct, IBufferElementData
        {
            return m_EntityManager.GetBuffer<TElementType>(entity);
        }

        public DynamicBuffer<TElementType> GetBufferRO<TElementType>(Entity entity) where TElementType : struct, IBufferElementData
        {
            return m_EntityManager.GetBufferRO<TElementType>(entity);
        }

        public void RemoveComponent<TComponentData>(Entity entity) where TComponentData : struct
        {
            m_EntityManager.RemoveComponent<TComponentData>(entity);
        }

        public TComponent GetUnityComponent<TComponent>(Entity entity) where TComponent : Component
        {
            return m_ComponentManager.GetComponent<TComponent>(entity);
        }

        public TComponent AddMissingUnityComponent<TComponent>(Entity entity, Action<TComponent> onComponentAdded = null) where TComponent : Component
        {
            var component = GetUnityComponent<TComponent>(entity);
            if (null == component)
            {
                component = m_ComponentManager.AddComponent<TComponent>(entity);
                if (null != component)
                {
                    onComponentAdded?.Invoke(component);
                }
            }
            return component;
        }

        public void RemoveUnityComponent<TComponent>(Entity entity) where TComponent : Component
        {
            m_ComponentManager.RemoveComponent<TComponent>(entity);
        }

        public Entity GetEntityFromUnityComponent(Component component)
        {
            return m_WorldManager.GetEntityFromGuid(component.GetComponent<EntityReference>().Guid);
        }

        public Entity GetEntity<TObject>(TObject obj) where TObject : Object
        {
            return m_AssetManager.GetEntity(obj);
        }

        public TObject GetUnityObject<TObject>(Entity entity) where TObject : Object
        {
            return m_AssetManager.GetUnityObject<TObject>(entity);
        }
    }
}
