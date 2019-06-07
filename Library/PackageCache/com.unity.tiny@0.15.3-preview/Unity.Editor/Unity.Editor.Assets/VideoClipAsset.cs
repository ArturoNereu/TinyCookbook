using System;
using Unity.Entities;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Unity.Editor.Extensions;
using Unity.Authoring.Hashing;

namespace Unity.Editor.Assets
{ 
    [EntityWithComponentsBinding(typeof(Unity.Tiny.Video.VideoClip))]
    internal class VideoClipAsset : UnityObjectAsset<UnityEngine.Video.VideoClip>
    {
        public override AssetInfo GetAssetInfo(IAssetEnumerator ctx, UnityEngine.Video.VideoClip clip)
        {
            return new AssetInfo(clip, clip.name);
        }
    }
    internal class VideoClipAssetImporter : UnityObjectAssetImporter<UnityEngine.Video.VideoClip>
    {
        public override Entity Import(IAssetImporter ctx, UnityEngine.Video.VideoClip clip)
        {
            var entityVideo = ctx.CreateEntity(typeof(Unity.Tiny.Video.VideoClip), typeof(Unity.Tiny.Video.VideoClipLoadFromFile));
            ctx.AddBufferFromString<Unity.Tiny.Video.VideoClipLoadFromFileName>(entityVideo, "Data/" + clip.GetGuid().ToString("N"));
            return entityVideo;
        }
    }

    internal class VideoClipAssetExporter : UnityObjectAssetExporter<UnityEngine.Video.VideoClip>
    {
        public override uint ExportVersion => 1;

        public override IEnumerable<FileInfo> Export(FileInfo outputFile, UnityEngine.Video.VideoClip clip)
        {
             return AssetExporter.ExportSource(outputFile, clip);
        }

        public override Guid GetExportHash(UnityEngine.Video.VideoClip clip)
        {
            var bytes = new List<byte>();

            // Add clip importer settings bytes
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(clip);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var importer = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.VideoClipImporter;
                if (importer != null)
                {
                    var importerJson = UnityEditor.EditorJsonUtility.ToJson(importer);
                    bytes.AddRange(Encoding.ASCII.GetBytes(importerJson));
                }

                // Add hash file
                bytes.AddRange(File.ReadAllBytes(assetPath));
            }
            
            // New id from bytes
            return GuidUtility.NewGuid(bytes.ToArray());
        }
    }
}
