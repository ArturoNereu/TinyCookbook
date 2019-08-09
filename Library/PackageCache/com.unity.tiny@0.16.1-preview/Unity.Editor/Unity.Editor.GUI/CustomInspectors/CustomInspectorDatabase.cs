using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities.Reflection;
using UnityEditor;

namespace Unity.Editor
{
    [InitializeOnLoad]
    internal static class CustomInspectorDatabase
    {
        public class PostProcessor : AssetPostprocessor
        {
            private const string k_UxmlExtension = ".uxml";
            private static void OnPostprocessAllAssets(
                string[] importedAssets,
                string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths)
            {
                if (Process(importedAssets)) return;
                if (Process(deletedAssets)) return;
                if (Process(movedAssets)) return;
            }

            private static bool Process(string[] paths)
            {
                if (!paths.Any(path => path.EndsWith(k_UxmlExtension, StringComparison.InvariantCultureIgnoreCase)))
                    return false;
                if (Application.AuthoringProject != null)
                    EntityInspector.ForceRebuildAll();
                return true;
            }
        }

        private static readonly Dictionary<Type, List<Type>> k_ComponentDataInspectorsPerType;
        private static readonly Dictionary<Type, List<Type>> k_SharedComponentDataInspectorsPerType;
        private static readonly Dictionary<Type, List<Type>> k_BufferElementDataInspectorsPerType;
        private static readonly Dictionary<Type, List<Type>> k_StructInspectorsPerType;

        static CustomInspectorDatabase()
        {
            k_ComponentDataInspectorsPerType = new Dictionary<Type, List<Type>>();
            k_SharedComponentDataInspectorsPerType = new Dictionary<Type, List<Type>>();
            k_BufferElementDataInspectorsPerType = new Dictionary<Type, List<Type>>();
            k_StructInspectorsPerType = new Dictionary<Type, List<Type>>();

            RegisterCustomInspectors();
        }

        internal static IInspector GetCustomInspectorForType(Type type)
            => GetInspectorInstance(k_ComponentDataInspectorsPerType, type);

        internal static IInspector GetSharedComponentInspectorForType(Type type)
            => GetInspectorInstance(k_SharedComponentDataInspectorsPerType, type);

        internal static IInspector GetCustomBufferInspectorForType(Type type)
            => GetInspectorInstance(k_BufferElementDataInspectorsPerType, type);

        internal static IInspector GetCustomStructInspectorForType(Type type)
            => GetInspectorInstance(k_StructInspectorsPerType, type);

        private static IInspector GetInspectorInstance(Dictionary<Type, List<Type>> typeMap, Type type)
        {
            if (typeMap.TryGetValue(type, out var inspector))
            {
                return (IInspector) Activator.CreateInstance(inspector[0]);

            }
            return null;
        }

        private static void RegisterCustomInspectors()
        {
            foreach (var type in EditorTypes.CompiledTypesInEditor)
            {
                RegisterInspectorType(k_ComponentDataInspectorsPerType, typeof(IComponentInspector<>), type);
                RegisterInspectorType(k_SharedComponentDataInspectorsPerType, typeof(ISharedComponentInspector<>), type);
                RegisterInspectorType(k_BufferElementDataInspectorsPerType, typeof(IDynamicBufferInspector<>), type);
                RegisterInspectorType(k_StructInspectorsPerType, typeof(IStructInspector<>), type);
            }
        }

        private static void RegisterInspectorType(Dictionary<Type, List<Type>> typeMap, Type interfaceType, Type inspectorType)
        {
            var inspectorInterface = inspectorType.GetInterface(interfaceType.FullName);
            if (null == inspectorInterface || inspectorType.IsAbstract || inspectorType.ContainsGenericParameters)
            {
                return;
            }

            var genericArguments = inspectorInterface.GetGenericArguments();
            var componentType = genericArguments[0];

            if (!typeMap.TryGetValue(componentType, out var list))
            {
                typeMap[componentType] = list = new List<Type>();
            }

            list.Add(inspectorType);
        }
    }
}
