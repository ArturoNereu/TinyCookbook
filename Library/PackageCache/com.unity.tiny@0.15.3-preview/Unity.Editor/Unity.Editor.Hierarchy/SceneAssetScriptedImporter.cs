using JetBrains.Annotations;
using Unity.Serialization;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Unity.Editor
{
    [UsedImplicitly]
    [ScriptedImporter(1, new[] {"scene"})]
    internal class SceneAssetScriptedImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            var asset = ScriptableObject.CreateInstance<SceneAsset>();
            var icon = GetThumbnailForAsset(context, asset);
            asset.Icon = icon;

            using (var reader = new SerializedObjectReader(context.assetPath))
            {
                reader.Step(out var root);

                // @TODO This is not very future proof at all...
                reader.Step(NodeType.BeginArray);

                var obj = root.AsObjectView();
                asset.Guid = obj["Guid"].AsStringView().ToString();
                asset.SerializedVersion = (uint) obj["SerializedVersion"].AsUInt64();
            }

            context.AddObjectToAsset("asset", asset, icon);
            context.SetMainObject(asset);
        }

        private Texture2D GetThumbnailForAsset(AssetImportContext context, SceneAsset asset)
        {
            return Icons.Scene;
        }
    }
}
