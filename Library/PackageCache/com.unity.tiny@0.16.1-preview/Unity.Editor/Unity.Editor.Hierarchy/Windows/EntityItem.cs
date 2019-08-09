using System;
using Unity.Authoring;
using Unity.Editor.Bindings;
using Unity.Entities;
using UnityEngine;

namespace Unity.Editor.Hierarchy
{
    internal class EntityItem : HierarchyItem
    {
        private readonly Session m_Session;
        private readonly IWorldManager m_WorldManager;
        private readonly UnityComponentCacheManager m_ComponentCache;

        public Entity Entity { get; }
        public Guid Guid { get; }
        public EntityNode Node { get; }

        public EntityItem(Session session, Entity entity, Guid guid, EntityNode node)
        {
            m_Session = session;
            m_WorldManager = session.GetManager<IWorldManager>();
            m_ComponentCache = m_Session.GetManager<UnityComponentCacheManager>();

            Entity = entity;
            Guid = guid;
            Node = node;

            m_ComponentCache.CreateLink(entity, guid);
        }

        public override string displayName
        {
            get => m_WorldManager.GetEntityName(Entity);
            set
            {
                // TODO: Check name
                m_WorldManager.SetEntityName(Entity, value);
            }
        }

        public override int id => m_ComponentCache.GetEntityReference(Guid)?.gameObject.GetInstanceID() ?? -1;

        public override int depth => parent?.depth + 1 ?? 0;

        public override Texture2D icon => Icons.Entity;
    }
}
