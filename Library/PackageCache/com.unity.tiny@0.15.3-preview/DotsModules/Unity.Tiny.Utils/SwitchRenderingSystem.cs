
using System;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Input;
using Unity.Tiny.Core2D;
using Unity.Tiny.Rendering;
using Unity.Tiny.Debugging;
#if !UNITY_WEBGL
using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif
namespace Unity.Tiny.Utils
{

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class SwitchRenderingSystem : ComponentSystem
    {
        public bool Disable = false;
        protected override void OnUpdate()
        {
            if (Disable)
                return;
            var input = World.GetExistingSystem<InputSystem>();
            if (input.GetKeyDown(KeyCode.Return))
            {
                var tinyEnv = World.TinyEnvironment();
                DisplayInfo di = tinyEnv.GetConfigData<DisplayInfo>();
                if (di.renderMode == RenderMode.Canvas)
                {
                    di.renderMode = RenderMode.WebGL;
                }
                else
                {
                    di.renderMode = RenderMode.Canvas;
                }
                Debug.LogFormat("Switch render mode to {0}", di.renderMode.ToString());
                tinyEnv.SetConfigData(di);
            }
        }
    }
}
