using Unity.Editor.Bridge;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace Unity.Editor.Assets
{
    [EntityWithComponentsBinding(typeof(Sprite2D))]
    internal class SpriteAsset : UnityObjectAsset<UnityEngine.Sprite>
    {
        public override AssetInfo GetAssetInfo(IAssetEnumerator ctx, UnityEngine.Sprite sprite)
        {
            return new AssetInfo(sprite, sprite.name, ctx.GetAssetInfo(GetSpriteTexture(sprite)));
        }

        internal static UnityEngine.Texture2D GetSpriteTexture(UnityEngine.Sprite sprite)
        {
            return sprite.IsInAtlas() ? sprite.GetAtlasTexture() : sprite.texture;
        }

        internal static UnityEngine.Rect GetSpriteTextureRect(UnityEngine.Sprite sprite)
        {
            return sprite.IsInAtlas() ? sprite.GetAtlasTextureRect() : sprite.textureRect;
        }

        internal static UnityEngine.Vector2 GetSpriteTextureRectOffset(UnityEngine.Sprite sprite)
        {
            return sprite.IsInAtlas() ? sprite.GetAtlasTextureRectOffset() : sprite.textureRectOffset;
        }
    }

    internal class SpriteAssetImporter : UnityObjectAssetImporter<UnityEngine.Sprite>
    {
        public override Entity Import(IAssetImporter ctx, UnityEngine.Sprite sprite)
        {
            var entity = ctx.CreateEntity(typeof(Sprite2D));

            var texture = SpriteAsset.GetSpriteTexture(sprite);
            var rect = SpriteAsset.GetSpriteTextureRect(sprite);
            var offset = SpriteAsset.GetSpriteTextureRectOffset(sprite);
            var pivot = sprite.pivot;
            var border = sprite.border;

            ctx.SetComponentData(entity, new Sprite2D()
            {
                image = ctx.GetEntity(texture),
                imageRegion = new Rect(
                    rect.x / texture.width,
                    rect.y / texture.height,
                    rect.width / texture.width,
                    rect.height / texture.height),
                pivot = new float2(
                    (pivot.x - offset.x) / rect.width,
                    (pivot.y - offset.y) / rect.height),
                pixelsToWorldUnits = 1f / sprite.pixelsPerUnit
            });

            if (border != UnityEngine.Vector4.zero)
            {
                ctx.AddComponentData(entity, new Sprite2DBorder()
                {
                    bottomLeft = new float2(
                        border.x / rect.width,
                        border.y / rect.height),
                    topRight = new float2(
                        (rect.width - border.z) / rect.width,
                        (rect.height - border.w) / rect.height)
                });
            }
            return entity;
        }
    }
}
