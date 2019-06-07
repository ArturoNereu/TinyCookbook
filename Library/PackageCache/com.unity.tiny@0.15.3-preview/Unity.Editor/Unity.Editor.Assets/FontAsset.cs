using TMPro;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Tiny.Text;

namespace Unity.Editor.Assets
{
    [EntityWithComponentsBinding(typeof(BitmapFont))]
    internal class BitmapFontAsset: UnityObjectAsset<TMPro.TMP_FontAsset>
    {
        public override AssetInfo GetAssetInfo(IAssetEnumerator ctx, TMP_FontAsset font)
        {
            var fontAsset = new AssetInfo(font, font.name);
            var textureAsset = ctx.GetAssetInfo(font.atlasTexture);
            textureAsset.Parent = fontAsset;

            return fontAsset;
        }
    }

    internal class FontAssetImporter : UnityObjectAssetImporter<TMPro.TMP_FontAsset>
    {
        public override Entity Import(IAssetImporter ctx, TMP_FontAsset font)
        {
            var entity = ctx.CreateEntity(typeof(BitmapFont), typeof(CharacterInfoBuffer));

            ctx.SetComponentData(entity, new BitmapFont()
            {
                textureAtlas = ctx.GetEntity(font.atlasTexture),
                size = font.creationSettings.pointSize,
                ascent = font.faceInfo.ascentLine,
                descent = font.faceInfo.descentLine
            });

            var buffer = ctx.GetBuffer<CharacterInfoBuffer>(entity).Reinterpret<CharacterInfo>();
            foreach (var lookup in font.characterLookupTable)
            {
                var glyph = lookup.Value;
                var rect = glyph.glyph.glyphRect;
                var region = new Rect(
                    (float)rect.x / font.atlasTexture.width,
                    (float)rect.y / font.atlasTexture.height,
                    (float)rect.width / font.atlasTexture.width,
                    (float)rect.height / font.atlasTexture.height);

                buffer.Add(new CharacterInfo
                {
                    value = lookup.Key,
                    advance = glyph.glyph.metrics.horizontalAdvance,
                    bearingX = glyph.glyph.metrics.horizontalBearingX,
                    bearingY = glyph.glyph.metrics.horizontalBearingY,
                    width = glyph.glyph.metrics.width,
                    height = glyph.glyph.metrics.height,
                    characterRegion = region
                });
            }

            return entity;
        }
    }
}
