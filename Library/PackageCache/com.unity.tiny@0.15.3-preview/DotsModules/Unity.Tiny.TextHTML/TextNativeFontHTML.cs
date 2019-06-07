using System;
using System.Runtime.InteropServices;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Text;
using UnityEngine.Assertions;
using UnityEngine.Experimental.LowLevel;
using Unity.Tiny.Debugging;

[assembly: ModuleDescription("Unity.Tiny.TextHTML", "HTML text rendering")]
[assembly: IncludedPlatform(Platform.Web | Platform.WeChat | Platform.FBInstant)]
namespace Unity.Tiny.TextHTML
{
    [HideInInspector]
    internal struct Text2DPrivateCacheHTML : ISystemStateComponentData
    {
        public bool italic;
        public int weight;
        public float size;
        public Color color;
        public float minSizeAutoFit;
        public float maxSizeAutoFit;
        public float2 rect;
        public int cacheIndex;

        public bool Equals(Text2DPrivateCacheHTML t)
        {
            if (italic != t.italic)
                return false;
            if (weight != t.weight)
                return false;
            if (size != t.size)
                return false;
            if (minSizeAutoFit != t.minSizeAutoFit || maxSizeAutoFit != t.maxSizeAutoFit)
                return false;
            if (rect.x != t.rect.x || rect.y != t.rect.y)
                return false;
            if (color != t.color)
                return false;
            return true;
        }
    }

    //  This system does some pre-process work to compute text measurements before
    //  rendering the text using native fonts. You must schedule it in order to render text in Canvas or WebGL.
    //
    //  If you are using a Text2DAutofit component, this system also computes the
    //  text size required to auto-fit the text.
    //
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(Unity.Tiny.UILayout.SetRectTransformSizeSystem))]
    internal class TextNativeFontHtmlSystem : ComponentSystem
    {
        private EntityQuery groupAddNativeFontPrivC;
        private EntityQuery groupRemoveNativeFontPrivateC;

        [DllImport("__Internal")]
        static extern void js_measureText(string text, string family, float size, float weight, bool italic, out float outWidth, out float outHeight);

        protected override void OnCreate()
        {
            base.OnCreate();
            groupAddNativeFontPrivC = EntityManager.CreateEntityQuery(ComponentType.ReadOnly(typeof(Text2DRenderer)),
                ComponentType.ReadOnly(typeof(Text2DStyleNativeFont)),
                ComponentType.Exclude(typeof(Text2DPrivateNative)),
                ComponentType.Exclude(typeof(Text2DPrivateCacheHTML)));

            groupRemoveNativeFontPrivateC = EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly(typeof(Text2DPrivateNative)),
                ComponentType.ReadOnly(typeof(Text2DPrivateCacheHTML)),
                ComponentType.Exclude(typeof(Text2DRenderer)));
        }

        protected override void OnDestroy()
        {
            groupAddNativeFontPrivC.Dispose();
            groupRemoveNativeFontPrivateC.Dispose();
        }

        protected override void OnUpdate()
        {
            EntityManager em = EntityManager;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            // Check Native font components
            Entities.With(groupAddNativeFontPrivC).ForEach((Entity e, ref Text2DRenderer tr, ref Text2DStyleNativeFont nf) =>
            {
                if (CheckNativeFontComponents(em, ref tr, ref nf))
                {
                    ecb.AddComponent(e, new Text2DPrivateNative());
                    ecb.AddComponent(e, new Text2DPrivateCacheHTML());
                    ecb.AddBuffer<TextPrivateString>(e);
                    ecb.AddBuffer<TextPrivateFontName>(e);
                }
            });
            ecb.Playback(em);
            ecb.Dispose();

            Entities.ForEach((Entity e, ref Text2DRenderer tr, ref Text2DPrivateNative textP,
                ref Text2DPrivateCacheHTML textHtmlP, ref Text2DStyleNativeFont nfStyle) =>
            {
                Text2DStyle textStyle = em.GetComponentData<Text2DStyle>(tr.style);
                NativeFont nf = em.GetComponentData<NativeFont>(nfStyle.font);

                Text2DPrivateCacheHTML temp = new Text2DPrivateCacheHTML();
                temp.size = textStyle.size;
                temp.color = textStyle.color;
                temp.weight = nfStyle.weight;
                temp.italic = nfStyle.italic;
                temp.cacheIndex = textHtmlP.cacheIndex;
                if (em.HasComponent<Text2DAutoFit>(e)) {
                    Text2DAutoFit autoFit = em.GetComponentData<Text2DAutoFit>(e);
                    temp.minSizeAutoFit = autoFit.minSize;
                    temp.maxSizeAutoFit = autoFit.maxSize;
                }

                if (em.HasComponent<RectTransformFinalSize>(e)) {
                    temp.rect = em.GetComponentData<RectTransformFinalSize>(e).size;
                }

                string newText = em.GetBufferAsString<TextString>(e);
                string privText = em.GetBufferAsString<TextPrivateString>(e);
                string strFontName = em.GetBufferAsString<TextPrivateFontName>(e);

                if (!textHtmlP.Equals(temp) || newText != privText || TextService.GetFontFamilyName(nf.name) != strFontName) {
                    // We will need to recreate the text texture in the renderer anyway
                    temp.cacheIndex = -1;
                    // Re-compute text size
                    float newSize = MeasureNativeFontText(em, e, ref textP, newText, TextService.GetFontFamilyName(nf.name), temp.rect);
                    textP.size = newSize;

                    em.SetBufferFromString<TextPrivateFontName>(e, TextService.GetFontFamilyName(nf.name));

                    //Update private text component
                    em.SetBufferFromString<TextPrivateString>(e, newText);
                }
                textHtmlP = temp;
            });

            ecb = new EntityCommandBuffer(Allocator.Temp);
            // De-init private components
            Entities.With(groupRemoveNativeFontPrivateC).ForEach((Entity e, ref Text2DPrivateNative tr, ref Text2DPrivateCacheHTML nc) =>
            {
                ecb.RemoveComponent<Text2DPrivateNative>(e);
                ecb.RemoveComponent<Text2DPrivateCacheHTML>(e);
                ecb.RemoveComponent<TextPrivateString>(e);
                ecb.RemoveComponent<TextPrivateFontName>(e);
            });
            ecb.Playback(em);
            ecb.Dispose();
        }

        public bool CheckNativeFontComponents(EntityManager em, ref Text2DRenderer tr, ref Text2DStyleNativeFont nf)
        {
            if (!em.Exists(tr.style)) {
                Debug.Log("Entity Text2DRenderer.style is not set ");
                return false;
            }
            if (!em.Exists(nf.font)) {
                Debug.Log("Entity Text2DStyleNativeFont.font is not set ");
                return false;
            }
            if (!em.HasComponent<Text2DStyle>(tr.style)) {
                Debug.LogFormat("Missing a Text2DStyle component on entity: {0}",
                                  tr.style.ToString());
                return false;
            }
            if (!em.HasComponent<NativeFont>(nf.font)) {
                Debug.LogFormat("Missing a NativeFont component on entity: {0}", nf.font.ToString());
                return false;
            }
            return true;
        }

        private float MeasureNativeFontText(EntityManager em, Entity e, ref Text2DPrivateNative privNative, string text, string family, float2 rectTransformSize)
        {
            Text2DStyle style = em.GetComponentData<Text2DStyle>(e);
            Text2DRenderer tr = em.GetComponentData<Text2DRenderer>(e);
            Text2DStyleNativeFont styleNative = em.GetComponentData<Text2DStyleNativeFont>(e);
            NativeFont nf = em.GetComponentData<NativeFont>(styleNative.font);

            float newSize = style.size * nf.worldUnitsToPt;
            float outWidth = 0.0f, outHeight = 0.0f;
            js_measureText(text, family, newSize, styleNative.weight, styleNative.italic, out outWidth, out outHeight);

            if (em.HasComponent<Text2DAutoFit>(e))
            {
                Text2DAutoFit autoFit = em.GetComponentData<Text2DAutoFit>(e);
                float epsilon = 0.001f;
                // Re-measure if the text is too long/short or too tall
                if (math.abs(outWidth - rectTransformSize.x) >= epsilon || outHeight > rectTransformSize.y)
                {
                    float ratio = 1.0f;
                    if (outWidth > 0.0f)
                        ratio = rectTransformSize.x / outWidth;

                    newSize = (newSize * ratio <  autoFit.maxSize) ? newSize * ratio : autoFit.maxSize;

                    js_measureText(text, family, newSize, styleNative.weight, styleNative.italic, out outWidth,out outHeight);

                    if (outHeight > rectTransformSize.y) {
                        if (outHeight > 0.0f)
                            ratio = rectTransformSize.y / outHeight;
                        newSize = (newSize * ratio <  autoFit.maxSize) ? newSize * ratio : autoFit.maxSize;
                        js_measureText(text, family, newSize, styleNative.weight, styleNative.italic, out outWidth, out outHeight);
                    }
                }
                float minSize = autoFit.minSize;
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
            string line = "Text with native font, measured with: " + ((int) outWidth).ToString();
            line += " measured height: ";
            line += ((int) outHeight).ToString();
            Debug.Log(line);

            //Adjust text bounds with pivot
            privNative.bounds.x = -(rectTransformSize.x / 2) + (rectTransformSize.x * (tr.pivot.x));
            privNative.bounds.y = -(rectTransformSize.y / 2) + (rectTransformSize.y * (tr.pivot.y));
            privNative.bounds.width = outWidth;
            privNative.bounds.height = outHeight;

            return newSize;
        }
    }
}
