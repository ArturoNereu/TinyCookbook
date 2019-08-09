using System.Collections.Generic;
using System.Linq;
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

            public bool IsEnabled(BuildPipeline.BuildContext context) => AssetEnumerator.GetAllReferencedAssets(context.Project).Count > 0;

            public bool Run(BuildPipeline.BuildContext context)
            {
                var assetEntities = ExportAssets(context);
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

            private IReadOnlyDictionary<AssetInfo, Entity> ExportAssets(BuildPipeline.BuildContext context)
            {
                var project = context.Project;
                var assetExporter = new AssetExporter(project);

                var assets = context.Session.GetManager<IAssetManagerInternal>().EnumerateAssets(project);
                foreach (var pair in assets)
                {
                    var asset = pair.Key;
                    var entity = pair.Value;

                    // Export asset
                    var exportedFiles = assetExporter.Export(context.DataDirectory, asset, entity);

                    // Update manifest
                    var assetGuid = context.WorldManager.GetEntityGuid(entity);
                    var assetPath = UnityEditor.AssetDatabase.GetAssetPath(asset.Object);
                    context.Manifest.Add(assetGuid, assetPath, exportedFiles);
                }

                return assets;
            }
        }
    }
}
