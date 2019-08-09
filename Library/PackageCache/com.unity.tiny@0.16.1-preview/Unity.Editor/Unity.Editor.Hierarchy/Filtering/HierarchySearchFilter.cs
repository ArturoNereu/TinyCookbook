using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Tiny.Core;

namespace Unity.Editor
{
    internal class HierarchySearchFilter : IDisposable
    {
        private readonly EntityManager m_EntityManager;
        private readonly ComplexEntityQuery[] m_EntityQuery;
        private readonly NameFilter[] m_Names;

        internal HierarchySearchFilter(EntityManager entityManager, List<NameFilter> names, ComplexEntityQuery[] entityQuery)
        {
            m_EntityManager = entityManager;
            m_Names = names.ToArray();
            m_EntityQuery = entityQuery;
        }

        private bool KeepEntityBasedOnName(Entity entity)
        {
            if (m_Names.Length == 0)
            {
                return true;
            }

            if (!m_EntityManager.HasComponent<EntityName>(entity))
            {
                return false;
            }

            var entityName = m_EntityManager.GetBuffer<EntityName>(entity).Reinterpret<char>().AsString();
            foreach (var filter in m_Names)
            {
                if (!filter.Keep(entityName))
                {
                    return false;
                }
            }
            return true;
        }

        public NativeHashMap<Entity, int> ToResult(Allocator allocator)
        {
            var map = new NativeHashMap<Entity, int>(512, Allocator.TempJob);
            try
            {
                if (m_EntityQuery.All(q => q.Empty))
                {
                    using (var allEntities = m_EntityManager.GetAllEntities(Allocator.TempJob))
                    {
                        foreach (var entity in allEntities)
                        {
                            map.TryAdd(entity, 0);
                        }
                    }
                }
                else
                {
                    foreach (var query in m_EntityQuery)
                    {
                        query.AddToQuery(map);
                    }
                }

                var r = new NativeHashMap<Entity, int>(512, allocator);

                using (var entities = map.GetKeyArray(Allocator.TempJob))
                {
                    foreach (var entity in entities)
                    {
                        if (map.TryGetValue(entity, out var count) && count == m_EntityQuery.Length &&
                            KeepEntityBasedOnName(entity))
                        {
                            r.TryAdd(entity, 0);
                        }
                    }

                    return r;
                }
            }
            finally
            {
                map.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var query in m_EntityQuery)
            {
                query?.Dispose();
            }
        }
    }
}
