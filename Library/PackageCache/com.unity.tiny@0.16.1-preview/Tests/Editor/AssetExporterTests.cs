using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Authoring.Hashing;
using Unity.Editor.Assets;
using Unity.Editor.Extensions;
using Unity.Entities;

namespace Unity.Editor.Tests
{
    internal class AssetExporterTests
    {
        private const string k_ProjectPath = "Assets/Samples/HelloWorld/HelloWorld.project";

        private struct AssetFileStats : IEquatable<AssetFileStats>
        {
            public string Name;
            public Guid ContentHash;
            public long Size;

            public bool Equals(AssetFileStats other)
            {
                return Name == other.Name && ContentHash == other.ContentHash && Size == other.Size;
            }
        }

        [Test, TestCase(k_ProjectPath)]
        public void AssetExport(string projectPath)
        {
            var projectFile = new FileInfo(projectPath);
            if (!projectFile.Exists)
            {
                UnityEngine.Debug.LogWarning($"Skipping {nameof(AssetExport)} test case since {projectPath} could not be found.");
                return;
            }

            using (var project = Project.Open(projectFile))
            {
                // Wipe-out output directory
                var outputDir = Application.OutputDirectory.Combine(project.Name.ToIdentifier()).Combine("Data");
                if (Directory.Exists(outputDir.FullName))
                {
                    outputDir.Delete(true);
                }
                outputDir.EnsureExists();
                Assert.That(outputDir.GetFiles(), Is.Empty);

                // Enumerate assets
                var assetExporter = new AssetExporter(project);
                var assets = project.Session.GetManager<IAssetManagerInternal>().EnumerateAssets(project);

                // Export assets and remember the file creation time and size
                var initialAssetFileStats = AssertExportAssets(outputDir, assetExporter, assets);

                // Re-export assets and gather file creation time and size
                var newAssetFileStats = AssertExportAssets(outputDir, assetExporter, assets);

                // Compare, they should be equal since re-exporting should do nothing
                Assert.That(initialAssetFileStats, Is.EqualTo(newAssetFileStats));
            }
        }

        private IReadOnlyList<AssetFileStats> AssertExportAssets(DirectoryInfo outputDir, AssetExporter assetExporter, IReadOnlyDictionary<AssetInfo, Entity> assets)
        {
            var fileStats = new List<AssetFileStats>();
            foreach (var pair in assets)
            {
                var exportedFiles = assetExporter.Export(outputDir, pair.Key, pair.Value);
                foreach (var exportedFile in exportedFiles)
                {
                    Assert.IsTrue(exportedFile.Exists);

                    var manifest = new AssetFileStats
                    {
                        Name = exportedFile.Name,
                        ContentHash = GuidUtility.NewGuid(exportedFile),
                        Size = exportedFile.Length
                    };

                    Assert.IsFalse(string.IsNullOrEmpty(manifest.Name));
                    Assert.That(manifest.ContentHash, Is.Not.EqualTo(Guid.Empty));
                    Assert.Greater(manifest.Size, 0);

                    fileStats.Add(manifest);
                }
            }
            return fileStats;
        }
    }
}
