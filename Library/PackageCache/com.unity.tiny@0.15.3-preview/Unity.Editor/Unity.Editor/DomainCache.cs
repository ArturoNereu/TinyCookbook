using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Editor.Assets;
using Unity.Editor.Extensions;
using UnityEngine.Assertions;

namespace Unity.Editor
{
    internal static class DomainCache
    {
        public static IReadOnlyDictionary<Type, Type> AssetImporterTypes => AssetImporterCache.Types;
        public static IReadOnlyDictionary<Type, Type> AssetExporterTypes => AssetExporterCache.Types;
        public static IReadOnlyDictionary<Type, Type> UnityObjectAssetTypes => UnityObjectAssetCache.Types;

        private static readonly Dictionary<Guid, HashSet<Assembly>> k_IncludedAssembliesPerProject = new Dictionary<Guid, HashSet<Assembly>>();

        public static T GetDefaultValue<T>() where T : struct
        {
            return DefaultValueCache<T>.DefaultValue;
        }

        public static object GetDefaultValue(Type type)
        {
            Assert.IsTrue(type.IsValueType);
            var generic = typeof(DefaultValueCache<>)
                .MakeGenericType(type);
            RuntimeHelpers.RunClassConstructor(generic.TypeHandle);
            return generic.GetField("DefaultValue", BindingFlags.Public | BindingFlags.Static)?
                .GetValue(null);
        }

        public static Type GetAssetType(Type type)
        {
            return null == type ? null : UnityObjectAssetCache.GetAssetType(type);
        }

        internal static void CacheIncludedAssemblies(Project project)
        {
            if (null == project)
            {
                return;
            }
            var file = project.GetProjectFile();
            var set = new HashSet<Assembly>(project.IncludedAssemblies());
            k_IncludedAssembliesPerProject[project.Guid] = set;
        }

        public static bool IsIncludedInProject(Project project, Type type)
            => IsIncludedInProject(project, type.Assembly);

        public static bool IsIncludedInProject(Project project, Assembly assembly)
        {
            if (null == project)
            {
                return false;
            }

            if (k_IncludedAssembliesPerProject.TryGetValue(project.Guid, out var set))
            {
                return set.Contains(assembly);
            }

            return false;
        }

        private static class DefaultValueCache<T> where T : struct
        {
            static DefaultValueCache()
            {
                try
                {
                    var type = typeof(T);
                    var defaultProperty = type.GetProperty("Default", BindingFlags.Public | BindingFlags.Static);
                    if (defaultProperty != null && defaultProperty.GetMethod != null &&
                        defaultProperty.GetMethod.ReturnType == type)
                    {
                        DefaultValue = (T) defaultProperty.GetValue(null);
                    }
                }
                catch (Exception ex)
                {
                    // trap to avoid repeated TypeInitializationException
                    UnityEngine.Debug.LogException(ex);
                }
            }

            public static readonly T DefaultValue;
        }

        private static class AssetImporterCache
        {
            private static readonly Dictionary<Type, Type> m_AssetImporterTypes = new Dictionary<Type, Type>();

            public static IReadOnlyDictionary<Type, Type> Types => m_AssetImporterTypes;

            static AssetImporterCache()
            {
                var types = UnityEditor.TypeCache.GetTypesDerivedFrom<IUnityObjectAssetImporter>();
                foreach (var type in types)
                {
                    if (type.BaseType == null || !type.BaseType.IsSubclassOfRawGeneric(typeof(UnityObjectAssetImporter<>)))
                    {
                        continue;
                    }

                    var assetType = type.BaseType.GenericTypeArguments.FirstOrDefault();
                    if (!m_AssetImporterTypes.TryGetValue(assetType, out var registeredType))
                    {
                        m_AssetImporterTypes[assetType] = type;
                    }
                    else
                    {
                        Debug.LogError($"Asset type '{assetType.FullName}' is already registered by '{registeredType.FullName}'.");
                    }
                }
            }
        }

        private static class AssetExporterCache
        {
            private static readonly Dictionary<Type, Type> m_AssetExporterTypes = new Dictionary<Type, Type>();

            public static IReadOnlyDictionary<Type, Type> Types => m_AssetExporterTypes;

            static AssetExporterCache()
            {
                var types = UnityEditor.TypeCache.GetTypesDerivedFrom<IUnityObjectAssetExporter>();
                foreach (var type in types)
                {
                    if (type.BaseType == null || !type.BaseType.IsSubclassOfRawGeneric(typeof(UnityObjectAssetExporter<>)))
                    {
                        continue;
                    }

                    var assetType = type.BaseType.GenericTypeArguments.FirstOrDefault();
                    if (!m_AssetExporterTypes.TryGetValue(assetType, out var registeredType))
                    {
                        m_AssetExporterTypes[assetType] = type;
                    }
                    else
                    {
                        Debug.LogError($"Asset type '{assetType.FullName}' is already registered by '{registeredType.FullName}'.");
                    }
                }
            }
        }

        private static class UnityObjectAssetCache
        {
            private static readonly Dictionary<Type, Type> m_UnityObjectAssetTypes = new Dictionary<Type, Type>();
            private static readonly Dictionary<Type, Type> m_AssetTypeForType = new Dictionary<Type, Type>();

            public static IReadOnlyDictionary<Type, Type> Types => m_UnityObjectAssetTypes;
            public static Type GetAssetType(Type type) => m_AssetTypeForType.TryGetValue(type, out var asset) ? asset : null;

            static UnityObjectAssetCache()
            {
                var types = UnityEditor.TypeCache.GetTypesDerivedFrom<IUnityObjectAssetEnumerator>();
                foreach (var type in types)
                {
                    if (type.BaseType == null || !type.BaseType.IsSubclassOfRawGeneric(typeof(UnityObjectAsset<>)))
                    {
                        continue;
                    }

                    var assetType = type.BaseType.GenericTypeArguments.FirstOrDefault();
                    if (!m_UnityObjectAssetTypes.TryGetValue(assetType, out var registeredType))
                    {
                        m_UnityObjectAssetTypes[assetType] = type;
                    }
                    else
                    {
                        Debug.LogError($"Asset type '{assetType.FullName}' is already registered by '{registeredType.FullName}'.");
                    }

                    var binder = type.GetCustomAttribute<EntityWithComponentsBindingAttribute>();
                    // TODO: Support multiple types
                    if (null != binder && binder.Types.FirstOrDefault() is var mainType && null != mainType)
                    {
                        m_AssetTypeForType[mainType] = assetType;
                    }
                }
            }
        }
    }
}
