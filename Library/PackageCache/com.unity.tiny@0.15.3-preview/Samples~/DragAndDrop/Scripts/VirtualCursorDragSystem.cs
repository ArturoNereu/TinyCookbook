using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Input;

#if !UNITY_WEBGL
using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
    using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

namespace DragAndDrop
{
    /// <summary>
    /// Use the arrow keys and the Space key to drag and drop the entity selected by the virtual cursor.
    /// Add the SelectionCursor component to the virtual cursor entity and the Draggable component to objects that can be dragged.
    /// </summary>
    public class VirtualCursorDragSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var env = World.TinyEnvironment();
            var selectionCursorEntity = Entity.Null;
            Entities.WithAll<SelectionCursor>().ForEach((Entity entity) => { selectionCursorEntity = entity; });
            if (!EntityManager.Exists(selectionCursorEntity))
                return;

            var selectionCursor = EntityManager.GetComponentData<SelectionCursor>(selectionCursorEntity);
            var selectedEntity = selectionCursor.SelectedEntity;
            if (!EntityManager.Exists(selectedEntity))
                return;

            var draggable = EntityManager.GetComponentData<Draggable>(selectedEntity);
            var transformLocalPosition = EntityManager.GetComponentData<Translation>(selectedEntity);
            var deltaTime = World.TinyEnvironment().frameDeltaTime;
            if (GrabObjectDown() && !draggable.IsLocked && !draggable.InMouseDrag && draggable.TouchID < 0)
            {
                // Start to drag on key down
                draggable.InKeyboardDrag = true;
                draggable.DragOffset = new float3();
                draggable.DragStartPosition = transformLocalPosition.Value;
            }
            else if (draggable.InKeyboardDrag)
            {
                if (GrabObjectPressed() && !draggable.IsLocked)
                {
                    // While dragging, move the dragged object in the direction the user is pointing with the input keys or joystick
                    var inputDirection = GetKeyboardInputDirection();
                    draggable.DragOffset = new float3(
                        draggable.DragOffset.x + inputDirection.x * deltaTime * draggable.KeyboardDragMoveSpeed,
                        draggable.DragOffset.y + inputDirection.y * deltaTime * draggable.KeyboardDragMoveSpeed,
                        0f);

                    transformLocalPosition.Value = new float3(
                        draggable.DragStartPosition.x + draggable.DragOffset.x,
                        draggable.DragStartPosition.y + draggable.DragOffset.y,
                        0f);

                    EntityManager.SetComponentData(selectedEntity, transformLocalPosition);
                }
                else
                {
                    draggable.InKeyboardDrag = false;
                }
            }

            EntityManager.SetComponentData(selectedEntity, draggable);
        }

        public float2 GetKeyboardInputDirection()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();
            var inputDirection = new float2(0f, 0f);

            if (LeftPressed())
                inputDirection.x -= 1f;
            if (RightPressed())
                inputDirection.x += 1f;
            if (UpPressed())
                inputDirection.y += 1f;
            if (DownPressed())
                inputDirection.y -= 1f;

            return inputDirection;
        }

        public bool LeftPressed()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();
            return inputSystem.GetKey(KeyCode.LeftArrow) || inputSystem.GetKey(KeyCode.A);
        }

        public bool RightPressed()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();
            return inputSystem.GetKey(KeyCode.RightArrow) || inputSystem.GetKey(KeyCode.D);
        }

        public bool UpPressed()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();
            return inputSystem.GetKey(KeyCode.UpArrow) || inputSystem.GetKey(KeyCode.W);
        }

        public bool DownPressed()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();
            return inputSystem.GetKey(KeyCode.DownArrow) || inputSystem.GetKey(KeyCode.S);
        }

        public bool GrabObjectDown()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();
            return inputSystem.GetKeyDown(KeyCode.Space) || inputSystem.GetKeyDown(KeyCode.Return);
        }

        public bool GrabObjectPressed()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();
            return inputSystem.GetKey(KeyCode.Space) || inputSystem.GetKey(KeyCode.Return);
        }
    }
}
