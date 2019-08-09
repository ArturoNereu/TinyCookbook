using Unity.Mathematics;
using Unity.Collections;
using Unity.Tiny.Input;

namespace Unity.Tiny.UIControls
{
    public struct PointerState
    {
        public PointerID id;
        public float2 pos;
        public bool down;
        public bool up;
        public bool willGone;
        public bool cancelled;
    };

    public static class PointerStateUtil
    {
        private const int MaxPointers = 8;

        public static NativeList<PointerState> GetPointersState(InputSystem input)
        {
            var pointers = new NativeList<PointerState>(MaxPointers, Allocator.Persistent);

            if (input.IsMousePresent())
                GetMouseState(input, ref pointers);

            if (input.IsTouchSupported())
                GetTouchState(input, ref pointers);

            return pointers;
        }

        private static void GetMouseState(InputSystem input, ref NativeList<PointerState> pointers)
        {
            if (pointers.Length == MaxPointers)
                return;

            var mousePos = input.GetWorldInputPosition();
            var mouseDown = input.GetMouseButtonDown(0);
            var mouseUp = input.GetMouseButtonUp(0);

            PointerState mouse = new PointerState()
            {
                id = new PointerID(PointerType.Mouse, 0),
                pos = mousePos.xy,
                down = mouseDown,
                up = mouseUp,
                willGone = false,
                cancelled = false
            };

            pointers.Add(mouse);
        }

        private static void GetTouchState(InputSystem input, ref NativeList<PointerState> pointers)
        {
            int touchCount = input.TouchCount();

            for (int i = 0; i < touchCount; i++)
            {
                if (pointers.Length == MaxPointers)
                    break;

                var touch = input.GetTouch(i);
                var worldPoint = input.TranslateScreenToWorld(new float2(touch.x, touch.y));
                bool touchDown = touch.phase == TouchState.Began;
                bool touchUp = touch.phase == TouchState.Ended;
                bool touchCancelled = touch.phase == TouchState.Canceled;

                var touchPointerId = new PointerID(PointerType.Touch, touch.fingerId);
                bool willGone = touchUp;
                PointerState touchPointer = new PointerState()
                {
                    id = touchPointerId,
                    pos = worldPoint.xy,
                    down = touchDown,
                    up = touchUp,
                    willGone = willGone,
                    cancelled = touchCancelled
                };

                pointers.Add(touchPointer);
            }
        }
    }
}
