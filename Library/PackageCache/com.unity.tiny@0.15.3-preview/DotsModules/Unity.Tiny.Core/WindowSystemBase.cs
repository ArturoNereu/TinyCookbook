using System;
using Unity.Entities;
using Unity.Collections;

namespace Unity.Tiny.Core
{
    public abstract class WindowSystem : ComponentSystem
    {
        public delegate bool MainLoopDelegate();
        void quit()
        {
            World.QuitUpdate = true;
        }
        public abstract void InfiniteMainLoop(MainLoopDelegate m);
        public abstract void DebugReadbackImage(out int w, out int h, out NativeArray<byte> pixels);
    }

}
