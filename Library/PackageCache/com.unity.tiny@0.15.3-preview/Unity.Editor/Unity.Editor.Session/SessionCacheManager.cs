using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Entities;
using Unity.Tiny.Core;

namespace Unity.Editor
{
    internal interface ISessionCacheManager
    {
        string GetEntityName(Entity entity);
    }

    internal class SessionCacheManager : SessionManager, ISessionCacheManager
    {
        private IWorldManagerInternal m_WorldManager;
        private readonly Dictionary<Guid, string> m_EntityGuidToName;

        private EntityManager EntityManager => m_WorldManager.EntityManager;

        public SessionCacheManager(Session session) : base(session)
        {
            m_EntityGuidToName = new Dictionary<Guid,string>();
        }

        public override void Load()
        {
            m_WorldManager = Session.GetManager<IWorldManagerInternal>();
        }

        public override void Unload()
        {
        }

        private void HandleChanges(Changes changes)
        {
            foreach (var created in changes.CreatedEntities())
            {
                var entity = m_WorldManager.GetEntityFromGuid(created);
                if (EntityManager.Exists(entity) && EntityManager.HasComponent<EntityName>(entity))
                {
                    m_EntityGuidToName[created] = EntityManager.GetBufferAsString<EntityName>(entity);
                }
            }

            foreach (var changed in changes.ChangedEntitiesWithAddComponent<EntityName>()
                .Concat(changes.ChangedEntitiesWithSetComponent<EntityName>()))
            {
                var entity = m_WorldManager.GetEntityFromGuid(changed);
                if (EntityManager.Exists(entity) && EntityManager.HasComponent<EntityName>(entity))
                {
                    m_EntityGuidToName[changed] = EntityManager.GetBufferAsString<EntityName>(entity);
                }
            }

            foreach (var changed in changes.ChangedEntitiesWithRemoveComponent<EntityName>())
            {
                m_EntityGuidToName.Remove(changed);
            }

            foreach (var deleted in changes.DeletedEntities())
            {
                m_EntityGuidToName.Remove(deleted);
            }
        }

        public string GetEntityName(Entity entity)
        {
            var guid = m_WorldManager.GetEntityGuid(entity);

            if (m_EntityGuidToName.TryGetValue(guid, out var name))
            {
                return name;
            }

            if (EntityManager.HasComponent<EntityName>(entity))
            {
                name = EntityManager.GetBufferAsString<EntityName>(entity);
                m_EntityGuidToName.Add(guid, name);
                return name;
            }

            return entity.ToString();
        }
    }
}
