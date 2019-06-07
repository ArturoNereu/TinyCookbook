using System;
using System.Linq;
using NiceIO;
using Unity.Editor.Extensions;
using Unity.Properties;
using Unity.Serialization.Json;

namespace Unity.Editor.Build
{
    internal partial class BuildStep
    {
        internal static IBuildStep GenerateBeeFiles => new BuildStepGenerateBeeFiles();

        private class BuildStepGenerateBeeFiles : IBuildStep
        {
            private struct BeeBuildSettings
            {
                public struct EmscriptenLinkSettings
                {
                    public uint TOTAL_MEMORY;
                }

                public EmscriptenLinkSettings emscriptenLinkSettings;
            }

            public string Description => "Generating bee files";

            public bool IsEnabled(BuildPipeline.BuildContext context) => true;

            public bool Run(BuildPipeline.BuildContext context)
            {
                BuildProgramDataFileWriter.WriteAll(context.OutputDirectory.FullName);
                WriteBeeExportManifestFile(context);
                WriteBeeBuildSettingsFile(context);
                return true;
            }

            private static void WriteBeeExportManifestFile(BuildPipeline.BuildContext context)
            {
                var file = context.ArtifactsDirectory.GetFile("export.manifest");
                file.UpdateAllLines(context.Manifest.ExportedFiles.Select(x => x.FullName).ToArray());
            }

            private static void WriteBeeBuildSettingsFile(BuildPipeline.BuildContext context)
            {
                var settings = context.Project.Settings;
                var file = context.OutputDirectory.GetFile("buildsettings.json");
                file.UpdateAllText(JsonSerialization.Serialize(new BeeBuildSettings
                {
                    emscriptenLinkSettings = new BeeBuildSettings.EmscriptenLinkSettings
                    {
                        TOTAL_MEMORY = settings.WebSettings.MemorySizeInMB * 1024 * 1024
                    }
                }));
            }
        }
    }
}
