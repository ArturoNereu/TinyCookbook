using System;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.UILayout;

namespace Unity.Tiny.Utils
{
    public class DemoUtils
    {
        static public World world;
        static public EntityManager entityManager;
        static public TinyEnvironment tinyEnv;
        static public void Initialize(int width, int height, RenderMode renderMode = RenderMode.Auto, bool DisableSwitchRenderingSystem = false)
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Enabled;

            world = DefaultTinyWorldInitialization.InitializeWorld("main");
            tinyEnv = world.TinyEnvironment();

            SetDisplaySize(width, height, renderMode);

            DefaultTinyWorldInitialization.InitializeSystems(world);

            entityManager = world.EntityManager;
            var srs = World.Active.GetExistingSystem<SwitchRenderingSystem>();
            srs.Disable = DisableSwitchRenderingSystem;
        }

        static public void SetDisplaySize(int width, int height, RenderMode renderMode = RenderMode.Auto)
        {
            DisplayInfo di = tinyEnv.GetConfigData<DisplayInfo>();
            di.width = width;
            di.height = height;
            di.renderMode = renderMode;
            di.autoSizeToFrame=true;
            tinyEnv.SetConfigData(di);
        }

        static public Rect CreateRect(float x, float y, float width, float height)
        {
            return new Rect()
            {
                x = x,
                y = y,
                width = width,
                height = height
            };
        }

        static public Entity CreateCamera(int h, Color backgroundColor, float depth = 0, float rectX = 0, float rectY = 0, float rectW = 1, float rectH = 1)
        {
            var e = entityManager.CreateEntity();
            var cam = new Camera2D();
            cam.backgroundColor = backgroundColor;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.depth = depth;
            cam.halfVerticalSize = h * 0.5f;
            cam.rect = new Rect(rectX, rectY, rectW, rectH);
            cam.cullingMode = CameraCullingMode.NoCulling;
            entityManager.AddComponentData(e, cam);
            entityManager.AddComponent(e, typeof(Parent));
            var cas = new Camera2DAxisSort();
            cas.axis = new float3(0, 1, 0);
            entityManager.AddComponentData(e, cas); // sort negative y
            tinyEnv.SetEntityName(e, "default_camera");
            return e;
        }

        static public Entity CreateImage2D(string file, bool disableSmoothing = true, bool hasAlpha = true, bool bitmask = false)
        {
            var e = entityManager.CreateEntity();
            Image2D img = new Image2D();
            img.imagePixelSize = new float2(1, 1);
            img.hasAlpha = hasAlpha;
            img.disableSmoothing = disableSmoothing;
            entityManager.AddComponentData(e, img);
            entityManager.AddComponent(e, typeof(Image2DLoadFromFile));
            entityManager.AddBufferFromString<Image2DLoadFromFileImageFile>(e, file);
            if (bitmask)
            {
                var mask = new Image2DAlphaMask();
                mask.threshold = 0.5f;
                entityManager.AddComponentData(e, mask);
            }
            return e;
        }
        static public Entity CreateSprite(Entity image, int x0 = -1, int y0 = -1,
                                            int x1 = -1, int y1 = -1, int w = -1, int h = -1,
                                            float pixelToWorldUnits = 1f)
        {
            Entity e = entityManager.CreateEntity();

            entityManager.AddComponentData(e, new Sprite2D()
            {
                image = image,
                imageRegion = x0 >= 0f ? PixelRect(x0, y0, x1, y1, w, h) : new Rect() { x = 0f, y = 0f, width = 1f, height = 1f },
                pivot = new float2(0.5f, 0.5f),
                pixelsToWorldUnits = pixelToWorldUnits
            });

            return e;
        }

        static public Entity CreateRenderer(Entity sprite)
        {
            Entity e = entityManager.CreateEntity();
            entityManager.AddComponentData(e, new Sprite2DRenderer()
            {
                blending = BlendOp.Alpha,
                color = Colors.White,
                sprite = sprite,
            });
            return e;
        }

        static public Entity CreateSquare(Entity image, Color color, float cx = 0, float cy = 0, float cz = 0, float w = 1, float h = 1, float pixelsToWorldUnits = 1f)
        {
            var e = entityManager.CreateEntity();

            entityManager.AddComponent(e, typeof(Parent));
            entityManager.AddComponentData(e, new Translation() { Value = new float3(cx, cy, cz) });
            entityManager.AddComponentData(e, new NonUniformScale() { Value = new float3(w, h, 1) });

            var spr = new Sprite2DRenderer();
            spr.color = color;
            spr.blending = BlendOp.Alpha;
            entityManager.AddComponentData(e, spr);

            var sp = new Sprite2D();
            sp.imageRegion = new Rect(0f, 0f, 1f, 1f);
            sp.pivot = new float2(0.5f, 0.5f);
            sp.image = image;
            sp.pixelsToWorldUnits = pixelsToWorldUnits;
            entityManager.AddComponentData(e, sp);
            return e;
        }

        static public Entity CreateSpriteRect(Entity image, Color color, int x0 = -1, int y0 = -1, int x1 = -1, int y1 = -1, int w = -1, int h = -1, float pixelsToWorldUnits = 1f)
        {
            var e = entityManager.CreateEntity();

            entityManager.AddComponent(e, typeof(Parent));

            var spr = new Sprite2DRenderer();
            spr.color = color;
            spr.blending = BlendOp.Alpha;
            entityManager.AddComponentData(e, spr);

            var sp = new Sprite2D();
            sp.imageRegion = x0 >= 0 ? PixelRect(x0, y0, x1, y1, w, h) : new Rect() { x = 0, y = 0, width = 1, height = 1 };
            sp.pivot = new float2(0.5f, 0.5f);
            sp.image = image;
            sp.pixelsToWorldUnits = pixelsToWorldUnits;
            entityManager.AddComponentData(e, sp);
            return e;
        }
        static public Rect PixelRect(int x0, int y0, int x1, int y1, int w, int h)
        {
            // convert integer pixel coords inside integer pixel rectangle to normalized
            // float rectangle and y flip
            Rect r = new Rect();
            r.x = (float)x0 / (float)w;
            r.y = (float)y0 / (float)h;
            r.width = (float)(x1 - x0) / (float)w;
            r.height = (float)(y1 - y0) / (float)h;
            r.y = 1.0f - r.y - r.height;
            return r;
        }

        static public Entity CreateUICanvas()
        {
            var e = entityManager.CreateEntity();
            tinyEnv.SetEntityName(e, "ui_canvas");
            DisplayInfo di = tinyEnv.GetConfigData<DisplayInfo>();
            Entity cam = tinyEnv.GetEntityByName("default_camera");
            entityManager.AddComponentData(e, new UICanvas()
            {
                uiScaleMode = UIScaleMode.ConstantPixelSize,
                referenceResolution = new float2(di.width, di.height),
                matchWidthOrHeight = 0.258f,
                camera = cam
            });
            entityManager.AddComponentData(e, new Parent());
            entityManager.AddComponentData(e, new Translation() {
                Value = new float3(0.0f, 0.0f, 0.0f)
            });

            entityManager.AddComponentData(e, new RectTransform() {
                anchorMin = new float2(0.0f, 0.0f),
                anchorMax = new float2(1.0f, 1.0f),
                sizeDelta = new float2(0.0f, 0.0f),
                anchoredPosition = new float2(di.width/2.0f, di.height/2.0f),
                pivot = new float2(0.5f, 0.5f)
            });
            return e;
        }

        static public void AddRectTransform(Entity e)
        {
            DisplayInfo di = tinyEnv.GetConfigData<DisplayInfo>();
            entityManager.AddComponentData(e, new RectTransform()
            {
                anchorMin = new float2(0.0f, 0.0f),
                anchorMax = new float2(1.0f, 1.0f),
                sizeDelta = new float2(0.0f, 0.0f),
                anchoredPosition = new float2(0.0f, 0.0f),
                pivot = new float2(0.5f, 0.5f)
            });
        }

        static public bool MainLoop()
        {
            #if UNITY_DOTSPLAYER
            World.Active.Update();
            #endif
            return !World.Active.QuitUpdate;
        }

        static public void RunWindow()
        {
            World.Active.GetExistingSystem<WindowSystem>().InfiniteMainLoop(MainLoop);
            World.Active.Dispose();
            Console.WriteLine("Done");
        }
    }
}
