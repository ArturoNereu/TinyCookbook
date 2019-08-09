using System;
using System.Collections.Generic;

namespace Unity.Entities.Reflection
{
    internal static class EditorTypes
    {
        private static readonly List<Type> s_CompiledTypesInEditor = new List<Type>();

        static EditorTypes()
        {
            PopulateAllTypesCompiledInTheEditor(s_CompiledTypesInEditor);
        }

        /// <summary>
        /// Returns all the types compiled by the Unity Editor.
        /// </summary>
        public static IEnumerable<Type> CompiledTypesInEditor => s_CompiledTypesInEditor;

        // TODO: Move this method back to a bridge
        private static void PopulateAllTypesCompiledInTheEditor(List<Type> types)
        {
            var assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies();
            foreach (var unityAssembly in assemblies)
            {
                try
                {
                    var assembly = AppDomain.CurrentDomain.Load(unityAssembly.name);
                    types.AddRange(assembly.GetTypes());
                }
                catch (System.IO.FileNotFoundException)
                {
                    // Skip.
                }
            }
        }
    }
}
