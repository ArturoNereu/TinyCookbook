using System;
using System.Diagnostics;
using Unity.Entities;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;

namespace Unity.Tiny.HTML
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class HTMLWindowSystem : WindowSystem
    {
        public HTMLWindowSystem()
        {
            initialized = false;
        }

        private static MainLoopDelegate staticM;

        [MonoPInvokeCallbackAttribute]
        static bool ManagedRAFCallback()
        {
#if UNITY_DOTSPLAYER
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.FreeTempMemory();
#endif
            return staticM();
        }

        internal class MonoPInvokeCallbackAttribute : Attribute
        {
        }

        public override void InfiniteMainLoop(MainLoopDelegate m)
        {
            staticM = m;
            HTMLNativeCalls.set_animation_frame_callback(Marshal.GetFunctionPointerForDelegate((MainLoopDelegate)ManagedRAFCallback));
            Console.WriteLine("HTML Main loop exiting.");
        }

        public override void DebugReadbackImage(out int w, out int h, out NativeArray<byte> pixels)
        {
            var env = World.TinyEnvironment();
            var config = env.GetConfigData<DisplayInfo>();
            pixels = new NativeArray<byte>(config.framebufferWidth*config.framebufferHeight*4, Allocator.Persistent);
            unsafe
            {
                HTMLNativeCalls.debugReadback(config.framebufferWidth, config.framebufferHeight, pixels.GetUnsafePtr());
            }

            w = config.framebufferWidth;
            h = config.framebufferHeight;
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            if (initialized)
                return;

            Console.WriteLine("HTML Window init.");
            try
            {
                initialized = HTMLNativeCalls.init();
            }
            catch
            {
                Console.WriteLine("  Excepted (Is lib_unity_tiny2d_html.dll missing?).");
                initialized = false;
            }
            if (!initialized)
            {
                Console.WriteLine("  Failed.");
                return;
            }

            var env = World.TinyEnvironment();
            var config = env.GetConfigData<DisplayInfo>();
            HTMLNativeCalls.getFrameSize(ref config.frameWidth, ref config.frameHeight);
            HTMLNativeCalls.getScreenSize(ref config.screenWidth, ref config.screenHeight);
            if (config.autoSizeToFrame)
            {
                config.width = config.frameWidth;
                config.height = config.frameHeight;
            }
            bool wantwebgl = config.renderMode == RenderMode.Auto || config.renderMode == RenderMode.WebGL;
            iswebgl = HTMLNativeCalls.setCanvasSizeAndMode(config.width, config.height, wantwebgl);
            config.renderMode = iswebgl ? RenderMode.WebGL : RenderMode.Canvas;
            config.framebufferWidth = config.width;
            config.framebufferHeight = config.height;
            env.SetConfigData(config);

            frameTime = HTMLNativeCalls.time();
        }

        protected override void OnDestroy()
        {
            // close window
            Console.WriteLine("HTML Window shutdown.");
            HTMLNativeCalls.shutdown(0);
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            if (!initialized)
                return;
            var env = World.TinyEnvironment();
            var config = env.GetConfigData<DisplayInfo>();

            var wantwebgl = config.renderMode == RenderMode.Auto || config.renderMode == RenderMode.WebGL;
            int wCanvas = 0, hCanvas = 0;
            HTMLNativeCalls.getCanvasSize(ref wCanvas, ref hCanvas);
            HTMLNativeCalls.getFrameSize(ref config.frameWidth, ref config.frameHeight);
            if (config.autoSizeToFrame)
            {
                config.width = config.frameWidth;
                config.height = config.frameHeight;
            }
            if (wCanvas != config.width || hCanvas != config.height || wantwebgl != iswebgl)
            {
                iswebgl = HTMLNativeCalls.setCanvasSizeAndMode(config.width, config.height, wantwebgl);
                config.renderMode = iswebgl ? RenderMode.WebGL : RenderMode.Canvas;
                config.framebufferWidth = config.width;
                config.framebufferHeight = config.height;
            }
            env.SetConfigData(config);

            double newFrameTime = HTMLNativeCalls.time();
            env.StepWallRealtimeFrame(newFrameTime - frameTime);
            frameTime = newFrameTime;
        }

        protected bool initialized;
        protected bool iswebgl;

        protected double frameTime;
    }

    static class HTMLNativeCalls
    {
        // calls to HTMLWrapper.cpp
        [DllImport("lib_unity_tiny_html", EntryPoint = "init_html")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool init();

        [DllImport("lib_unity_tiny_html", EntryPoint = "shutdown_html")]
        public static extern void shutdown(int exitCode);

        [DllImport("lib_unity_tiny_html", EntryPoint = "time_html")]
        public static extern double time();

        [DllImport("lib_unity_tiny_html", EntryPoint = "rafcallbackinit_html")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool set_animation_frame_callback(IntPtr func);

        // calls to HTMLWrapper.js directly
        [DllImport("lib_unity_tiny_html", EntryPoint = "js_html_setCanvasSize")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool setCanvasSizeAndMode(int width, int height, bool webgl);

        [DllImport("lib_unity_tiny_html", EntryPoint = "js_html_debugReadback")]
        public static unsafe extern void debugReadback(int w, int h, void *pixels);

        [DllImport("lib_unity_tiny_html", EntryPoint = "js_html_getCanvasSize")]
        public static extern void getCanvasSize(ref int w, ref int h);

        [DllImport("lib_unity_tiny_html", EntryPoint = "js_html_getFrameSize")]
        public static extern void getFrameSize(ref int w, ref int h);

        [DllImport("lib_unity_tiny_html", EntryPoint = "js_html_getScreenSize")]
        public static extern void getScreenSize(ref int w, ref int h);
    }

}

