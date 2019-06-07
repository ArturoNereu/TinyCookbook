using System;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Rendering;
using System.Runtime.InteropServices;
using Unity.Tiny.Text;
using Unity.Tiny.HTML;

/**
 * @module
 * @name UTiny
 */
namespace Unity.Tiny.Rendering
{
    public struct Image2DRenderToTextureHTMLEntry
    {
        public int imageIndex;
        public int w, h;
    }

    // private components for use in c++ part declared here
    public struct Image2DRenderToTextureHTML : ISystemStateComponentData
    {
        public Image2DRenderToTextureHTMLEntry rendering;
        public Image2DRenderToTextureHTMLEntry displaying;
    }

    public struct HTMLTintedSpriteDesc {
        public int imageIndex;
        public Rect texRect;
        public Color tintColor;
    }

    public struct Sprite2DRendererHTML : ISystemStateComponentData
    {
        public int tintedIndex;        // index to js
        public HTMLTintedSpriteDesc desc;  // current value
        public bool haspattern;
    }

    public struct TileHTML : ISystemStateComponentData
    {
        public int tintedIndex;        // index to js
        public HTMLTintedSpriteDesc desc;  // current value
    }

    public struct TintedGlyphHTML: IBufferElementData
    {
        public uint value; // character value
        public int tintedIndex;
        public HTMLTintedSpriteDesc desc;
    }

    // Component used only in c++ to fill TintedGlyphHTML buffer
    public struct TintedGlyphIndex : ISystemStateComponentData
    {
        public int index;
    }

    public struct TextBitmapHTML : IBufferElementData
    {
        public ushort c;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(DisplayListSystem))]
    public class RendererCanvasSystem : RendererComponentSystem
    {
        [DllImport("lib_unity_tiny_renderercanvas", EntryPoint = "init_renderercanvas")]
        [return: MarshalAs(UnmanagedType.I1)]
        static private extern bool InitNative(IntPtr emr);

        [DllImport("lib_unity_tiny_renderercanvas", EntryPoint = "deinit_renderercanvas")]
        static private extern void DeInitNative(IntPtr emr);

        [DllImport("lib_unity_tiny_renderercanvas", EntryPoint = "beginscene_renderercanvas")]
        static private extern void BeginSceneNative(IntPtr emr, float w, float h);

        [DllImport("lib_unity_tiny_renderercanvas", EntryPoint = "endscene_renderercanvas")]
        static private extern void EndSceneNative(IntPtr emr);

        [DllImport("lib_unity_tiny_renderercanvas", EntryPoint = "begincamera_renderercanvas")]
        static private extern void BeginCameraNative(IntPtr emr, Entity ecam, float w, float h);

        [DllImport("lib_unity_tiny_renderercanvas", EntryPoint = "endcamera_renderercanvas")]
        static private extern void EndCameraNative(IntPtr emr, Entity ecam);

        [DllImport("lib_unity_tiny_renderercanvas", EntryPoint = "drawbatch_renderercanvas")]
        static private extern unsafe void RenderSpriteBatchNative(IntPtr emr, int n, DisplayListEntry *batch);

        [DllImport("lib_unity_tiny_renderercanvas", EntryPoint = "text_releaseTintedGlyphs")]
        static private extern unsafe void ReleaseTintedGlyphs(IntPtr emr, Entity entity);

        private EntityQuery groupAddTextBitmap;
        private EntityQuery groupRemoveTextBitmap;
        private EntityQuery groupRemoveGlyphHtml;
        private EntityQuery groupInitializeBuffer;
        protected override void BeginScene(float2 targetSize)
        {
            Entities
                .With(groupRemoveGlyphHtml)
                .ForEach((Entity e, DynamicBuffer<TintedGlyphHTML> glyph) =>
                {
                    ReleaseTintedGlyphs(wrapper, e);
                    PostUpdateCommands.RemoveComponent<TintedGlyphHTML>(e);
                    PostUpdateCommands.RemoveComponent<TintedGlyphIndex>(e);
                });

            Entities
                .With(groupRemoveTextBitmap)
                .ForEach((Entity e, DynamicBuffer<TextBitmapHTML> glyph) =>
                {
                    PostUpdateCommands.RemoveComponent<TextBitmapHTML>(e);
                });

            Entities
                .With(groupAddTextBitmap)
                .ForEach((Entity e, ref Text2DStyleBitmapFont text) =>
                {
                    PostUpdateCommands.AddComponent(e, new TintedGlyphIndex(){index = 0});
                    DynamicBuffer<TintedGlyphHTML> tintedGlyphBuffer = PostUpdateCommands.AddBuffer<TintedGlyphHTML>(e);
                    DynamicBuffer<TextBitmapHTML> textBuffer = PostUpdateCommands.AddBuffer<TextBitmapHTML>(e);
                });

            //TODO: See if we need to optimize this
            Entities
                .With(groupInitializeBuffer)
                .ForEach((Entity e, ref TintedGlyphIndex index) =>
                {
                    if (EntityManager.HasComponent<GlyphPrivateBuffer>(e))
                    {
                        int length = EntityManager.GetBuffer<GlyphPrivateBuffer>(e).Length;
                        DynamicBuffer<TintedGlyphHTML> tintedGlyphBuffer = EntityManager.GetBuffer<TintedGlyphHTML>(e);
                        DynamicBuffer<TextBitmapHTML> textBuffer = EntityManager.GetBuffer<TextBitmapHTML>(e);
                        tintedGlyphBuffer.ResizeUninitialized(length);
                        textBuffer.ResizeUninitialized(length);
                    }
                });

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
            wrapper = CPlusPlus.WrapEntityManager(EntityManager);
            groupAddTextBitmap = EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly(typeof(Text2DStyleBitmapFont)),
                ComponentType.Exclude(typeof(TintedGlyphHTML)),
                ComponentType.Exclude(typeof(TextBitmapHTML)));

            groupRemoveTextBitmap = EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly(typeof(TextBitmapHTML)),
                ComponentType.Exclude(typeof(Text2DStyleBitmapFont)));

            groupRemoveGlyphHtml = EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly(typeof(TintedGlyphHTML)),
                ComponentType.Exclude(typeof(Text2DStyleBitmapFont)));

            groupInitializeBuffer = EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly(typeof(TintedGlyphIndex)));

            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            DeInitNative(wrapper);
            CPlusPlus.ReleaseHandleForEntityManager(wrapper);
            wrapper = IntPtr.Zero;
            groupAddTextBitmap.Dispose();
            groupRemoveTextBitmap.Dispose();
            groupRemoveGlyphHtml.Dispose();
            groupInitializeBuffer.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var env = World.TinyEnvironment();
            var config = env.GetConfigData<DisplayInfo>();
            if (config.renderMode == RenderMode.Canvas)
            {
                if (InitNative(wrapper))
                    base.OnUpdate();
            }
        }
    }



}
