using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Unity.Authoring.Hashing;
using Unity.Editor.Extensions;
using Unity.Entities;
using Unity.Tiny.Audio;

namespace Unity.Editor.Assets
{
    [EntityWithComponentsBinding(typeof(AudioClip)), UsedImplicitly]
    internal class AudioClipAsset : UnityObjectAsset<UnityEngine.AudioClip>
    {
    }

    [UsedImplicitly]
    internal class AudioClipAssetImporter : UnityObjectAssetImporter<UnityEngine.AudioClip>
    {
        public override Entity Import(IAssetImporter context, UnityEngine.AudioClip audioClip)
        {
            // If any UnityEditor.AudioImporter properties are used here, make sure
            // that GetExportHash will also take UnityEditor.AudioImporter into account.
            var entity = context.CreateEntity(typeof(AudioClip), typeof(AudioClipLoadFromFile), typeof(AudioClipLoadFromFileAudioFile));
            context.SetBufferFromString<AudioClipLoadFromFileAudioFile>(entity, "Data/" + audioClip.GetGuid().ToString("N"));
            return entity;
        }
    }

    [UsedImplicitly]
    internal class AudioClipAssetExporter : UnityObjectAssetExporter<UnityEngine.AudioClip>
    {
        public override uint ExportVersion => 1;

        public override IEnumerable<FileInfo> Export(IAssetExporter context, FileInfo outputFile, UnityEngine.AudioClip audioClip)
        {
            return AssetExporter.ExportSource(outputFile, audioClip);
        }

        public override Guid GetExportHash(IAssetExporter context, UnityEngine.AudioClip audioClip)
        {
            var bytes = new List<byte>();

            // Add audio clip importer settings bytes
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(audioClip);
            if (!string.IsNullOrEmpty(assetPath))
            {
                // Add audio clip content bytes
                bytes.AddRange(File.ReadAllBytes(assetPath));
            }

            // New guid from bytes
            return GuidUtility.NewGuid(bytes.ToArray());
        }
    }
}
