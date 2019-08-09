using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.Text
{
    public static class TextService
    {
        /// <summary>
        /// Creates a NativeFont entity.
        /// </summary>
        public static Entity CreateNativeFont(EntityManager mgr, FontName fontName, string ttfFileName, float worldUnitsToPt)
        {
            Entity e = mgr.CreateEntity(typeof(NativeFont), typeof(NativeFontLoadFromFile));
            mgr.SetComponentData(e, new NativeFont() {name = fontName, worldUnitsToPt = worldUnitsToPt});
            mgr.AddBufferFromString<NativeFontLoadFromFileName>(e, ttfFileName);
            return e;
        }

        /// <summary>
        /// Creates text to render with a NativeFont.
        /// </summary>
        public static Entity CreateTextWithNativeFont(EntityManager mgr, ref Entity fontEntity, string text, float size,
            bool italic, int weight, Color color, BlendOp blending, float2 pivot)
        {
            Entity e = mgr.CreateEntity(typeof(Text2DStyle), typeof(Text2DRenderer), typeof(Text2DStyleNativeFont));
            mgr.SetComponentData(e, new Text2DStyle() { color = color, size = size});
            mgr.SetComponentData(e, new Text2DRenderer() { blending = blending, pivot = pivot, style = e });
            mgr.SetComponentData(e, new Text2DStyleNativeFont() {font = fontEntity, italic = italic, weight = weight});
            mgr.AddBufferFromString<TextString>(e, text);
            return e;
        }

       /// <summary>
       /// Creates a BitmapFont entity.
       /// </summary>
        public static Entity CreateBitmapFont(EntityManager mgr, ref Entity textureAtlasImg, float size, float ascent, float descent)
        {
            Entity e = mgr.CreateEntity();
            mgr.AddComponentData(e, new BitmapFont(){textureAtlas = textureAtlasImg, size = size, ascent = ascent, descent = descent});
            mgr.AddBuffer<CharacterInfoBuffer>(e);
            return e;
        }

        /// <summary>
        /// Adds a glyph to a BitmapFont
        /// </summary>
        public static void AddGlyph(EntityManager mgr, ref Entity fontEntity, uint character, float bearingX, float bearingY,
            float advance, float width, float height, Rect characterRegion)
        {
            if (!mgr.Exists(fontEntity))
                throw new ArgumentException("BitmapFont entity referenced in AddGlyph does not exist.");

            DynamicBuffer<CharacterInfoBuffer> buffer = mgr.GetBuffer<CharacterInfoBuffer>(fontEntity);
            buffer.Add(new CharacterInfoBuffer()
            {
                data = new CharacterInfo()
                {
                    value = character,
                    bearingX = bearingX,
                    bearingY = bearingY,
                    advance = advance,
                    width = width,
                    height = height,
                    characterRegion = characterRegion
                }
            });
        }

        /// <summary>
        /// Creates a text to render with a BitmapFont.
        /// </summary>
        public static Entity CreateTextWithBitmapFont(EntityManager mgr, ref Entity fontEntity, string text, float size,
            Color color, BlendOp blending, float2 pivot)
        {
            Entity e = mgr.CreateEntity(typeof(Text2DStyle), typeof(Text2DRenderer), typeof(Text2DStyleBitmapFont));
            mgr.SetComponentData(e, new Text2DRenderer() { blending = blending, pivot = pivot, style = e });
            mgr.SetComponentData(e, new Text2DStyle() { color = color, size = size});
            mgr.SetComponentData(e, new Text2DStyleBitmapFont() {font = fontEntity});
            mgr.AddBufferFromString<TextString>(e, text);
            return e;
        }

        /// <summary>
        /// Get CharacterInfo value of a UTF-32 code point. Will return a default value if not present.
        /// </summary>
        /// <returns>True if the character was found.</returns>
        public static bool GetCharacterInfo(DynamicBuffer<CharacterInfoBuffer> data, uint character, out CharacterInfo outCharacterInfo)
        {
            for (int i = 0; i < data.Length; ++i) {
                if (data[i].data.value == character) {
                    outCharacterInfo = data[i].data;
                    return true;
                }
            }
            outCharacterInfo = new CharacterInfo();
            return false;
        }

        /// <summary>
        /// Returns a string descriptor of a FontName.
        /// </summary>
        public static string GetFontFamilyName(FontName fontName)
        {
            switch (fontName)
            {
                case FontName.SansSerif:
                    return "Arial, Helvetica, sans-serif";
                case FontName.Serif:
                    return "\"Times New Roman\", Times, serif";
                case FontName.Monospace:
                    return "\"Courier New\", Courier, monospace";
                default:
                    break;
            }

            return "";
        }
    }
}
