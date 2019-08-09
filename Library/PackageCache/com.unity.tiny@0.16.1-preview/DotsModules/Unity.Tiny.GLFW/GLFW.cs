using System;
using Unity.Entities;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;

namespace Unity.Tiny.GLFW
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class GLFWWindowSystem : WindowSystem
    {
         public GLFWWindowSystem()
        {
            initialized = false;
        }

        public override void InfiniteMainLoop(MainLoopDelegate m)
        {
            while (!World.QuitUpdate)
            {
#if UNITY_DOTSPLAYER
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.FreeTempMemory();
#endif
                m();
            }
            Debug.Log("Infinite main loop exited.");
        }

        public override void DebugReadbackImage(out int w, out int h, out NativeArray<byte> pixels)
        {
            var env = World.TinyEnvironment();
            var config = env.GetConfigData<DisplayInfo>();
            pixels = new NativeArray<byte>(config.framebufferWidth*config.framebufferHeight*4, Allocator.Persistent);
            unsafe
            {
                GLFWNativeCalls.debugReadback(config.framebufferWidth, config.framebufferHeight, pixels.GetUnsafePtr());
            }
            w = config.framebufferWidth;
            h = config.framebufferHeight;
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            if (initialized)
                return;

            // setup window
            Debug.Log("GLFW Window init.");

            var env = World.TinyEnvironment();
            var config = env.GetConfigData<DisplayInfo>();
            try
            {
                initialized = GLFWNativeCalls.init(config.width, config.height);
            }
            catch
            {
                Debug.Log("  Excepted (Is lib_unity_tiny_glfw.dll missing?).");
                initialized = false;
            }
            if (!initialized)
            {
                Debug.Log("  Failed.");
                World.QuitUpdate = true;
            }
            int winw = 0, winh = 0;
            GLFWNativeCalls.getWindowSize(ref winw, ref winh);
            config.focused = true;
            config.visible = true;
            config.orientation = winw >= winh ? DisplayOrientation.Horizontal : DisplayOrientation.Vertical;
            config.frameWidth = winw;
            config.frameHeight = winh;
            int sw = 0, sh = 0;
            GLFWNativeCalls.getScreenSize(ref sw, ref sh);
            config.screenWidth = sw;
            config.screenHeight = sh;
            config.width = winw;
            config.height = winh;
            int fbw = 0, fbh = 0;
            GLFWNativeCalls.getFramebufferSize(ref fbw, ref fbh);
            config.framebufferWidth = fbw;
            config.framebufferHeight = fbh;
            env.SetConfigData(config);
            frameTime = GLFWNativeCalls.time();
        }

        protected override void OnDestroy()
        {
            // close window
            if (initialized)
            {
                Debug.Log("GLFW Window shutdown.");
                GLFWNativeCalls.shutdown(0);
                initialized = false;
            }
        }

        protected override void OnUpdate()
        {
            if (!initialized)
                return;
            GLFWNativeCalls.swapBuffers();
            var env = World.TinyEnvironment();
            var config = env.GetConfigData<DisplayInfo>();
            int winw = 0, winh = 0;
            GLFWNativeCalls.getWindowSize(ref winw, ref winh);
            if (winw != config.width || winh != config.height)
            {
                if (config.autoSizeToFrame)
                {
                    config.width = winw;
                    config.height = winh;
                    config.frameWidth = winw;
                    config.frameHeight = winh;
                    int fbw = 0, fbh = 0;
                    GLFWNativeCalls.getFramebufferSize(ref fbw, ref fbh);
                    config.framebufferWidth = fbw;
                    config.framebufferHeight = fbh;
                    env.SetConfigData(config);
                }
                else
                {
                    GLFWNativeCalls.resize(config.width, config.height);
                }
            }
            if (!GLFWNativeCalls.messagePump())
            {
                Debug.Log("GLFW message pump exit.");
                GLFWNativeCalls.shutdown(1);
                World.QuitUpdate = true;
                initialized = false;
                return;
            }
#if DEBUG
            GLFWNativeCalls.debugClear();
#endif
            double newFrameTime = GLFWNativeCalls.time();
            env.StepWallRealtimeFrame(newFrameTime - frameTime);
            frameTime = newFrameTime;
        }

        protected bool initialized;
        protected double frameTime;
    }

    public static class GLFWNativeCalls
    {
        [DllImport("lib_unity_tiny_glfw", EntryPoint = "init_glfw")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool init(int width, int height);

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "getWindowSize_glfw")]
        public static extern void getWindowSize(ref int width, ref int height);

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "getScreenSize_glfw")]
        public static extern void getScreenSize(ref int width, ref int height);

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "getFramebufferSize_glfw")]
        public static extern void getFramebufferSize(ref int width, ref int height);

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "getWindowFrameSize_glfw")]
        public static extern void getWindowFrameSize(ref int left, ref int top, ref int right, ref int bottom);

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "shutdown_glfw")]
        public static extern void shutdown(int exitCode);

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "resize_glfw")]
        public static extern void resize(int width, int height);

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "messagePump_glfw")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool messagePump();

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "swapBuffers_glfw")]
        public static extern void swapBuffers();

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "debugClear_glfw")]
        public static extern void debugClear();

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "debugReadback_glfw")]
        public static unsafe extern void debugReadback(int w, int h, void *pixels);

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "time_glfw")]
        public static extern double time();

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "getwindow_glfw")]
        public static extern unsafe void *getWindow();

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "get_key_stream_glfw_input")]
        public static extern unsafe int * getKeyStream(ref int len);

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "get_mouse_pos_stream_glfw_input")]
        public static extern unsafe int * getMousePosStream(ref int len);

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "get_mouse_button_stream_glfw_input")]
        public static extern unsafe int * getMouseButtonStream(ref int len);

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "init_glfw_input")]
        public static extern unsafe bool init(void *window);

        [DllImport("lib_unity_tiny_glfw", EntryPoint = "reset_glfw_input")]
        public static extern void resetStreams();
    }

}
