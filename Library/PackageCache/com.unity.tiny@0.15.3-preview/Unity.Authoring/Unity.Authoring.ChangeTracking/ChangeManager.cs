#if !NET_DOTS
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Profiling;

namespace Unity.Authoring.ChangeTracking
{
    /// <summary>
    /// The caretaker is used to manage the shadow world and handle snapshots.
    /// </summary>
    internal interface IChangeManager : ISessionManager
    {
        /// <summary>
        /// Register to receive world change events.
        /// </summary>
        void RegisterChangeCallback(ChangeEventHandler handler, int order = 0);

        /// <summary>
        /// Unregister from receiving world change events.
        /// </summary>
        void UnregisterChangeCallback(ChangeEventHandler handler);

        /// <summary>
        /// Ticks the version change system and dispatches events to all listeners.
        /// </summary>
        void Update();
        
        event Action BeginChangeTracking;
        event Action EndChangeTracking;
    }

    internal class ChangeManager: SessionManager, IChangeManager
    {
        private struct OrderedChangeEventHandler
        {
            public ChangeEventHandler Handler { get; }
            public int Order { get; }

            public OrderedChangeEventHandler(ChangeEventHandler handler, int order)
            {
                Handler = handler;
                Order = order;
            }
        }

        /// <summary>
        /// Handle to the main authoring world.
        /// </summary>
        private IWorldManagerInternal m_WorldManager;

        /// <summary>
        /// Change tracker to record diffs on the main world.
        /// </summary>
        private WorldChangeTracker m_WorldChangeTracker;

        /// <summary>
        /// Ordered set of callbacks.
        /// </summary>
        private readonly List<OrderedChangeEventHandler> m_Callbacks;

        /// <summary>
        /// Copy of the callback set used to support mutation during iteration.
        /// </summary>
        private readonly List<OrderedChangeEventHandler> m_CallbacksBuffer;

        /// <summary>
        /// Optimization to only re-sort the callbacks when they are added or removed.
        /// </summary>
        private bool m_CallbacksChanged;
        
        public event Action BeginChangeTracking = delegate { };
        public event Action EndChangeTracking = delegate { };

        public ChangeManager(Session session) : base(session)
        {
            m_Callbacks = new List<OrderedChangeEventHandler>();
            m_CallbacksBuffer = new List<OrderedChangeEventHandler>();
        }

        public override void Load()
        {
            m_WorldManager = Session.GetManager<IWorldManagerInternal>();

            if (null == m_WorldManager)
            {
                throw new ArgumentNullException(nameof(m_WorldManager));
            }

            m_WorldChangeTracker = new WorldChangeTracker(m_WorldManager.World, Allocator.Persistent);
        }

        public override void Unload()
        {
            m_WorldManager = null;

            m_WorldChangeTracker.Dispose();
            m_WorldChangeTracker = null;
        }

        /// <inheritdoc />
        public void RegisterChangeCallback(ChangeEventHandler handler, int order = 0)
        {
            m_Callbacks.Add(new OrderedChangeEventHandler(handler, order));
            m_CallbacksChanged = true;
        }

        /// <inheritdoc />
        public void UnregisterChangeCallback(ChangeEventHandler handler)
        {
            m_Callbacks.RemoveAll(callback => callback.Handler == handler);
            m_CallbacksChanged = true;
        }

        /// <inheritdoc />
        public void Update()
        {
            if (null == m_WorldManager.World)
            {
                throw new ArgumentException(nameof(m_WorldManager) + "." + nameof(m_WorldManager.World));
            }

            var entityManager = m_WorldManager.World.EntityManager;

            BeginChangeTracking();
            
            for (;;)
            {
                var entityManagerVersion = entityManager.Version;
                var globalSystemVersion = entityManager.GlobalSystemVersion;

                if (m_WorldChangeTracker.TryGetChanges(out var changes))
                {
                    try
                    {
                        // We may get false positives if someone requests ReadWrite access and does not mutate anything.
                        if (!changes.WorldDiff.HasChanges)
                        {
                            break;
                        }

                        m_WorldManager.RebuildGuidCache();

                        DispatchedChangedEvent(changes);
                    }
                    finally
                    {
                        changes.Dispose();
                    }

                    if (entityManagerVersion != entityManager.Version || HasAnyChunkChanged(entityManager, globalSystemVersion))
                    {
                        continue;
                    }
                }

                break;
            }

            EndChangeTracking();
        }

        private static unsafe bool HasAnyChunkChanged(EntityManager entityManager, uint globalSystemVersion)
        {
            using (var query = entityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {typeof(EntityGuid)},
                Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
            }))
            {
                using (var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob))
                {
                    for (var chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
                    {
                        var chunk = chunks[chunkIndex];
                        var archetype = chunk.Archetype.Archetype;
                        for (var typeIndex = 0; typeIndex < archetype->TypesCount; typeIndex++)
                        {
                            if (chunk.m_Chunk->GetChangeVersion(typeIndex) > globalSystemVersion)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }

        private void DispatchedChangedEvent(Changes changes)
        {
            if (m_CallbacksChanged)
            {
                m_Callbacks.Sort(CompareWrapper);
                m_CallbacksBuffer.Clear();
                m_CallbacksBuffer.AddRange(m_Callbacks);
            }

            foreach (var update in m_CallbacksBuffer)
            {
                update.Handler.Invoke(changes);
            }
        }

        private static int CompareWrapper(OrderedChangeEventHandler lhs, OrderedChangeEventHandler rhs)
        {
            return lhs.Order.CompareTo(rhs.Order);
        }
    }
}

#endif
