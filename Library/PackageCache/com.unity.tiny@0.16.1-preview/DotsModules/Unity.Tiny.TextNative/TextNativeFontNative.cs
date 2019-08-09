using System;
using Unity.Authoring.Core;
using Unity.Tiny.Text;
using Unity.Entities;
using Unity.Tiny.Core2D;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;

namespace Unity.Tiny.TextNative
{
    internal struct Text2DPrivateCacheNative : ISystemStateComponentData
    {
        public float size;
        public Color color;
        public float minSizeAutoFit;
        public float maxSizeAutoFit;
        public float2 rect;
        public int glTexId;
        public bool dirty;

        public Text2DPrivateCacheNative(int id)
        {
            size = 0.0f;
            minSizeAutoFit = 0.0f;
            maxSizeAutoFit = 0.0f;
            rect = new float2(0.0f, 0.0f);
            color = new Color();
            glTexId = id;
            dirty = true;
        }

        public bool Equals(Text2DPrivateCacheNative t)
        {
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

    public static class FontSTBNativeCalls
    {
        [DllImport("lib_unity_tiny_textnative", EntryPoint = "startload_font_stb", CharSet = CharSet.Ansi)]
        public static extern long StartLoad([MarshalAs(UnmanagedType.LPStr)]string fontFile); // returns loadId
        [DllImport("lib_unity_tiny_textnative", EntryPoint = "checkload_font_stb")]
        public static extern int CheckLoading(long loadId, ref int fontHandle); // 0=still working, 1=ok, 2=fail, fontHandle set when ok
        [DllImport("lib_unity_tiny_textnative", EntryPoint = "measureText_stb")]
        public static extern void MeasureText([MarshalAs(UnmanagedType.LPStr)]string text, int fontHandle, int textLength, float size, ref float width, ref float height);
        [DllImport("lib_unity_tiny_textnative", EntryPoint = "freeFont_stb")]
        public static extern void FreeFont(int fontHandle);
        [DllImport("lib_unity_tiny_textnative", EntryPoint = "abortload_stb")]
        public static extern void AbortLoad(long loadId);
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(Unity.Tiny.UILayout.SetRectTransformSizeSystem))]
    public class TextNativeFontNativeSystem : ComponentSystem
    {
        public bool CheckNativeFontComponents(EntityManager em, ref Text2DRenderer tr, ref Text2DStyleNativeFont nf)
        {
            if (!em.Exists(tr.style))
            {
                Debug.Log("Entity Text2DRenderer.style is not set ");
                return false;
            }
            if (!em.Exists(nf.font))
            {
                Debug.Log("Entity Text2DStyleNativeFont.font is not set ");
                return false;
            }
            if (!em.HasComponent<Text2DStyle>(tr.style))
            {
                Debug.LogFormat("Missing a Text2DStyle component on entity: {0}", tr.style.ToString());
                return false;
            }
            if (!em.HasComponent<NativeFont>(nf.font))
            {
                Debug.LogFormat("Missing a NativeFont component on entity: {0}", nf.font.ToString());
                return false;
            }
            if (!em.HasComponent<NativeFontLoadFromFileName>(nf.font))
            {
                Debug.LogFormat("Missing NativeFontLoadFromFileName component on entity: {0}", nf.font.ToString());
                return false;
            }
            return true;
        }

        private float MeasureNativeFontText(EntityManager em, Entity e, ref Text2DPrivateNative privNative, string text, float2 rectTransformSize)
        {
            Text2DStyle style = em.GetComponentData<Text2DStyle>(e);
            Text2DRenderer tr = em.GetComponentData<Text2DRenderer>(e);
            Text2DStyleNativeFont styleNative = em.GetComponentData<Text2DStyleNativeFont>(e);
            NativeFont nf = em.GetComponentData<NativeFont>(styleNative.font);
            NativeFontPrivate nfP = em.GetComponentData<NativeFontPrivate>(styleNative.font);

            int newSize = (int)(style.size * nf.worldUnitsToPt);
            float outWidth = 0.0f, outHeight = 0.0f;
            FontSTBNativeCalls.MeasureText(text, nfP.fontHandle, text.Length, newSize, ref outWidth, ref outHeight);

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

                    newSize = (int)((newSize * ratio < autoFit.maxSize) ? newSize * ratio : autoFit.maxSize);

                    FontSTBNativeCalls.MeasureText(text, nfP.fontHandle, text.Length, newSize, ref outWidth, ref outHeight);

                    if (outHeight > rectTransformSize.y)
                    {
                        if (outHeight > 0.0f)
                            ratio = rectTransformSize.y / outHeight;
                        newSize = (int)((newSize * ratio < autoFit.maxSize) ? newSize * ratio : autoFit.maxSize);
                        FontSTBNativeCalls.MeasureText(text, nfP.fontHandle, text.Length, newSize, ref outWidth, ref outHeight);
                    }
                }

                float minSize = autoFit.minSize;
                if (newSize < minSize)
                {
                    Debug.LogFormat("Text: {0} is not renderable, the resized font {1} is smaller than the minimum allowed {2}", text, newSize, minSize);
                    return 0.0f;
                }
            }

            Debug.LogFormat("Text with native font: {0} , measured with {1} measure height {2}", text, outWidth, outHeight);

            //Adjust text bounds with pivot
            privNative.bounds.x = -(rectTransformSize.x / 2) + (rectTransformSize.x * (tr.pivot.x));
            privNative.bounds.y = -(rectTransformSize.y / 2) + (rectTransformSize.y * (tr.pivot.y));
            privNative.bounds.width = outWidth;
            privNative.bounds.height = outHeight;

            return newSize;
        }

        protected override void OnUpdate()
        {
            EntityManager em = EntityManager;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            // Check Native font components
            Entities.WithAll<Text2DRenderer, Text2DStyleNativeFont>().WithNone<Text2DPrivateNative, Text2DPrivateCacheNative>().ForEach((Entity e, ref Text2DRenderer tr, ref Text2DStyleNativeFont nf) =>
            {
                if (CheckNativeFontComponents(em, ref tr, ref nf))
                {
                    NativeFont nativeFont = em.GetComponentData<NativeFont>(nf.font);
                    if(nativeFont.status == FontStatus.Loaded)
                    {
                        ecb.AddComponent(e, new Text2DPrivateNative());
                        ecb.AddComponent(e, new Text2DPrivateCacheNative(-1));
                        ecb.AddBuffer<TextPrivateString>(e);
                        ecb.AddBuffer<TextPrivateFontName>(e);
                    }
                }
            });
            ecb.Playback(em);
            ecb.Dispose();

            //Text measurement
            Entities.ForEach((Entity e, ref Text2DRenderer tr, ref Text2DPrivateNative textP,
               ref Text2DPrivateCacheNative textCache, ref Text2DStyleNativeFont nfStyle) =>
            {
                Text2DStyle textStyle = em.GetComponentData<Text2DStyle>(tr.style);
                NativeFont nf = em.GetComponentData<NativeFont>(nfStyle.font);

                Text2DPrivateCacheNative temp = new Text2DPrivateCacheNative(-1);
                temp.size = textStyle.size;
                temp.color = textStyle.color;
                temp.glTexId = textCache.glTexId;
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

                string newText = em.GetBufferAsString<TextString>(e);
                string privText = em.GetBufferAsString<TextPrivateString>(e);
                string strFontName = em.GetBufferAsString<TextPrivateFontName>(e);

                if (!textCache.Equals(temp) || newText != privText || TextService.GetFontFamilyName(nf.name) != strFontName)
                {
                    // We will need to recreate the text texture in the renderer anyway
                    temp.dirty = true;
                    // Re-compute text size
                    float newSize = MeasureNativeFontText(em, e, ref textP, newText, temp.rect);
                    textP.size = newSize;

                    em.SetBufferFromString<TextPrivateFontName>(e, TextService.GetFontFamilyName(nf.name));
                }
                else
                    temp.dirty = false;

                em.SetBufferFromString<TextPrivateString>(e, newText);

                textCache = temp;
            });

            ecb = new EntityCommandBuffer(Allocator.Temp);
            // De-init private components
            Entities.WithAll<Text2DPrivateNative, Text2DPrivateCacheNative>().WithNone<Text2DRenderer>().ForEach((Entity e, ref Text2DPrivateNative tr, ref Text2DPrivateCacheNative nc) =>
            {
                ecb.RemoveComponent<Text2DPrivateNative>(e);
                ecb.RemoveComponent<Text2DPrivateCacheNative>(e);
                ecb.RemoveComponent<TextPrivateString>(e);
                ecb.RemoveComponent<TextPrivateFontName>(e);
            });
            ecb.Playback(em);
            ecb.Dispose();
        }
    }

    public class FontSTBAssetLoader : IGenericAssetLoader<NativeFont, NativeFontPrivate, NativeFontLoadFromFile, NativeFontLoading>
    {
        public void StartLoad(EntityManager man, Entity e, ref NativeFont font, ref NativeFontPrivate fontPrivate, ref NativeFontLoadFromFile fspec, ref NativeFontLoading loading)
        {
            if (loading.internalId != 0)
                FontSTBNativeCalls.AbortLoad(loading.internalId);

            font.status = FontStatus.Loading;

            string fontName = man.GetBufferAsString<NativeFontLoadFromFileName>(e);
            if (man.HasComponent<NativeFontLoadFromFileName>(e) && fontName.Length <= 0)
                Debug.LogFormat("The file one entity {1} contains an empty NativeFontLoadFromFileName string.", e);

            loading.internalId = FontSTBNativeCalls.StartLoad(fontName);
        }

        public LoadResult CheckLoading(IntPtr cppwrapper, EntityManager man, Entity e, ref NativeFont font, ref NativeFontPrivate fontPrivate, ref NativeFontLoadFromFile unused, ref NativeFontLoading loading)
        {
            int newHandle = -1;
            int r = FontSTBNativeCalls.CheckLoading(loading.internalId, ref newHandle);
            if (r == 0)
                return LoadResult.stillWorking;
            //Before loading make sure a previous font with newHandle has been cleaned
            FreeNative(man, e, ref fontPrivate);

            Assert.IsTrue(newHandle > 0);
            fontPrivate.fontHandle = newHandle;

            string fontName = man.GetBufferAsString<NativeFontLoadFromFileName>(e);
            if (man.HasComponent<NativeFontLoadFromFileName>(e) && fontName.Length <= 0)
                if (r == 2)
            {
                    font.status = FontStatus.LoadError;
                Debug.LogFormat("Failed to load {0}", fontName);
                return LoadResult.failed;
            }

            Debug.LogFormatAlways("Loaded font: {0} handle {1}", fontName, fontPrivate.fontHandle);

            font.status = FontStatus.Loaded;

            return LoadResult.success;
        }

        public void FreeNative(EntityManager man, Entity e, ref NativeFontPrivate fontPrivate)
        {
            FontSTBNativeCalls.FreeFont(fontPrivate.fontHandle);
        }

        public void FinishLoading(EntityManager man, Entity e, ref NativeFont font, ref NativeFontPrivate fontPrivate, ref NativeFontLoading loading)
        {
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TextNativeFontNativeSystem))]
    public class FontSTBLoadSystem : GenericAssetLoader<NativeFont, NativeFontPrivate, NativeFontLoadFromFile, NativeFontLoading>
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            c = new FontSTBAssetLoader();
        }

        protected override void OnUpdate()
        {
            // loading
            base.OnUpdate();
        }
    }
}

