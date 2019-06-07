using System;
using System.Reflection;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEditor.Searcher;

namespace Unity.Editor
{
    internal static class ComponentSearcherDatabases
    {
        public static readonly AddComponentSearcherAdapter SearcherAdapter;

        static ComponentSearcherDatabases()
        {
            SearcherAdapter = new AddComponentSearcherAdapter("Add Components");
        }

        // We need to temporarily ignore some types in the editor until we resolve some conflicts.
        private static bool ShouldTemporarilyIgnoreType(Type type)
        {
            var assemblyName = type.Assembly.GetName().Name;
            const string transforms = "Unity.Transforms";
            if (assemblyName == transforms || type.Namespace == transforms)
            {
                return true;
            }

            if (assemblyName.EndsWith("Tests") || (type.Namespace?.EndsWith("Tests") ?? false))
            {
                return true;
            }

            return false;
        }

        public static ECSComponentDatabase DynamicPopulateComponents(NativeHashMap<int, int> map)
        {
            return Populate<IComponentData>(map);
        }
        
        public static ECSComponentDatabase DynamicPopulateSharedComponents(NativeHashMap<int, int> map)
        {
            return Populate<ISharedComponentData>(map);
        }
        
        public static ECSComponentDatabase DynamicPopulateBufferComponents(NativeHashMap<int, int> map)
        {
            return Populate<IBufferElementData>(map);
        }

        private static ECSComponentDatabase Populate<T>(NativeHashMap<int, int> map)
        {
            using (var pooledList = ListPool<SearcherItem>.GetDisposable())
            using (var pooledDict = DictionaryPool<string, SearcherItem>.GetDisposable())
            {
                var list = pooledList.List;
                var dict = pooledDict.Dictionary;
                var componentRoot = new SearcherItem(typeof(T).Name);
                list.Add(componentRoot);

                var collection = TypeCache.GetTypesDerivedFrom<T>();
                foreach (var type in collection)
                {
                    if (type.IsGenericType || type.IsAbstract || type.ContainsGenericParameters)
                    {
                        continue;
                    }

                    if (null != type.GetCustomAttribute<HideInInspectorAttribute>() ||
                        null != type.Assembly.GetCustomAttribute<HideInInspectorAttribute>() ||
                        ShouldTemporarilyIgnoreType(type))
                    {
                        continue;
                    }

                    if (typeof(ISystemStateComponentData).IsAssignableFrom(type) ||
                        typeof(ISystemStateSharedComponentData).IsAssignableFrom(type) ||
                        typeof(ISystemStateBufferElementData).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    try
                    {
                        var index = TypeManager.GetTypeIndex(type);
                        if (!map.TryGetValue(index, out _))
                        {
                            var component = new TypeSearcherItem(type);
                            var @namespace = type.Namespace ?? "Global"; 
                            if (!dict.TryGetValue(@namespace, out var item))
                            {
                                dict[@namespace] = item = new SearcherItem(@namespace);
                                componentRoot.AddChild(item);
                            }

                            item.AddChild(component);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                foreach (var kvp in dict)
                {
                    kvp.Value.Children.Sort(CompareByName);
                }
                
                componentRoot.Children.Sort(CompareByName);

                return new ECSComponentDatabase(list);
            }
        }

        private static int CompareByName(SearcherItem x, SearcherItem y)
        {
            return string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
