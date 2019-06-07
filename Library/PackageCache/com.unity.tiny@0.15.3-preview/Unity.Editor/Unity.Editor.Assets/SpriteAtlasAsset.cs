using System.Linq;
using Unity.Editor.Bridge;
using Unity.Entities;
using Unity.Tiny.Core2D;

namespace Unity.Editor.Assets
{
    [EntityWithComponentsBinding(typeof(SpriteAtlas))]
    internal class SpriteAtlasAsset : UnityObjectAsset<UnityEngine.U2D.SpriteAtlas>
    {
        public override AssetInfo GetAssetInfo(IAssetEnumerator ctx, UnityEngine.U2D.SpriteAtlas atlas)
        {
            return new AssetInfo(atlas, atlas.name);
        }
    }

    internal class SpriteAtlasAssetImporter : UnityObjectAssetImporter<UnityEngine.U2D.SpriteAtlas>
    {
        public override Entity Import(IAssetImporter ctx, UnityEngine.U2D.SpriteAtlas atlas)
        {
            var entity = ctx.CreateEntity(typeof(SpriteAtlas));

            var sprites = atlas.GetPackedSprites().Select(s => ctx.GetEntity(s)).Where(e => e != Entity.Null).ToArray();
            var buffer = ctx.GetBuffer<SpriteAtlas>(entity).Reinterpret<Entity>();
            foreach (var sprite in sprites)
            {
                buffer.Add(sprite);
            }

            return entity;
        }
    }
}
