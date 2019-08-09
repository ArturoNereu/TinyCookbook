using JetBrains.Annotations;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Unity.Editor
{
    [UsedImplicitly]
    [ScriptedImporter(1, new[] { "project" })]
    internal class ProjectScriptedImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            var asset = ScriptableObject.CreateInstance<ProjectAsset>();
            context.AddObjectToAsset("asset", asset, Icons.Project);
            context.SetMainObject(asset);
        }
    }
}
