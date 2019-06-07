using System;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Tiny.Rendering;
using Unity.Tiny.Text;

/**
 * @module
 * @name Unity.Tiny
 */
namespace Unity.Tiny.Rendering
{
    public struct Image2DGLES2 : ISystemStateComponentData
    {
        public uint glTexId;
        public bool externalOwner;
        public bool smoothingEnabled;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(DisplayListSystem))]
    public class RendererGLES2System : RendererComponentSystem
    {
        [DllImport("lib_unity_tiny_renderergles2", EntryPoint = "init_rendereres2")]
        [return: MarshalAs(UnmanagedType.I1)]
        static private extern bool InitNative(IntPtr emr);

        [DllImport("lib_unity_tiny_renderergles2", EntryPoint = "deinit_rendereres2")]
        static private extern void DeInitNative(IntPtr emr);

        [DllImport("lib_unity_tiny_renderergles2", EntryPoint = "beginscene_rendereres2")]
        static private extern void BeginSceneNative(IntPtr emr, float w, float h);

        [DllImport("lib_unity_tiny_renderergles2", EntryPoint = "endscene_rendereres2")]
        static private extern void EndSceneNative(IntPtr emr);

        [DllImport("lib_unity_tiny_renderergles2", EntryPoint = "begincamera_rendereres2")]
        static private extern void BeginCameraNative(IntPtr emr, Entity ecam, float w, float h);

        [DllImport("lib_unity_tiny_renderergles2", EntryPoint = "endcamera_rendereres2")]
        static private extern void EndCameraNative(IntPtr emr, Entity ecam);

        [DllImport("lib_unity_tiny_renderergles2", EntryPoint = "drawbatch_rendereres2")]
        static private extern unsafe void RenderSpriteBatchNative(IntPtr emr, int n, DisplayListEntry *batch);

        [DllImport("lib_unity_tiny_renderergles2", EntryPoint = "freeimage_rendereres2")]
        static private extern void FreeImageNative(ref Image2DGLES2 im);

        protected override void BeginScene(float2 targetSize)
        {
            BeginSceneNative(wrapper, targetSize.x, targetSize.y);
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

        protected override void OnCreate()
        {
            base.OnCreate();
            wrapper = CPlusPlus.WrapEntityManager(EntityManager);
        }

        protected override void OnDestroy()
        {
            DeInitNative(wrapper);
            CPlusPlus.ReleaseHandleForEntityManager(wrapper);
            wrapper = IntPtr.Zero;
            base.OnDestroy();
        }

        protected void AddImage2DES2GLComponent()
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            // no image2d es2 yet: add it!
            Entities.WithAny<Image2D, Text2DRenderer>().WithNone<Image2DGLES2>().ForEach((Entity e) =>
            {
                ecb.AddComponent(e, new Image2DGLES2());
            });
            // image2d removed or entity deleted: free native texture
            Entities.WithNone<Image2D, Text2DRenderer>().ForEach((Entity e, ref Image2DGLES2 imes2) =>
            {
                FreeImageNative(ref imes2);
                ecb.RemoveComponent<Image2DGLES2>(e);
            });

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        protected override void OnUpdate()
        {
            var env = World.TinyEnvironment();
            var config = env.GetConfigData<DisplayInfo>();
            if (config.renderMode == RenderMode.WebGL || config.renderMode == RenderMode.Auto)
            {
                if (InitNative(wrapper))
                {
                    AddImage2DES2GLComponent();
                    base.OnUpdate();
                }
            }
        }
    }

}
