using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Serialization.Json;
using NiceIO;
using Unity.Editor.Extensions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Unity.Editor.Build
{
    internal static class BuildProgramDataFileWriter
    {
        // Writing asmdefs takes an annoying long time. Let's rely on the fact that it is impossible to change your asmdef layout without
        // a scripting domain reload happening. A domain reload will cause this static field to be set back to false, so if we see it
        // is true, we know that things haven't changed, so we can stop spending 300ms figuring out where all asmdefs live.
        public static bool alreadyWrittenDataFile = false;
        
        private class AsmDefJsonObject
        {
            [SerializeField] public string name = null;
        }

        public static void WriteAll(string directory, string selectedConfig = null)
        {
            WriteAsmdefsJson(directory);
            WriteBeeConfigFile(directory);
            WriteBeeBatchFile(directory);
            if (selectedConfig != null)
                WriteSelectedConfigFile(directory, selectedConfig);
        }

        private static void WriteSelectedConfigFile(NPath directory, string selectedConfig)
        {
            var file = directory.Combine("selectedconfig.json").MakeAbsolute();
            file.UpdateAllText(JsonSerialization.Serialize(new SelectedConfigJson()
            {
                Config = selectedConfig
            }));
        }

        private static void WriteBeeBatchFile(NPath directory)
        {
            var file = directory.Combine("bee");
            
            // Then write out some helper bee/bee.cmd scripts
            using (StreamWriter sw = new StreamWriter(file.ToString()))  
            {  
                sw.NewLine = "\n";
                sw.WriteLine($@"#!/bin/sh");
                sw.WriteLine();
                sw.WriteLine("MONO=");
                sw.WriteLine($@"BEE=""$PWD/{BeePath.RelativeTo(directory).ToString(SlashMode.Forward)}""");
                sw.WriteLine("BEE=$(printf %q \"$BEE\")");
                sw.WriteLine($@"if [ ""$APPDATA"" == """" ] ; then");
                sw.WriteLine("    MONO=mono");
                sw.WriteLine("fi");
                sw.WriteLine("cmdToRun=\"${MONO} ${BEE} $*\"");
                sw.WriteLine("if [ $# -eq 0 ]; then");
                sw.WriteLine("    eval \"${cmdToRun} -t\"");
                sw.WriteLine("  else");
                sw.WriteLine("    eval \"${cmdToRun}\"");
                sw.WriteLine("fi");
            }

            var cmdFile = directory.Combine("bee.cmd");
            using (StreamWriter sw = new StreamWriter(cmdFile.ToString()))  
            {  
                sw.NewLine = "\n";
                sw.WriteLine("@ECHO OFF");
                sw.WriteLine($@"set bee=%~dp0{BeePath.RelativeTo(directory).ToString(SlashMode.Backward)}");
                sw.WriteLine($@"if [%1] == [] (%bee% -t) else (%bee% %*)");
            }
        }

        private static NPath BeePath { get; } = Path.GetFullPath("Packages/com.unity.tiny/DotsPlayer/bee~/bee.exe");

        static void WriteAsmdefsJson(NPath directory)
        {
            var file = directory.Combine("asmdefs.json");

            if (file.FileExists() && alreadyWrittenDataFile)
                return;
                
            var asmdefs = new List<AsmDefDescription>();
            foreach (var asmdefFile in AllAssemblyDefinitions())
            {
                var asmdef = JsonUtility.FromJson<AsmDefJsonObject>(asmdefFile.MakeAbsolute().ReadAllText());
                var packageInfo = PackageInfo.FindForAssetPath(asmdefFile.ToString());
                var packageSource = packageInfo?.source.ToString() ?? "NoPackage";
                asmdefs.Add(new AsmDefDescription()
                {
                    AsmdefName = asmdef.name, FullPath = Path.GetFullPath(asmdefFile.ToString()), PackageSource = packageSource
                });
            }

            var projectPath = new NPath(UnityEngine.Application.dataPath).Parent;
            file.UpdateAllText(JsonSerialization.Serialize(new BeeAsmdefConfiguration()
            {
                asmdefs = asmdefs,
                UnityProjectPath = projectPath.ToString(),
                ProjectName = projectPath.FileName,
            }));
            alreadyWrittenDataFile = true;
        }

        private static IEnumerable<NPath> AllAssemblyDefinitions()
        {
            var paths = new HashSet<string>();
            var guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");

            foreach (var guid in guids)
            {
                var asmdefPath = AssetDatabase.GUIDToAssetPath(guid);
                paths.Add(asmdefPath);
            }

            foreach (var assembly in CompilationPipeline.GetAssemblies())
            {
                var path = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);
                if (path == null)
                {
                    continue;
                }
                paths.Add(path);
            }

            foreach (var path in paths.OrderBy(p => p))
            {
                // this creates a world of problems
                //if (AssemblyDefinitionUtility.IsRuntimeAssembly(path))
                {
                    yield return new NPath(path);
                }
            }
        }

        private struct BeeAsmdefConfiguration
        {
            public List<AsmDefDescription> asmdefs;
            public string UnityProjectPath;
            public string ProjectName;
        }

        private struct AsmDefDescription
        {
            public string AsmdefName;
            public string FullPath;
            public string PackageSource;
        }

        static void WriteBeeConfigFile(NPath directory)
        {
            var file = directory.Combine("bee.config");
            file.UpdateAllText(JsonSerialization.Serialize(new BeeConfig
            {
                BuildProgramBuildProgramFiles = new List<string>
                {
                    Path.GetFullPath("Packages/com.unity.tiny/DotsPlayer/bee~/BuildProgramBuildProgramSources")
                },
                MultiDag = true
            }));
        }

        private struct BeeConfig
        {
            public List<string> BuildProgramBuildProgramFiles;
            public bool MultiDag;
        }
    }

    internal class SelectedConfigJson
    {
        public string Config;
    }
}