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
    internal class AssetExporter
    {
        private readonly Dictionary<Type, IUnityObjectAssetExporter> m_AssetExporters = new Dictionary<Type, IUnityObjectAssetExporter>();

        private IWorldManager WorldManager { get; }
        private IAssetManagerInternal AssetManager { get; }
        private EntityManager EntityManager => WorldManager.EntityManager;

        public AssetExporter(Session session)
        {
            WorldManager = session.GetManager<IWorldManager>();
            Assert.IsNotNull(WorldManager);

            AssetManager = session.GetManager<IAssetManagerInternal>();
            Assert.IsNotNull(AssetManager);

            foreach (var pair in DomainCache.AssetExporterTypes)
            {
                m_AssetExporters[pair.Key] = (IUnityObjectAssetExporter)Activator.CreateInstance(pair.Value);
            }
        }

        internal static IReadOnlyDictionary<AssetInfo, Entity> Export(BuildPipeline.BuildContext context)
        {
            var project = context.Project;
            var assetExporter = new AssetExporter(project.Session);

            var assets = assetExporter.AssetManager.EnumerateAssets(project);
            foreach (var pair in assets)
            {
                assetExporter.Export(pair.Key, pair.Value, context);
            }

            return assets;
        }

        private void Export(AssetInfo asset, Entity entity, BuildPipeline.BuildContext context)
        {
            // Get asset exporter
            var assetExporter = m_AssetExporters.Values.FirstOrDefault(x => x.CanExport(asset.Object));
            if (assetExporter == null)
            {
                return; // Assets without asset exporter do not need to export anything
            }

            // Get asset path
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(asset.Object);
            if (string.IsNullOrEmpty(assetPath))
            {
                assetPath = string.Empty;
            }

            // Compute asset export file path
            Assert.IsTrue(EntityManager.HasComponent<EntityGuid>(entity));
            var assetGuid = WorldManager.GetEntityGuid(entity);
            var outputFile = context.DataDirectory.GetFile(assetGuid.ToString("N"));

            // Compute asset export hash
            var exportHash = assetExporter.GetExportHash(asset.Object);
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
                    var manifest = JsonSerialization.Deserialize<BuildManifest.Entry>(manifestFile.FullName);
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
            var didExport = false;
            if (exportedFiles == null)
            {
                exportedFiles = assetExporter.Export(outputFile, asset.Object);
                didExport = exportedFiles != null && exportedFiles.Count() > 0;
            }

            // Update manifest
            var entry = context.Manifest.Add(assetGuid, assetPath, exportedFiles, assetExporter.ExportVersion, exportHash);
            if (entry != null && didExport)
            {
                manifestFile.WriteAllText(JsonSerialization.Serialize(entry));
            }
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
    }

    internal interface IUnityObjectAssetExporter
    {
        uint ExportVersion { get; }
        bool CanExport(UnityEngine.Object obj);
        IEnumerable<FileInfo> Export(FileInfo outputFile, UnityEngine.Object obj);
        Guid GetExportHash(UnityEngine.Object obj);
    }

    internal abstract class UnityObjectAssetExporter<T> : IUnityObjectAssetExporter
        where T : UnityEngine.Object
    {
        public bool CanExport(UnityEngine.Object obj)
        {
            return obj is T;
        }

        public IEnumerable<FileInfo> Export(FileInfo outputFile, UnityEngine.Object obj)
        {
            return Export(outputFile, obj as T);
        }

        public Guid GetExportHash(UnityEngine.Object obj)
        {
            return GetExportHash(obj as T);
        }

        public abstract uint ExportVersion { get; }
        public abstract IEnumerable<FileInfo> Export(FileInfo outputFile, T obj);
        public abstract Guid GetExportHash(T obj);
    }
}
