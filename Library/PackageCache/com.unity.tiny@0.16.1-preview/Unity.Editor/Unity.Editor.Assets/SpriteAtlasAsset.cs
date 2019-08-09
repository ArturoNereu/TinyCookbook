using System.Linq;
using Unity.Editor.Bridge;
using Unity.Entities;
using Unity.Tiny.Core2D;

namespace Unity.Editor.Assets
{
    [EntityWithComponentsBinding(typeof(SpriteAtlas))]
    internal class SpriteAtlasAsset : UnityObjectAsset<UnityEngine.U2D.SpriteAtlas>
    {
        public override AssetInfo GetAssetInfo(IAssetEnumerator context, UnityEngine.U2D.SpriteAtlas atlas)
        {
            return new AssetInfo(atlas, atlas.name);
        }
    }

    internal class SpriteAtlasAssetImporter : UnityObjectAssetImporter<UnityEngine.U2D.SpriteAtlas>
    {
        public override Entity Import(IAssetImporter context, UnityEngine.U2D.SpriteAtlas atlas)
        {
            var entity = context.CreateEntity(typeof(SpriteAtlas));

            var sprites = atlas.GetPackedSprites().Select(s => context.GetEntity(s)).Where(e => e != Entity.Null).ToArray();
            var buffer = context.GetBuffer<SpriteAtlas>(entity).Reinterpret<Entity>();
            foreach (var sprite in sprites)
            {
                buffer.Add(sprite);
            }

            return entity;
        }
    }
}
