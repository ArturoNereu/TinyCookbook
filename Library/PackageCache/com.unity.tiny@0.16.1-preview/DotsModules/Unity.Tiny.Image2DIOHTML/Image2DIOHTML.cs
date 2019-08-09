using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

/**
 * @module
 * @name Unity.Tiny
 */
namespace Unity.Tiny.HTML
{

    public struct Image2DHTML : ISystemStateComponentData
    {
        public int imageIndex;
        public bool externalOwner;
    };

    public struct Image2DHTMLLoading : ISystemStateComponentData
    {
        public int dummy;
    }

    static class ImageIOHTMLNativeCalls
    {
        // directly calls out to JS!

        [DllImport("lib_unity_tiny_image2diohtml", EntryPoint = "js_html_initImageLoading")]
        public static extern void JSInitImageLoading();

        [DllImport("lib_unity_tiny_image2diohtml", EntryPoint = "js_html_loadImage", CharSet = CharSet.Ansi)]
        public static extern int JSLoadImage([MarshalAs(UnmanagedType.LPStr)]string imageFile, [MarshalAs(UnmanagedType.LPStr)]string maskFile);

        [DllImport("lib_unity_tiny_image2diohtml", EntryPoint = "js_html_checkLoadImage")]
        public static extern int JSCheckLoadImage(int idx);

        [DllImport("lib_unity_tiny_image2diohtml", EntryPoint = "js_html_finishLoadImage")]
        public static extern void JSFinishLoadImage (int idx, ref int w, ref int h, ref int hasAlpha);

        [DllImport("lib_unity_tiny_image2diohtml", EntryPoint = "js_html_freeImage")]
        public static extern void JSFreeImage(int idx);

        [DllImport("lib_unity_tiny_image2diohtml", EntryPoint = "js_html_extractAlphaFromImage")]
        public static extern unsafe void ExtractAlphaFromImage(int idx, byte* destPtr, int w, int h);

        //void js_html_extractAlphaFromImage (int idx, char *destPtr, int w, int h);
        //char *js_html_imageToDataURI (int idx, int w, int h);
        //int js_html_imagePostRequestStatus (int idx);
        //int js_html_imagePostRequest (int idx, int w, int h, const char *uri);
        //int js_html_imageToMemory (int idx, int w, int h, char *dest);
        //int js_html_imageFromMemory (int idx, int w, int h, const char *src);
    }


    public class Image2DIOHTMLLoader : IGenericAssetLoader<Image2D, Image2DHTML, Image2DLoadFromFile, Image2DHTMLLoading>
    {
        public void StartLoad(EntityManager man, Entity e, ref Image2D image, ref Image2DHTML imgHTML,
            ref Image2DLoadFromFile fspec, ref Image2DHTMLLoading loading)
        {
            // if there was an image with that index, free it
            FreeNative(man, e, ref imgHTML);

            image.status = ImageStatus.Loading;
            image.hasAlpha = false;
            image.imagePixelSize.x = 0;
            image.imagePixelSize.y = 0;

            // C++
            var fnImage = man.GetBufferAsString<Image2DLoadFromFileImageFile>(e);
            var fnMask = man.GetBufferAsString<Image2DLoadFromFileMaskFile>(e);

            imgHTML.imageIndex = ImageIOHTMLNativeCalls.JSLoadImage(fnImage, fnMask);
        }

        public LoadResult CheckLoading(IntPtr wrapper, EntityManager man, Entity e, ref Image2D image, ref Image2DHTML imgHTML,
            ref Image2DLoadFromFile unused, ref Image2DHTMLLoading loading)
        {
            int r = ImageIOHTMLNativeCalls.JSCheckLoadImage(imgHTML.imageIndex);
            if (r == 0)
                return LoadResult.stillWorking;

            var fnLog = man.GetBufferAsString<Image2DLoadFromFileImageFile>(e);
            if (man.HasComponent<Image2DLoadFromFileMaskFile>(e))
            {
                fnLog += " alpha=";
                fnLog += man.GetBufferAsString<Image2DLoadFromFileMaskFile>(e);
            }

            if (r == 2)
            {
                image.status = ImageStatus.LoadError;
                image.imagePixelSize.xy = 0;
                FreeNative(man, e, ref imgHTML);
                Console.WriteLine("Failed to load "+fnLog);
                return LoadResult.failed;
            }

            int wi = 0;
            int hi = 0;
            int ai = 0;
            ImageIOHTMLNativeCalls.JSFinishLoadImage(imgHTML.imageIndex, ref wi, ref hi, ref ai);
            image.hasAlpha = ai != 0;
            image.imagePixelSize.x = (float)wi;
            image.imagePixelSize.y = (float)hi;

            image.status = ImageStatus.Loaded;
            imgHTML.externalOwner = false;
            var s = "Loaded image: ";
            s += fnLog;
            s += " size: ";
            s += wi.ToString();
            s += ", ";
            s += hi.ToString();
            if (ai != 0) s += " (has alpha channel)";
            s += " idx: ";
            s += imgHTML.imageIndex.ToString();
            Console.WriteLine(s);

            return LoadResult.success;
        }

        public void FreeNative(EntityManager man, Entity e, ref Image2DHTML imgHTML)
        {
            if (!imgHTML.externalOwner && imgHTML.imageIndex > 0)
                ImageIOHTMLNativeCalls.JSFreeImage(imgHTML.imageIndex);
            if (imgHTML.imageIndex > 0)
            {
                var s = "Free HTML image at ";
                s += imgHTML.imageIndex.ToString();
                Console.WriteLine(s);
            }
            imgHTML.imageIndex = 0;
            imgHTML.externalOwner = false;
        }

        public void FinishLoading(EntityManager man, Entity e, ref Image2D img, ref Image2DHTML imgHTML,
            ref Image2DHTMLLoading loading)
        {
            unsafe
            {
                if (man.HasComponent<Image2DAlphaMask>(e))
                {
                    DynamicBuffer<Image2DAlphaMaskData> mask = man.GetBuffer<Image2DAlphaMaskData>(e);
                    int width = (int)img.imagePixelSize.x;
                    int height = (int)img.imagePixelSize.y;
                    mask.ResizeUninitialized(width * height);
                    ImageIOHTMLNativeCalls.ExtractAlphaFromImage(imgHTML.imageIndex, (byte*)mask.GetUnsafePtr(), width,
                        height);
                }
            }
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(Image2DMaskInit))]
    public class Image2DIOHTMLSystem : GenericAssetLoader< Image2D, Image2DHTML, Image2DLoadFromFile, Image2DHTMLLoading >
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            c = new Image2DIOHTMLLoader();
            ImageIOHTMLNativeCalls.JSInitImageLoading();
        }

        protected override void OnUpdate()
        {
            // loading
            base.OnUpdate();
        }
    }


}
