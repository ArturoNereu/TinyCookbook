using System;
using Unity.Authoring.Core;
using Unity.Authoring.Hashing;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Tiny.Core;

namespace Unity.Authoring
{
    /// <summary>
    /// Provides helpers to create and retrieve Entities.
    /// </summary>
    public interface IWorldManager : ISessionManager
    {
        /// <summary>
        /// Creates an Entity with the provided Component types.
        /// </summary>
        /// <param name="componentTypes">An array of Component types to be added to the Entity.</param>
        /// <returns>The new Entity.</returns>
        Entity CreateEntity(params ComponentType[] componentTypes);

        /// <summary>
        /// Creates an Entity with the provided archetype.
        /// </summary>
        /// <param name="archetype">The archetype that contains the Components to be added to the Entity.</param>
        /// <returns>The new Entity.</returns>
        Entity CreateEntity(EntityArchetype archetype);

        /// <summary>
        /// Creates an Entity with the provided name and Component types.
        /// </summary>
        /// <param name="name">The name of the Entity.</param>
        /// <param name="componentTypes">An array of Component types to be added to the Entity.</param>
        /// <returns>The new Entity.</returns>
        Entity CreateEntity(string name, params ComponentType[] componentTypes);

        /// <summary>
        /// Creates an Entity with the provided name and archetype.
        /// </summary>
        /// /// <param name="name">The name of the Entity.</param>
        /// <param name="archetype">The archetype that contains the Components to be added to the Entity.</param>
        /// <returns>The new Entity.</returns>
        Entity CreateEntity(string name, EntityArchetype archetype);

        /// <summary>
        /// Returns the Entity name defined by the <see cref="EntityName"/> buffer Component.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <returns>The name of the Entity.</returns>
        string GetEntityName(Entity entity);

        /// <summary>
        /// Sets the name of the Entity.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <param name="name">The name of the Entity.</param>
        void SetEntityName(Entity entity, string name);

        /// <summary>
        /// Returns the Entity GUID defined by the <see cref="EntityGuid"/> Component.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <returns>The GUID of the Entity.</returns>
        Guid GetEntityGuid(Entity entity);

        /// <summary>
        /// Sets the GUID of the Entity.
        /// </summary>
        /// <param name="entity">The Entity.</param>
        /// <param name="guid">The GUID of the Entity.</param>
        void SetEntityGuid(Entity entity, Guid guid);

        /// <summary>
        /// Returns the Entity that links to the GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns>The Entity.</returns>
        Entity GetEntityFromGuid(Guid guid);

        /// <summary>
        /// Returns the configuration Entity.
        /// </summary>
        /// <returns>The Entity.</returns>
        Entity GetConfigEntity();

        /// <summary>
        /// Returns the underlying <see cref="World"/> object.
        /// </summary>
        World World { get; }

        /// <summary>
        /// Returns the underlying <see cref="EntityManager"/> object.
        /// </summary>
        EntityManager EntityManager { get; }
    }

    internal interface IWorldManagerInternal : IWorldManager, ISessionManagerInternal
    {
        void RebuildGuidCache();
    }

    /// <summary>
    /// The world manager handles the `Authoring` world lifecycle.
    /// </summary>
    internal class WorldManager : ISessionManagerInternal, IWorldManagerInternal
    {
        [BurstCompile]
        private struct BuildEntityCacheJob : IJobParallelFor
        {
            public NativeArray<Entity> Entities;
            public NativeArray<EntityGuid> Guids;
            public NativeHashMap<EntityGuid, Entity>.Concurrent EntityGuidToEntity;

            public void Execute(int index)
            {
                EntityGuidToEntity.TryAdd(Guids[index], Entities[index]);
            }
        }

        private readonly World m_LastActiveWorld;
        private EntityQuery m_QueryGuids;
        private EntityQuery m_QueryConfigEntity;
        private NativeHashMap<EntityGuid, Entity> m_EntityGuidToEntity;
        private Entity m_ConfigEntity;

        public World World { get; private set; }
        public EntityManager EntityManager => World?.EntityManager ?? null;

        public WorldManager()
        {
            m_LastActiveWorld = World.Active;
            World = new World("DotsSession");
            m_EntityGuidToEntity = new NativeHashMap<EntityGuid, Entity>(4096, Allocator.Persistent);
            m_ConfigEntity = Entity.Null;
        }

        public void Load(Session session)
        {
            m_QueryGuids = EntityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<EntityGuid>() },
                Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
            });

            m_QueryConfigEntity = EntityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<ConfigurationTag>() },
                Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
            });
        }

        public void Unload(Session session)
        {
            m_EntityGuidToEntity.Dispose();
            m_QueryGuids.Dispose();
            m_QueryGuids = null;
            m_QueryConfigEntity.Dispose();
            m_QueryConfigEntity = null;
            World.Dispose();
            World = null;
            World.Active = m_LastActiveWorld;

            // Temporary fix for WordStorage instance leak
            if (WordStorage.Instance != null)
            {
                WordStorage.Instance.Dispose();
                WordStorage.Instance = null;
            }
        }

        public void RebuildGuidCache()
        {
            using (var entities = m_QueryGuids.ToEntityArray(Allocator.TempJob))
            using (var guids = m_QueryGuids.ToComponentDataArray<EntityGuid>(Allocator.TempJob))
            {
                m_EntityGuidToEntity.Clear();
                m_EntityGuidToEntity.Capacity = Math.Max(entities.Length, m_EntityGuidToEntity.Capacity);

                new BuildEntityCacheJob
                {
                    Entities = entities,
                    Guids = guids,
                    EntityGuidToEntity = m_EntityGuidToEntity.ToConcurrent()
                }.Schedule(entities.Length, 64).Complete();
            }
        }

        #region IWorldManager

        public Entity CreateEntity(params ComponentType[] componentTypes)
        {
            return CreateEntity(null, Guid.NewGuid(), componentTypes);
        }

        public Entity CreateEntity(EntityArchetype archetype)
        {
            return CreateEntity(null, Guid.NewGuid(), archetype);
        }

        public Entity CreateEntity(string name, params ComponentType[] componentTypes)
        {
            return CreateEntity(name, Guid.NewGuid(), componentTypes);
        }

        public Entity CreateEntity(string name, EntityArchetype archetype)
        {
            return CreateEntity(name, Guid.NewGuid(), archetype);
        }

        public Entity CreateEntity(string name, Guid guid, params ComponentType[] componentTypes)
        {
            var entity = EntityManager.CreateEntity(componentTypes);
            SetEntityName(entity, name);
            SetEntityGuid(entity, guid);
            return entity;
        }

        public Entity CreateEntity(string name, Guid guid, EntityArchetype archetype)
        {
            var entity = EntityManager.CreateEntity(archetype);
            SetEntityName(entity, name);
            SetEntityGuid(entity, guid);
            return entity;
        }

        public string GetEntityName(Entity entity)
        {
            if (EntityManager.HasComponent<EntityName>(entity))
            {
                return EntityManager.GetBufferAsString<EntityName>(entity);
            }

            return entity.ToString();
        }

        public void SetEntityName(Entity entity, string name)
        {
            if (EntityManager.HasComponent<EntityName>(entity))
            {
                EntityManager.SetBufferFromString<EntityName>(entity, name);
            }
            else if (!string.IsNullOrEmpty(name))
            {
                EntityManager.AddBufferFromString<EntityName>(entity, name);
            }
        }

        public Guid GetEntityGuid(Entity entity)
        {
            if (EntityManager.HasComponent<EntityGuid>(entity))
            {
                return EntityManager.GetComponentData<EntityGuid>(entity).ToGuid();
            }
            return Guid.Empty;
        }

        public void SetEntityGuid(Entity entity, Guid guid)
        {
            if (EntityManager.HasComponent<EntityGuid>(entity))
            {
                EntityManager.SetComponentData(entity, guid.ToEntityGuid());
            }
            else if (guid != Guid.Empty)
            {
                EntityManager.AddComponentData(entity, guid.ToEntityGuid());
            }
        }

        public Entity GetEntityFromGuid(Guid guid)
        {
            if (guid == Guid.Empty)
            {
                return Entity.Null;
            }

            return m_EntityGuidToEntity.TryGetValue(guid.ToEntityGuid(), out var entity) ? entity : Entity.Null;
        }

        public Entity GetConfigEntity()
        {
            if (m_ConfigEntity != Entity.Null)
            {
                return m_ConfigEntity;
            }

            using (var entities = m_QueryConfigEntity.ToEntityArray(Allocator.TempJob))
            {
                if (entities.Length > 0)
                {
                    m_ConfigEntity = entities[0];
                }
            }

            return m_ConfigEntity;
        }

        #endregion
    }
}
