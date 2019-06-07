using Unity.Authoring;
using Unity.Editor.Assets;
using Unity.Editor.Extensions;
using Unity.Entities;

namespace Unity.Editor.Build
{
    internal partial class BuildStep
    {
        internal static IBuildStep ExportAssets => new BuildStepExportAssets();

        private class BuildStepExportAssets : IBuildStep
        {
            public string Description => "Exporting assets";

            public bool IsEnabled(BuildPipeline.BuildContext context) => true;

            public bool Run(BuildPipeline.BuildContext context)
            {
                var assetEntities = AssetExporter.Export(context);
                if (assetEntities.Count == 0)
                {
                    return true;
                }

                using (var tmpWorld = new World(AssetsScene.Guid.ToString("N")))
                {
                    // Copy asset entities into temporary world
                    foreach (var pair in assetEntities)
                    {
                        CopyEntity(pair.Value, context.World, tmpWorld);
                    }

                    // Export assets scene
                    var outputFile = context.DataDirectory.GetFile(tmpWorld.Name);
                    if (!ExportWorld(outputFile, context.Project, AssetsScene.Path, tmpWorld))
                    {
                        return false;
                    }

                    // Update manifest
                    context.Manifest.Add(AssetsScene.Guid, AssetsScene.Path, outputFile.AsEnumerable());
                }

                return true;
            }
        }
    }
}
