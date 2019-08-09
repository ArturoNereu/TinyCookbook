using System.IO;
using UnityEditor;

namespace Unity.Editor.Build
{
    internal static class BuildShortcut
    {
        //Convenience method for CI workflow so they can quickly create all files required by the buildprogram
        //to build the samples without using the editor UI
        internal static void UpdateAsmDefsJson()
        {
            var dir = UnityEngine.Application.dataPath;
            var bootstrapFolder = Path.GetFullPath(dir + "/../Bootstrap");

            if (!Directory.Exists(bootstrapFolder))
                Directory.CreateDirectory(bootstrapFolder);
        
            BuildProgramDataFileWriter.WriteAll(bootstrapFolder);
            Debug.Log("Bee files written to: " + bootstrapFolder);
        }
    }
}
