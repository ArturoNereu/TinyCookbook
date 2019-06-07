using System;
using Unity.Authoring.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Collections;
using Unity.Tiny.Debugging;

namespace Unity.Tiny.Text
{ 
    [HideInInspector]
    internal struct Text2DPrivateCacheBitmap : ISystemStateComponentData
    {
        public float size;
        public Color color;
        public float minSizeAutoFit;
        public float maxSizeAutoFit;
        public float2 rect;

        public bool Equals(Text2DPrivateCacheBitmap t)
        {
            if (math.abs(size - t.size) > 0.001f)
                return false;
            if (math.abs(minSizeAutoFit - t.minSizeAutoFit) > 0.001f || math.abs(maxSizeAutoFit - t.maxSizeAutoFit) > 0.001f)
                return false;
            if (math.abs(rect.x - t.rect.x) > 0.001f || math.abs(rect.y - t.rect.y) > 0.001f)
                return false;
            if (color != t.color)
                return false;
            return true;
        }
    }

    //  This system does some pre-process work to compute text measurements before
    //  rendering the text using bitmap fonts. You must schedule it in order to render text in Canvas or WebGL.
    //
    //  If you are using a Text2DAutofit component, this system also computes the
    //  text size required to auto-fit the text.
    //
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(Unity.Tiny.UILayout.SetRectTransformSizeSystem))]
    internal class TextBitmapFontSystem : ComponentSystem
    {
        private EntityQuery groupAddBitmapFontPrivateC;
        private EntityQuery groupRemoveBitmapFontPrivateC;

        protected override void OnCreate()
        {
            groupAddBitmapFontPrivateC = EntityManager.CreateEntityQuery(ComponentType.ReadOnly(typeof(Text2DRenderer)),
                ComponentType.ReadOnly(typeof(Text2DStyleBitmapFont)),
                ComponentType.Exclude(typeof(Text2DPrivateBitmap)),
                ComponentType.Exclude(typeof(Text2DPrivateCacheBitmap)),
                ComponentType.Exclude(typeof(GlyphPrivateBuffer)),
                ComponentType.Exclude(typeof(TextPrivateString)));
            groupRemoveBitmapFontPrivateC = EntityManager.CreateEntityQuery(ComponentType.ReadOnly(typeof(Text2DPrivateBitmap)),
                ComponentType.ReadOnly(typeof(Text2DPrivateCacheBitmap)),
                ComponentType.Exclude(typeof(Text2DRenderer)));
        }

        protected override void OnDestroy()
        {
            groupAddBitmapFontPrivateC.Dispose();
            groupRemoveBitmapFontPrivateC.Dispose();
        }

        protected override void OnUpdate()
        {
            EntityManager em = EntityManager;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            //Check bitmap font components
            Entities.With(groupAddBitmapFontPrivateC).ForEach((Entity e, ref Text2DRenderer tr, ref Text2DStyleBitmapFont bf) =>
            {
                if (CheckBitmapFontComponents(em, ref tr, ref bf))
                {
                    BitmapFont bitmapFont = em.GetComponentData<BitmapFont>(bf.font);
                    Image2D image2D = em.GetComponentData<Image2D>(bitmapFont.textureAtlas);
                    if (image2D.status == ImageStatus.Loaded)
                    {
                        //Add private compo
                        ecb.AddComponent(e, new Text2DPrivateBitmap());
                        ecb.AddComponent(e, new Text2DPrivateCacheBitmap());
                        ecb.AddBuffer<GlyphPrivateBuffer>(e);
                        ecb.AddBuffer<TextPrivateString>(e);
                    }
                }
            });
            ecb.Playback(em);
            ecb.Dispose();

            Entities.ForEach((Entity e, ref Text2DRenderer tr, ref Text2DPrivateBitmap textP,
                ref Text2DPrivateCacheBitmap textHtmlP) =>
            {
                Text2DStyle textStyle = em.GetComponentData<Text2DStyle>(tr.style);
                Text2DStyleBitmapFont styleBitmap = em.GetComponentData<Text2DStyleBitmapFont>(e);
                BitmapFont bitmapFont = em.GetComponentData<BitmapFont>(styleBitmap.font);

                Text2DPrivateCacheBitmap temp = new Text2DPrivateCacheBitmap();
                temp.size = textStyle.size;
                temp.color = textStyle.color;

                if (em.HasComponent<Text2DAutoFit>(e))
                {
                    Text2DAutoFit autoFit = em.GetComponentData<Text2DAutoFit>(e);
                    temp.minSizeAutoFit = autoFit.minSize;
                    temp.maxSizeAutoFit = autoFit.maxSize;
                }

                if (em.HasComponent<RectTransformFinalSize>(e))
                {
                    temp.rect = em.GetComponentData<RectTransformFinalSize>(e).size;
                }

                var newText = em.GetBufferAsString<TextString>(e);
                var privText = em.GetBufferAsString<TextPrivateString>(e);

                if (!textHtmlP.Equals(temp) || newText != privText)
                {
                    // Re-compute text size
                    float newSize = MeasureBitmapFontText(em, ref e, ref textP, newText, temp.rect);
                    textP.size = newSize;
                }

                //Update private text component
                em.SetBufferFromString<TextPrivateString>(e, newText);

                textHtmlP = temp;
            });

            ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities.With(groupRemoveBitmapFontPrivateC).ForEach((Entity e, ref Text2DPrivateBitmap tr, ref Text2DPrivateCacheBitmap nc) =>
            {
                ecb.RemoveComponent<Text2DPrivateBitmap>(e);
                ecb.RemoveComponent<Text2DPrivateCacheBitmap>(e);
                ecb.RemoveComponent<GlyphPrivateBuffer>(e);
                ecb.RemoveComponent<TextPrivateString>(e);
            });
            ecb.Playback(em);
            ecb.Dispose();
        }

        private bool CheckBitmapFontComponents(EntityManager em, ref Text2DRenderer tr, ref Text2DStyleBitmapFont tb)
        {
            if (!em.Exists(tr.style))
            {
                Debug.Log("Entity Text2DRenderer.style is not set ");
                return false;
            }
            if (!em.Exists(tb.font))
            {
                Debug.Log("Entity Text2DStyleBitmapFont.font is not set ");
                return false;
            }
            if (!em.HasComponent<Text2DStyle>(tr.style))
            {
                Debug.LogFormat("Missing a Text2DStyle component on entity: {0}",
                                  tr.style.ToString());
                return false;
            }
            if (!em.HasComponent<BitmapFont>(tb.font))
            {
                Debug.LogFormat("Missing a BitmapFont component on entity: {0}", tb.font.ToString());
                return false;
            }
            BitmapFont bitmapFont = em.GetComponentData<BitmapFont>(tb.font);
            if (!em.HasComponent<Image2D>(bitmapFont.textureAtlas))
            {
                Debug.LogFormat("Missing a Image2D component for the texture atlas bitmap font on entity: {0}", bitmapFont.textureAtlas.ToString());
                return false;
            }
            return true;
        }

        private float MeasureBitmapFontText(EntityManager em, ref Entity e, ref Text2DPrivateBitmap privBitmap, string text, float2 rectTransformSize)
        {
            Text2DRenderer tr = em.GetComponentData<Text2DRenderer>(e);
            Text2DStyle style = em.GetComponentData<Text2DStyle>(tr.style);
            Text2DStyleBitmapFont bfStyle = em.GetComponentData<Text2DStyleBitmapFont>(e);
            BitmapFont bf = em.GetComponentData<BitmapFont>(bfStyle.font);

            float newSize = style.size;
            if (bf.size != 0.0f)
                newSize = style.size / bf.size;

            //Get buffer of supported glyphs
            DynamicBuffer<CharacterInfoBuffer> supportedGlyphs = em.GetBuffer<CharacterInfoBuffer>(bfStyle.font);
            DynamicBuffer<GlyphPrivateBuffer> glyphs = em.GetBuffer<GlyphPrivateBuffer>(e);
            glyphs.Clear();
            glyphs.ResizeUninitialized(text.Length);

            privBitmap.fontScale = new float2(newSize, newSize);
            float lineWidth = 0.0f;
            for (int i = 0; i < text.Length; ++i)
            {
                CharacterInfo info = new CharacterInfo();
                // TODO: text[i] should be in UTF16. Convert to UTF32?
                if (TextService.GetCharacterInfo(supportedGlyphs, text[i], out info))
                {
                    GlyphPrivate glyph = new GlyphPrivate();
                    glyph.ci = info;
                    // Glyph position is center of the glyph
                    float x = (lineWidth + info.bearingX + info.width / 2.0f);
                    float y = (info.height / 2.0f - (info.height - info.bearingY));
                    glyph.position = new float2(x, y);
                    glyphs[i] = new GlyphPrivateBuffer() { c = glyph };
                    lineWidth += info.advance;
                }
                else
                {
                    lineWidth += 10.0f;
                    string l = "Glyph: " + text[i];
                    l += " not supported by the font entity: ";
                    Debug.Log(l + e.ToString());
                }
            }

            float width = lineWidth;
            float height = bf.ascent + math.abs((float)bf.descent);

            //Resize if autofit added
            if (em.HasComponent<Text2DAutoFit>(e))
            {
                Text2DAutoFit autofit = em.GetComponentData<Text2DAutoFit>(e);

                float newWidth = width * newSize;
                float newHeight = height * newSize;

                float maxSize = autofit.maxSize / bf.size;
                float minSize = autofit.minSize / bf.size;

                if (math.abs(newWidth - rectTransformSize.x) >= 0.001f || newHeight > rectTransformSize.y)
                {
                    float ratio = 1.0f;
                    if (newWidth > 0.0f)
                    {
                        ratio = rectTransformSize.x / newWidth;
                    }

                    newSize = math.min(newSize * ratio, maxSize);
                    privBitmap.fontScale = new float2(newSize, newSize);

                    newHeight = height * newSize;

                    if (newHeight > rectTransformSize.y)
                    {
                        if (newHeight > 0.0f)
                            ratio = rectTransformSize.y / newHeight;
                        newSize = math.min(newSize * ratio, maxSize);
                        privBitmap.fontScale = new float2(newSize, newSize);
                    }
                }

                if (newSize < minSize)
                {
                    //TODO: Update when float.ToString will not throws an exception anymore
                    string l = "Text: " + text;
                    l += " is not renderable, the generated font size " + ((int)newSize).ToString();
                    l += " is smaller than the minimum allowed " + ((int)minSize).ToString();
                    Debug.Log(l);
                }
            }

            //TODO: Update when float.ToString will not throws an exception anymore
            string line = "Text with bitmap font, measured with: " + ((int)width).ToString();
            line += " measured height: ";
            line += ((int)height).ToString();
            Debug.Log(line);

            privBitmap.bounds.width = width;
            privBitmap.bounds.height = height;
            privBitmap.bounds.x = -(rectTransformSize.x / 2.0f) + (rectTransformSize.x * (tr.pivot.x));
            privBitmap.bounds.y = -(rectTransformSize.y / 2.0f) + (rectTransformSize.y * (tr.pivot.y));
            return newSize;
        }
    }
}
