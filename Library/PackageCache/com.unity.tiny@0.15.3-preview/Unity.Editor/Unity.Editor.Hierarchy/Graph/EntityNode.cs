using System;
using Unity.Authoring;
using Unity.Authoring.Core;
using Unity.Editor.Bindings;
using Unity.Entities;
using UnityEngine;

namespace Unity.Editor
{
    internal class EntityNode : SceneGraphNodeBase, IParent
    {
        private readonly Session m_Session;
        private readonly IWorldManager m_WorldManager;
        private readonly EntityManager m_EntityManager;
        private readonly UnityComponentCacheManager m_ComponentCache;

        public Entity Entity { get; set; }
        public Guid Guid { get; set; }
        public SiblingIndex Index { get; set; }


        public override bool EnabledInHierarchy
        {
            get
            {
                var self = !m_EntityManager.HasComponent<Disabled>(Entity);
                return self && (Parent?.EnabledInHierarchy ?? true);
            }
        }

        public Transform Transform => m_ComponentCache.GetEntityReference(Guid).transform;

        public EntityNode(ISceneGraph graph, Session session, Entity entity)
            : base(graph)
        {
            m_Session = session;
            m_WorldManager = m_Session.GetManager<IWorldManager>();
            m_EntityManager = m_WorldManager.EntityManager;
            m_ComponentCache = m_Session.GetManager<UnityComponentCacheManager>();

            Entity = entity;
            Guid = m_WorldManager.GetEntityGuid(entity);

            if (m_EntityManager.HasComponent<SiblingIndex>(Entity))
            {
                Index = m_EntityManager.GetComponentData<SiblingIndex>(Entity);
            }
            else
            {
                Index = new SiblingIndex { Index = int.MaxValue };
                m_EntityManager.AddComponentData(Entity, Index);
            }
        }

        public void SetSiblingIndex(int index)
        {
            Index = new SiblingIndex { Index = index };
            if (m_EntityManager.HasComponent<SiblingIndex>(Entity))
            {
                m_EntityManager.SetComponentData(Entity, Index);
            }
            else
            {
                m_EntityManager.AddComponentData(Entity, Index);
            }
        }
    }
}

