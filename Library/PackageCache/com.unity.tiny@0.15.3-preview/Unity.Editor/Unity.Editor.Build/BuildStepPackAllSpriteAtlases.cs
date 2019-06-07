using Unity.Editor.Bridge;
using Unity.Editor.Utilities;

namespace Unity.Editor.Build
{
    internal partial class BuildStep
    {
        internal static IBuildStep PackAllSpriteAtlases => new BuildStepPackAllSpriteAtlases();

        private class BuildStepPackAllSpriteAtlases : IBuildStep
        {
            public string Description => "Packing all sprite atlases";

            public bool IsEnabled(BuildPipeline.BuildContext context)
            {
                return UnityEditor.AssetDatabase.FindAssets($"t:{typeof(UnityEngine.U2D.SpriteAtlas).Name}").Length > 0;
            }

            public bool Run(BuildPipeline.BuildContext context)
            {
                // FIXME: Have PackAllSpriteAtlases return whether or not any sprite
                // atlas needed packing, and refresh asset entity cache only if true.
                SpriteAtlasBridge.PackAllSpriteAtlases();

                // Restore progress bar because pack sprite atlases will dispose it
                ProgressBarScope.Restore();

                // Refresh all asset entities since packing sprite atlases might have changed the structure
                context.Session.GetManager<IAssetManagerInternal>().Refresh();
                return true;
            }
        }
    }
}
