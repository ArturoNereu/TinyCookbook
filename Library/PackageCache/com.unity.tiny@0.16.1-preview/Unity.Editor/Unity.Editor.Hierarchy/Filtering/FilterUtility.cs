using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Unity.Editor
{
    internal static class FilterUtility
    {
        public class TypeInfo
        {
            public string Name;
            public int TypeIndex;
        }

        public static readonly List<TypeInfo> AllTypes;

        static FilterUtility()
        {
            AllTypes = new List<TypeInfo>(TypeManager.AllTypes.Count());
            foreach (var typeInfo in TypeManager.AllTypes)
            {
                if (null == typeInfo.Type)
                {
                    continue;
                }

                AllTypes.Add(new TypeInfo
                {
                    Name = typeInfo.Type.Name,
                    TypeIndex = typeInfo.TypeIndex
                });
            }
        }

        internal static HierarchySearchFilter CreateHierarchyFilter(EntityManager entityManager, string input)
        {
            var names = ListPool<NameFilter>.Get();
            var components = ListPool<ComponentFilter>.Get();
            try
            {
                GenerateFilterTokens(input, names, components);
                return GenerateSearchFilter(entityManager, names, components);
            }
            finally
            {
                ListPool<NameFilter>.Release(names);
                ListPool<ComponentFilter>.Release(components);
            }
        }
        
        internal static AddComponentSearchFilter CreateAddComponentFilter(string input)
        {
            var names = ListPool<NameFilter>.Get();
            var components = ListPool<ComponentFilter>.Get();
            try
            {
                GenerateFilterTokens(input, names);
                return new AddComponentSearchFilter(names);
            }
            finally
            {
                ListPool<NameFilter>.Release(names);
                ListPool<ComponentFilter>.Release(components);
            }
        }


        internal static HierarchySearchFilter GenerateSearchFilter(EntityManager entityManager, List<NameFilter> names,
            List<ComponentFilter> components)
        {
            var queries = new List<ComplexEntityQuery>();

            using (var map = new NativeHashMap<int, bool>(components.Count, Allocator.Temp))
            {
                foreach (var filter in components)
                {
                    map.Clear();
                    foreach (var type in FilterUtility.AllTypes)
                    {
                        if (filter.Inverted)
                        {
                            if (!filter.Keep(type.Name))
                            {
                                map.TryAdd(type.TypeIndex, true);
                            }
                        }
                        else
                        {
                            if (filter.Keep(type.Name))
                            {
                                map.TryAdd(type.TypeIndex, false);
                            }
                        }
                    }

                    var query = new ComplexEntityQuery();
                    if (map.Length > 0)
                    {
                        using (var types = map.GetKeyArray(Allocator.TempJob))
                        {
                            var all = new NativeList<ComponentType>(0, Allocator.TempJob);
                            var none = new NativeList<ComponentType>(0, Allocator.TempJob);
                            try
                            {
                                for (var i = 0; i < types.Length; ++i)
                                {
                                    if (map[types[i]])
                                    {
                                        none.Add(TypeManager.GetTypeInfo(types[i]).Type);
                                    }
                                    else
                                    {
                                        var typeInfo = TypeManager.GetTypeInfo(types[i]);
                                        all.Add(typeInfo.Type);
                                    }
                                }

                                for (var i = 0; i < all.Length; i+=4)
                                {
                                    var count = Mathf.Min(all.Length - i, 4);
                                    var t = new ComponentType[count];
                                    for (var j = 0; j < count; ++j)
                                    {
                                        t[j] = all[i + j];
                                    }
                                    query.AddAnyQuery(entityManager.CreateEntityQuery(new EntityQueryDesc{ Any = t, Options = EntityQueryOptions.IncludeDisabled }));
                                }

                                for (var i = 0; i < none.Length; i+=4)
                                {
                                    var count = Mathf.Min(none.Length - i, 4);
                                    var t = new ComponentType[count];
                                    for (var j = 0; j < count; ++j)
                                    {
                                        t[j] = none[i + j];
                                    }
                                    query.AddNoneQuery(entityManager.CreateEntityQuery(new EntityQueryDesc{ None = t, Options = EntityQueryOptions.IncludeDisabled }));
                                }
                            }
                            finally
                            {
                                all.Dispose();
                                none.Dispose();
                            }
                        }
                    }
                    else
                    {
                        query.AddNoneQuery(entityManager.CreateEntityQuery(new EntityQueryDesc{ None = new ComponentType[]{typeof(Entity)}, Options = EntityQueryOptions.IncludeDisabled }));
                    }
                    queries.Add(query);

                }
            }

            return new HierarchySearchFilter(entityManager, names, queries.ToArray());
        }

        private static void GenerateFilterTokens(string filter, ICollection<NameFilter> names, ICollection<ComponentFilter> components)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return;
            }

            filter = filter.Trim();

            var start = 0;
            var component = false;
            for (var i = 0; i < filter.Length - 1; ++i)
            {
                if (filter[i] == ' ')
                {
                    if (component)
                    {
                        component = false;
                        var f = CreateComponentFilter(filter.Substring(start, i - start));
                        if (!f.Universal)
                        {
                            components.Add(f);
                        }
                    }
                    else
                    {
                        names.Add(CreateNameFilter(filter.Substring(start, i - start)));
                    }

                    start = i + 1;
                    continue;
                }

                if (char.ToLower(filter[i]) == 'c' && char.ToLower(filter[i + 1]) == ':')
                {
                    component = true;
                    start = i + 2;
                    ++i;
                }
            }

            if (start < filter.Length)
            {
                if (component)
                {
                    var f = CreateComponentFilter(filter.Substring(start));
                    if (!f.Universal)
                    {
                        components.Add(f);
                    }
                }
                else
                {
                    names.Add(CreateNameFilter(filter.Substring(start)));
                }
            }
        }
        
        private static void GenerateFilterTokens(string filter, ICollection<NameFilter> names)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return;
            }

            filter = filter.Trim();

            var start = 0;
            for (var i = 0; i < filter.Length - 1; ++i)
            {
                if (filter[i] == ' ')
                {
                    names.Add(CreateNameFilter(filter.Substring(start, i - start)));
                    start = i + 1;
                }
            }

            if (start < filter.Length)
            { 
                names.Add(CreateNameFilter(filter.Substring(start)));
            }
        }

        private static NameFilter CreateNameFilter(string input)
        {
            input = input.Trim();
            var filter = new NameFilter();

            var length = input.Length;
            var current = 0;
            var end = input.Length;

            if (length > current && input[current] == '!')
            {
                filter.Inverted = true;
                ++current;
            }

            if (length > current && input[current] == '^')
            {
                filter.Comparer |= NameFilter.NameComparer.StartsWith;
                ++current;
            }

            if (length > 0 && input[length - 1] == '^')
            {
                filter.Comparer |= NameFilter.NameComparer.EndsWith;
                --end;
            }

            var size = end - current;
            if (current < length && current + size <= length)
            {
                filter.Name = input.Substring(current, size);
            }

            return filter;
        }

        private static ComponentFilter CreateComponentFilter(string input)
        {
            return new ComponentFilter { Name = CreateNameFilter(input) };
        }
    }
}
