using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

#if !UNITY_WEBGL
    using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
    using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

namespace DragAndDrop
{
    /// <summary>
    /// Use the mouse to drag and drop any entity that has the Draggable component.
    /// </summary>
    public class MouseDragSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();

            var startDragEntity = Entity.Null;
            var dragOffset = float3.zero;
            var draggableSortOrder = -1;
            Entities.ForEach((Entity entity, ref Draggable draggable, ref DragAnimation dragAnimation, ref Translation transformLocalPosition) =>
            {
                var inputPosition = inputSystem.GetWorldInputPosition();
                if (inputSystem.GetMouseButtonDown(0) && !draggable.IsLocked && !draggable.InKeyboardDrag && draggable.TouchID < 0)
                {
                    // Start to drag on mouse down, if pointer is on draggable object
                    var overlapsObject = OverlapsObjectCollider(transformLocalPosition.Value, inputPosition, draggable.Size);
                    if (overlapsObject && dragAnimation.DefaultSortOrder > draggableSortOrder)
                    {
                        startDragEntity = entity;
                        draggableSortOrder = dragAnimation.DefaultSortOrder;
                        dragOffset = new float3(
                            transformLocalPosition.Value.x - inputPosition.x,
                            transformLocalPosition.Value.y - inputPosition.y,
                            0f);
                    }
                }
                else if (draggable.InMouseDrag)
                {
                    if (inputSystem.GetMouseButton(0) && !draggable.IsLocked)
                    {
                        // While dragging, set the dragged object position to the pointer position
                        transformLocalPosition.Value = new float3(
                            inputPosition.x + draggable.DragOffset.x,
                            inputPosition.y + draggable.DragOffset.y,
                            0f);
                    }
                    else
                    {
                        draggable.InMouseDrag = false;
                    }
                }
            });

            if (startDragEntity != Entity.Null)
            {
                var draggable = EntityManager.GetComponentData<Draggable>(startDragEntity);
                draggable.InMouseDrag = true;
                draggable.DragOffset = dragOffset;
                EntityManager.SetComponentData(startDragEntity, draggable);
            }
        }

        bool OverlapsObjectCollider(float3 position, float3 inputPosition, float2 size)
        {
            var rect = new Rect(position.x - size.x * 0.5f, position.y - size.y * 0.5f, size.x, size.y);
            return rect.Contains(inputPosition.xy);
        }
    }
}
