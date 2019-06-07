using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using Unity.Tiny.Input;
using Unity.Tiny.Core;

#if !UNITY_WEBGL
    using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
    using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

namespace DragAndDrop
{
    /// <summary>
    /// Use 1 or more fingers to drag and drop any entity that has the Draggable component.
    /// </summary>
    public class MultiTouchDragSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();

            var startDragEntity = Entity.Null;
            var startDragTouchID = -1;
            var dragOffset = float3.zero;
            var draggableSortOrder = -1;
            Entities.ForEach((Entity entity, ref Draggable draggable, ref DragAnimation dragAnimation, ref Translation transformLocalPosition) =>
            {
                for (int i = 0; i < inputSystem.TouchCount(); i++)
                {
                    var touch = inputSystem.GetTouch(i);
                    var inputPosition = GetTouchWorldPosition(i);
                    if (touch.phase == TouchState.Began && !draggable.IsLocked)
                    {
                        if (draggable.InMouseDrag || draggable.InKeyboardDrag || draggable.TouchID >= 0 || inputSystem.GetMouseButtonDown(0))
                        {
                            return;
                        }

                        // Start touch drag
                        var overlapsObject = OverlapsObjectCollider(transformLocalPosition.Value, inputPosition, draggable.Size);
                        if (overlapsObject && dragAnimation.DefaultSortOrder > draggableSortOrder)
                        {
                            startDragTouchID = touch.fingerId;
                            startDragEntity = entity;
                            draggableSortOrder = dragAnimation.DefaultSortOrder;
                            dragOffset = new float3(
                                transformLocalPosition.Value.x - inputPosition.x,
                                transformLocalPosition.Value.y - inputPosition.y,
                                0f);
                        }
                    }
                    else if (draggable.TouchID == touch.fingerId)
                    {
                        if (!draggable.IsLocked && (touch.phase == TouchState.Moved || touch.phase == TouchState.Stationary))
                        {
                            // While dragging, set the dragged object position to the touch world position
                            transformLocalPosition.Value = new float3(
                                inputPosition.x + draggable.DragOffset.x,
                                inputPosition.y + draggable.DragOffset.y,
                                0f);
                        }
                        else if (touch.phase == TouchState.Ended || touch.phase == TouchState.Canceled)
                        {
                            // End touch drag
                            draggable.TouchID = -1;
                        }
                    }
                }
            });

            if (startDragEntity != Entity.Null)
            {
                var draggable = EntityManager.GetComponentData<Draggable>(startDragEntity);
                draggable.TouchID = startDragTouchID;
                draggable.DragOffset = dragOffset;
                EntityManager.SetComponentData(startDragEntity, draggable);
            }
        }

        bool OverlapsObjectCollider(float3 position, float3 inputPosition, float2 size)
        {
            var rect = new Rect(position.x - size.x * 0.5f, position.y - size.y * 0.5f, size.x, size.y);
            return rect.Contains(inputPosition.xy);
        }

        float3 GetTouchWorldPosition(int index)
        {
            var env = World.TinyEnvironment();
            
            var displayInfo = World.TinyEnvironment().GetConfigData<DisplayInfo>();
            var inputSystem = World.GetExistingSystem<InputSystem>();

            var touch = inputSystem.GetTouch(index);
            var cameraEntity = Entity.Null;
            Entities.WithAll<Camera2D>().ForEach((Entity entity) => { cameraEntity = entity; });
            var windowPosition = new float2(touch.x, touch.y);
            var windowSize = new float2(displayInfo.width, displayInfo.height);
            return TransformHelpers.WindowToWorld(this, cameraEntity, windowPosition, windowSize);
        }
    }
}
