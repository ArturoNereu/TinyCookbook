using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace Unity.Editor
{
    internal class ComplexEntityQuery : IDisposable
    {
        [Flags]
        private enum QueryMode
        {
            None = 0,
            ContainsAny = 1,
            ContainsNone = 2,
        }

        private QueryMode m_Mode = QueryMode.None;
        private readonly List<EntityQuery> m_AnyQueries = new List<EntityQuery>();
        private readonly List<EntityQuery> m_NoneQueries = new List<EntityQuery>();

        public bool Empty => m_AnyQueries.Count == 0 && m_NoneQueries.Count == 0;

        public void AddAnyQuery(EntityQuery any)
        {
            if (null == any)
            {
                return;
            }
            m_AnyQueries.Add(any);
            m_Mode |= QueryMode.ContainsAny;
        }

        public void AddNoneQuery(EntityQuery none)
        {
            if (null == none)
            {
                return;
            }
            m_NoneQueries.Add(none);
            m_Mode |= QueryMode.ContainsNone;
        }

        public void AddToQuery(NativeHashMap<Entity, int> resultMap)
        {
            using (var any = new NativeHashMap<Entity, int>(512, Allocator.TempJob))
            using (var none = new NativeHashMap<Entity, int>(512, Allocator.TempJob))
            {
                foreach (var query in m_AnyQueries)
                {
                    using (var entities = query.ToEntityArray(Allocator.TempJob))
                    {
                        for (var i = 0; i < entities.Length; ++i)
                        {
                            any.TryAdd(entities[i], 1);
                        }
                    }
                }

                foreach (var query in m_NoneQueries)
                {
                    using (var entities = query.ToEntityArray(Allocator.TempJob))
                    {
                        for (var i = 0; i < entities.Length; ++i)
                        {
                            var entity = entities[i];
                            if (!none.TryAdd(entity, 1) && none.TryGetValue(entity, out var currentCount))
                            {
                                none.Remove(entity);
                                none.TryAdd(entity, currentCount + 1);
                            }
                        }
                    }
                }

                switch (m_Mode)
                {
                    case QueryMode.None:
                        break;
                    case QueryMode.ContainsAny:
                        using (var entities = any.GetKeyArray(Allocator.TempJob))
                        {
                            foreach (var entity in entities)
                            {
                                if (!resultMap.TryAdd(entity, 1) && resultMap.TryGetValue(entity, out var currentCount))
                                {
                                    resultMap.Remove(entity);
                                    resultMap.TryAdd(entity, currentCount + 1);
                                }
                            }
                        }

                        break;
                    case QueryMode.ContainsNone:
                        using (var entities = none.GetKeyArray(Allocator.TempJob))
                        {
                            foreach (var entity in entities)
                            {
                                if (none.TryGetValue(entity, out var count) && count == m_NoneQueries.Count)
                                {
                                    if (!resultMap.TryAdd(entity, 1) &&
                                        resultMap.TryGetValue(entity, out var currentCount))
                                    {
                                        resultMap.Remove(entity);
                                        resultMap.TryAdd(entity, currentCount + 1);
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Dispose()
        {
            foreach (var query in m_AnyQueries)
            {
                query?.Dispose();
            }

            foreach (var query in m_NoneQueries)
            {
                query?.Dispose();
            }
        }
    }
}
