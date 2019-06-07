using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Authoring.Hashing;
using Unity.Editor.Extensions;
using Unity.Entities;
using Unity.Tiny.Audio;

namespace Unity.Editor.Assets
{
    [EntityWithComponentsBinding(typeof(AudioClip))]
    internal class AudioClipAsset : UnityObjectAsset<UnityEngine.AudioClip>
    {
    }

    internal class AudioClipAssetImporter : UnityObjectAssetImporter<UnityEngine.AudioClip>
    {
        public override Entity Import(IAssetImporter ctx, UnityEngine.AudioClip audioClip)
        {
            var entity = ctx.CreateEntity(typeof(AudioClip), typeof(AudioClipLoadFromFile), typeof(AudioClipLoadFromFileAudioFile));
            ctx.SetBufferFromString<AudioClipLoadFromFileAudioFile>(entity, "Data/" + audioClip.GetGuid().ToString("N"));
            return entity;
        }
    }

    internal class AudioClipAssetExporter : UnityObjectAssetExporter<UnityEngine.AudioClip>
    {
        public override uint ExportVersion => 1;

        public override IEnumerable<FileInfo> Export(FileInfo outputFile, UnityEngine.AudioClip audioClip)
        {
            return AssetExporter.ExportSource(outputFile, audioClip);
        }

        public override Guid GetExportHash(UnityEngine.AudioClip audioClip)
        {
            var bytes = new List<byte>();

            // Add audio clip importer settings bytes
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(audioClip);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var importer = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
                if (importer != null)
                {
                    var importerJson = UnityEditor.EditorJsonUtility.ToJson(importer);
                    bytes.AddRange(Encoding.ASCII.GetBytes(importerJson));
                }

                // Add audio clip content bytes
                bytes.AddRange(File.ReadAllBytes(assetPath));
            }

            // New guid from bytes
            return GuidUtility.NewGuid(bytes.ToArray());
        }
    }
}
