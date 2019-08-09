using JetBrains.Annotations;
using Unity.Serialization;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Unity.Editor
{
    [UsedImplicitly]
    [ScriptedImporter(1, new[] { "configuration" })]
    internal class ConfigurationAssetScriptedImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            var asset = ScriptableObject.CreateInstance<ConfigurationAsset>();
            context.AddObjectToAsset("asset", asset);
            context.SetMainObject(asset);
        }
    }
}
