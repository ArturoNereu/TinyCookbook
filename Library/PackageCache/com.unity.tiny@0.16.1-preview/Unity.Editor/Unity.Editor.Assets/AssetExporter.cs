using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Authoring;
using Unity.Editor.Build;
using Unity.Editor.Extensions;
using Unity.Entities;
using Unity.Serialization.Json;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Editor.Assets
{
    internal interface IAssetExporter
    {
        ProjectSettings Settings { get; }
    }

    internal class AssetExporter : IAssetExporter
    {
        private readonly Dictionary<Type, IUnityObjectAssetExporter> m_AssetExporters = new Dictionary<Type, IUnityObjectAssetExporter>();

        private class ExportManifest
        {
            public Guid AssetGuid;
            public string AssetPath;
            public uint ExportVersion;
            public Guid ExportHash;
            public List<string> ExportedFiles = new List<string>();
        }

        private IWorldManager WorldManager { get; }
        private EntityManager EntityManager => WorldManager.EntityManager;
        private IAssetManagerInternal AssetManager { get; }

        public AssetExporter(Project project)
        {
            WorldManager = project.Session.GetManager<IWorldManager>();
            Assert.IsNotNull(WorldManager);

            AssetManager = project.Session.GetManager<IAssetManagerInternal>();
            Assert.IsNotNull(AssetManager);

            foreach (var pair in DomainCache.AssetExporterTypes)
            {
                m_AssetExporters[pair.Key] = (IUnityObjectAssetExporter)Activator.CreateInstance(pair.Value);
            }

            Settings = project.Settings;
        }

        internal IEnumerable<FileInfo> Export(DirectoryInfo outputDir, AssetInfo asset, Entity entity)
        {
            // Get asset exporter
            var assetExporter = m_AssetExporters.Values.FirstOrDefault(x => x.CanExport(asset.Object));
            if (assetExporter == null)
            {
                return Enumerable.Empty<FileInfo>(); // Assets without asset exporter do not need to export anything
            }

            // Get asset path
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(asset.Object);
            if (assetPath == null)
            {
                assetPath = string.Empty;
            }

            // Compute asset export file path
            Assert.IsTrue(EntityManager.HasComponent<EntityGuid>(entity));
            var assetGuid = WorldManager.GetEntityGuid(entity);
            var outputFile = outputDir.GetFile(assetGuid.ToString("N"));

            // Compute asset export hash
            var exportHash = assetExporter.GetExportHash(this, asset.Object);
            if (exportHash == Guid.Empty)
            {
                throw new InvalidOperationException($"{assetExporter.GetType().FullName} did not provide a valid asset export hash.");
            }

            // Retrieve exported files from manifest
            IEnumerable<FileInfo> exportedFiles = null;
            var manifestFile = new FileInfo(outputFile.FullName + ".manifest");
            if (manifestFile.Exists)
            {
                try
                {
                    var manifest = JsonSerialization.Deserialize<ExportManifest>(manifestFile.FullName);
                    if (manifest != null &&
                        manifest.AssetPath == assetPath &&
                        manifest.AssetGuid == assetGuid &&
                        manifest.ExportVersion == assetExporter.ExportVersion &&
                        manifest.ExportHash == exportHash)
                    {
                        // Verify that all files exists
                        var files = manifest.ExportedFiles.Select(x => new FileInfo(x));
                        if (files.Where(file => file.Exists).Count() == manifest.ExportedFiles.Count)
                        {
                            exportedFiles = files;
                        }
                    }
                }
                catch (InvalidJsonException)
                {
                    // Manifest file couldn't be read, format might have changed
                }
            }

            // Export asset if export files are not found
            if (exportedFiles == null)
            {
                exportedFiles = assetExporter.Export(this, outputFile, asset.Object);

                // Update export manifest
                manifestFile.WriteAllText(JsonSerialization.Serialize(new ExportManifest
                {
                    AssetGuid = assetGuid,
                    AssetPath = assetPath,
                    ExportVersion = assetExporter.ExportVersion,
                    ExportHash = exportHash,
                    ExportedFiles = exportedFiles.Select(f => f.FullName).ToList()
                }));
            }

            return exportedFiles;
        }

        internal static IEnumerable<FileInfo> ExportSource(FileInfo outputFile, UnityEngine.Object obj)
        {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new NullReferenceException($"Asset path for object '{obj.name}' not found in AssetDatabase.");
            }

            var srcFile = Application.RootDirectory.GetFile(assetPath);
            if (!srcFile.Exists)
            {
                throw new FileNotFoundException(srcFile.FullName);
            }

            outputFile.Directory.Create();
            return srcFile.CopyTo(outputFile.FullName, true).AsEnumerable();
        }

        #region IAssetExporter

        public ProjectSettings Settings { get; }

        #endregion
    }

    internal interface IUnityObjectAssetExporter
    {
        uint ExportVersion { get; }
        bool CanExport(UnityEngine.Object obj);
        IEnumerable<FileInfo> Export(IAssetExporter context, FileInfo outputFile, UnityEngine.Object obj);
        Guid GetExportHash(IAssetExporter context, UnityEngine.Object obj);
    }

    internal abstract class UnityObjectAssetExporter<T> : IUnityObjectAssetExporter
        where T : UnityEngine.Object
    {
        public bool CanExport(UnityEngine.Object obj)
        {
            return obj is T;
        }

        public IEnumerable<FileInfo> Export(IAssetExporter context, FileInfo outputFile, UnityEngine.Object obj)
        {
            return Export(context, outputFile, obj as T);
        }

        public Guid GetExportHash(IAssetExporter context, UnityEngine.Object obj)
        {
            return GetExportHash(context, obj as T);
        }

        public abstract uint ExportVersion { get; }
        public abstract IEnumerable<FileInfo> Export(IAssetExporter context, FileInfo outputFile, T obj);
        public abstract Guid GetExportHash(IAssetExporter context, T obj);
    }
}
