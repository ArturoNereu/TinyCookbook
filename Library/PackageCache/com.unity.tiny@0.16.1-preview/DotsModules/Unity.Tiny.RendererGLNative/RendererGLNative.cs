using System;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using Unity.Tiny.Core2D;
using System.Runtime.InteropServices;
using Unity.Tiny.GLFW;
using Unity.Tiny.Debugging;
using Unity.Tiny.STB;

/**
 * @module
 * @name Unity.Tiny
 */
namespace Unity.Tiny.Rendering
{
    internal struct TextureGL : ISystemStateComponentData
    {
        public uint glTexId;
        public bool externalOwner;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(DisplayListSystem))]
    public class RendererGLNativeSystem : RendererComponentSystem
    {
        [DllImport("lib_unity_tiny_rendererglnative", EntryPoint = "init_rendererglfw")]
        static private extern void InitNative(IntPtr emr);

        [DllImport("lib_unity_tiny_rendererglnative", EntryPoint = "deinit_rendererglfw")]
        static private extern void DeInitNative(IntPtr emr);

        [DllImport("lib_unity_tiny_rendererglnative", EntryPoint = "beginscene_rendererglfw")]
        static private extern void BeginSceneNative(IntPtr emr, float w, float h);

        [DllImport("lib_unity_tiny_rendererglnative", EntryPoint = "endscene_rendererglfw")]
        static private extern void EndSceneNative(IntPtr emr);

        [DllImport("lib_unity_tiny_rendererglnative", EntryPoint = "begincamera_rendererglfw")]
        static private extern void BeginCameraNative(IntPtr emr, Entity ecam, float w, float h);

        [DllImport("lib_unity_tiny_rendererglnative", EntryPoint = "endcamera_rendererglfw")]
        static private extern void EndCameraNative(IntPtr emr, Entity ecam);

        [DllImport("lib_unity_tiny_rendererglnative", EntryPoint = "drawbatch_rendererglfw")]
        static private extern unsafe void RenderSpriteBatchNative(IntPtr emr, int n, DisplayListEntry *batch);

        [DllImport("lib_unity_tiny_rendererglnative", EntryPoint = "setPresentBorder_rendererglfw")]
        static private extern void SetPresentBorder(int dx, int dy, int dw, int dh);

        [DllImport("lib_unity_tiny_rendererglnative", EntryPoint = "isBadIntelDriver_rendererglfw")]
        [return: MarshalAs(UnmanagedType.I1)]
        static private extern bool IsBadIntelDriver();

        [DllImport("lib_unity_tiny_rendererglnative", EntryPoint = "uploadNewTexture_rendererglfw")]
        static private extern unsafe uint UploadNewTexture(int w, int h, byte *pixels, int disableSmoothing);

        protected override void BeginScene(float2 targetSize)
        {
            if (workaroundIntelDriver)
            {
                int left=0, top=0, right=0, bottom=0;
                GLFWNativeCalls.getWindowFrameSize(ref left, ref top, ref right, ref bottom);
                if (left >= 1) left--;
                if (right >= 1) right--;
                if (bottom >= 1) bottom--;
                if (top >= 1) top--;
                if (top >= 1) top--;
                SetPresentBorder(left,bottom,-left-right, -bottom-top);
            }
            BeginSceneNative(wrapper, targetSize.x, targetSize.y);

            // upload all texture that need uploading - we do not track changes to images here. need a different mechanic for that. 
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities.WithNone<TextureGL>().ForEach((Entity e, ref Image2D im2d, ref Image2DSTB imstb) => {
                if (im2d.status != ImageStatus.Loaded)
                    return;
                uint texId;
                unsafe {
                    int hasAlpha = 0;
                    int w = 0;
                    int h = 0;
                    byte *pixels = ImageIOSTBNativeCalls.GetImageFromHandle(imstb.imageHandle, ref hasAlpha, ref w, ref h);
                    texId = UploadNewTexture(w, h, pixels, im2d.disableSmoothing?1:0);
                    Debug.LogFormat("Uploaded texture {0},{1} from image handle {2}", w, h, imstb.imageHandle);
                    ImageIOSTBNativeCalls.FreeBackingMemory(imstb.imageHandle);
                }
                ecb.AddComponent(e,new TextureGL {
                    glTexId = texId,
                    externalOwner = false
                });
            });
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        protected override void EndScene()
        {
            EndSceneNative(wrapper);
        }

        protected override void BeginCamera(Entity eCam, float2 targetSize)
        {
            BeginCameraNative(wrapper, eCam, targetSize.x, targetSize.y);
        }

        protected override void EndCamera(Entity eCam) {
            EndCameraNative(wrapper, eCam);
        }

        protected override void BeginRTT(Entity eCam, ref Camera2DRenderToTexture rtt) {

        }

        protected override void EndRTT(Entity eCam, ref Camera2DRenderToTexture rtt) {

        }

        protected override unsafe void RenderSpriteBatch(int n, DisplayListEntry* batch)
        {
            RenderSpriteBatchNative(wrapper, n, batch);
        }

        protected IntPtr wrapper;

        private bool initialized = false;
        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            if (initialized)
                return;

            wrapper = CPlusPlus.WrapEntityManager(EntityManager);
            InitNative(wrapper);
            workaroundIntelDriver = IsBadIntelDriver();
            if (workaroundIntelDriver)
                Debug.Log("Intel driver present workaround enabled.");
            initialized = true;
        }

        protected override void OnDestroy()
        {
            if (!initialized)
                return;

            DeInitNative(wrapper);
            CPlusPlus.ReleaseHandleForEntityManager(wrapper);
            wrapper = IntPtr.Zero;
            base.OnDestroy();
        }

        public bool workaroundIntelDriver;
    }

}
