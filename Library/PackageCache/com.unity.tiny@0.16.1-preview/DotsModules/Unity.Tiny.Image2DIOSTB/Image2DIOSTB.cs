using System;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;
using Unity.Collections;
using System.Runtime.InteropServices;

/**
 * @module
 * @name Unity.Tiny
 */
namespace Unity.Tiny.STB
{
    public struct Image2DSTB : ISystemStateComponentData
    {
        public int imageHandle;
    }

    public struct Image2DSTBLoading : ISystemStateComponentData
    {
        public long internalId;
    }

    public static class ImageIOSTBNativeCalls
    {
        [DllImport("lib_unity_tiny_image2diostb", EntryPoint = "startload_stb", CharSet = CharSet.Ansi)]
        public static extern long StartLoad([MarshalAs(UnmanagedType.LPStr)]string imageFile, [MarshalAs(UnmanagedType.LPStr)]string maskFile); // returns loadId

        [DllImport("lib_unity_tiny_image2diostb", EntryPoint = "freeimage_stb")]
        public static extern void FreeNative(int imageHandle);

        [DllImport("lib_unity_tiny_image2diostb", EntryPoint = "abortload_stb")]
        public static extern void AbortLoad(long loadId);

        [DllImport("lib_unity_tiny_image2diostb", EntryPoint = "checkload_stb")]
        public static extern int CheckLoading(long loadId, ref int imageHandle); // 0=still working, 1=ok, 2=fail, imageHandle set when ok

        [DllImport("lib_unity_tiny_image2diostb", EntryPoint = "finishload_stb")]
        public static extern void FinishLoading();

        [DllImport("lib_unity_tiny_image2diostb", EntryPoint = "initmask_stb")]
        public static extern unsafe void InitImage2DMask(int imageHandle, byte* buffer);

        [DllImport("lib_unity_tiny_image2diostb", EntryPoint = "getimage_stb")] 
        public static extern unsafe byte *GetImageFromHandle(int imageHandle, ref int hasAlpha, ref int sizeX, ref int sizeY);

        [DllImport("lib_unity_tiny_image2diostb", EntryPoint = "freeimagemem_stb")] 
        public static extern void FreeBackingMemory(int imageHandle);
    }

    class ImageIOSTBSystemLoadFromFile : IGenericAssetLoader<Image2D, Image2DSTB, Image2DLoadFromFile, Image2DSTBLoading>
    {
        public void StartLoad(EntityManager man, Entity e, ref Image2D image, ref Image2DSTB imgSTB, ref Image2DLoadFromFile fspec, ref Image2DSTBLoading loading)
        {
            // if there are async still loading, but set to new file stop job
            if (loading.internalId!=0) {
                ImageIOSTBNativeCalls.AbortLoad (loading.internalId);
            }

            image.status =  ImageStatus.Loading;
            image.hasAlpha = false;
            image.imagePixelSize.x = 0;
            image.imagePixelSize.y = 0;

            var fnImage = man.GetBufferAsString<Image2DLoadFromFileImageFile>(e);
            if (man.HasComponent<Image2DLoadFromFileImageFile>(e) && fnImage.Length <= 0)
                Debug.LogFormat("The file one entity {1} contains an empty Image2DLoadFromFileImageFile string.", e);
            var fnMask = man.GetBufferAsString<Image2DLoadFromFileMaskFile>(e);
            if (man.HasComponent<Image2DLoadFromFileMaskFile>(e) && fnMask.Length <= 0)
                Debug.LogFormat("The file one entity {1} contains an empty Image2DLoadFromFileMaskFile string.", e);
           
            loading.internalId = ImageIOSTBNativeCalls.StartLoad(fnImage, fnMask);
        }

        public LoadResult CheckLoading(IntPtr cppwrapper, EntityManager man, Entity e, ref Image2D image, ref Image2DSTB imgSTB, ref Image2DLoadFromFile unused, ref Image2DSTBLoading loading)
        {
            int newHandle = 0;
            int r = ImageIOSTBNativeCalls.CheckLoading(loading.internalId, ref newHandle);
            if (r==0)
                return LoadResult.stillWorking;
            FreeNative(man, e, ref imgSTB);
            Assert.IsTrue(newHandle > 0);
            imgSTB.imageHandle = newHandle;

            var fnLog = string.Empty;
            fnLog += man.GetBufferAsString<Image2DLoadFromFileImageFile>(e);
            if (man.HasComponent<Image2DLoadFromFileMaskFile>(e)) {
                fnLog += " alpha=";
                fnLog += man.GetBufferAsString<Image2DLoadFromFileMaskFile>(e);
            }

            if (r == 2) {
                image.status = ImageStatus.LoadError;
                image.imagePixelSize.xy = 0;
                Debug.LogFormat("Failed to load {0}",fnLog);
                return LoadResult.failed;
            }

            int hasAlpha = 0;
            int w = 0, h = 0;
            unsafe
            {
                ImageIOSTBNativeCalls.GetImageFromHandle(imgSTB.imageHandle, ref hasAlpha, ref w, ref h);
                Assert.IsTrue(w > 0 && h > 0);
                image.hasAlpha = hasAlpha!=0;
                image.imagePixelSize.x = (float)w;
                image.imagePixelSize.y = (float)h;
            }
            Debug.LogFormatAlways("Loaded image: {0} Handle {4} Size: {1},{2} Alpha: {3}",fnLog, w, h, image.hasAlpha?"yes":"no", imgSTB.imageHandle);

            // We finished loading the image and retrieve its pixel size, lets init the mask data also 
            if (man.HasComponent<Image2DAlphaMask>(e)) {
                DynamicBuffer<Image2DAlphaMaskData> maskData = man.GetBuffer<Image2DAlphaMaskData>(e);
                maskData.ResizeUninitialized(w * h);
                unsafe
                {
                    ImageIOSTBNativeCalls.InitImage2DMask(imgSTB.imageHandle, (byte*)(maskData.GetUnsafePtr()));
                    Debug.LogAlways("  Created alpha mask for image.");
                }
            }
            image.status = ImageStatus.Loaded;
            return LoadResult.success;
        }

        public void FreeNative(EntityManager man, Entity e, ref Image2DSTB imgSTB)
        {
            ImageIOSTBNativeCalls.FreeNative(imgSTB.imageHandle);
        }

        public void FinishLoading(EntityManager man, Entity e, ref Image2D img, ref Image2DSTB imgSTB, ref Image2DSTBLoading loading)
        {
            ImageIOSTBNativeCalls.FinishLoading();
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(Image2DMaskInit))]
    public class Image2DIOSTBSystem : GenericAssetLoader< Image2D, Image2DSTB, Image2DLoadFromFile, Image2DSTBLoading >
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            c = new ImageIOSTBSystemLoadFromFile();
        }

        protected override void OnUpdate()
        {
            // loading
            base.OnUpdate();
        }
    }

}
