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
    /// Use the arrow keys of the keyboard to navigate and change the entity that is currently selected by the virtual cursor.
    /// Add the SelectionCursor component to the virtual cursor entity and the Draggable component to objects that can be selected.
    /// </summary>
    public class VirtualCursorSelectSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var env = World.TinyEnvironment();
            var selectionCursorEntity = Entity.Null;
            Entities.WithAll<SelectionCursor>().ForEach((Entity entity) => { selectionCursorEntity = entity; });
            if (!EntityManager.Exists(selectionCursorEntity))
            {
                return;
            }

            var selectionCursor = EntityManager.GetComponentData<SelectionCursor>(selectionCursorEntity);
            if (EntityManager.Exists(selectionCursor.SelectedEntity))
            {
                // Move cursor to neighbor draggable object
                var draggable = EntityManager.GetComponentData<Draggable>(selectionCursor.SelectedEntity);
                if (!draggable.InKeyboardDrag && !selectionCursor.IsLocked)
                {
                    if (LeftDown())
                    {
                        MoveCursor(selectionCursorEntity, ref selectionCursor, 270f);
                    }
                    else if (RightDown())
                    {
                        MoveCursor(selectionCursorEntity, ref selectionCursor, 90f);
                    }
                    else if (UpDown())
                    {
                        MoveCursor(selectionCursorEntity, ref selectionCursor, 180f);
                    }
                    else if (DownDown())
                    {
                        MoveCursor(selectionCursorEntity, ref selectionCursor, 0f);
                    }
                }

                // Set cursor position to follow the currently selected object
                var cursorTransformPosition = EntityManager.GetComponentData<Translation>(selectionCursorEntity);
                var selectedTransformPosition = EntityManager.GetComponentData<Translation>(selectionCursor.SelectedEntity);
                cursorTransformPosition.Value = selectedTransformPosition.Value;
                EntityManager.SetComponentData(selectionCursorEntity, cursorTransformPosition);

                // Animate cursor scale
                selectionCursor.ScaleAnimationTimer += (float)env.frameDeltaTime;
                var cursorTransformScale = EntityManager.GetComponentData<NonUniformScale>(selectionCursorEntity);
                var scale = selectionCursor.IsVisible && !draggable.InKeyboardDrag && !draggable.InMouseDrag ? 1f + math.sin(selectionCursor.ScaleAnimationTimer * 5f) * 0.04f : 0f;
                cursorTransformScale.Value = new float3(scale, scale, 1f);
                EntityManager.SetComponentData(selectionCursorEntity, cursorTransformScale);
                EntityManager.SetComponentData(selectionCursorEntity, selectionCursor);
            }
        }

        void MoveCursor(Entity selectionCursorEntity, ref SelectionCursor selectionCursor, float moveDirectionAngle)
        {
            var cursorTransformPosition = EntityManager.GetComponentData<Translation>(selectionCursorEntity);

            var closestEntity = Entity.Null;
            float closestDistance = 0f;
            Entities.ForEach((Entity entity, ref Draggable draggable, ref Translation transformLocalPosition) =>
            {
                var angle = math.atan2(cursorTransformPosition.Value.y - transformLocalPosition.Value.y,
                    cursorTransformPosition.Value.x - transformLocalPosition.Value.x) - math.PI / 2f;
                angle = (360f + angle * 180f / math.PI) % 360f;

                var angleDiff = (float)math.abs((angle + 360f) - (moveDirectionAngle + 360f));
                if (angleDiff > 270f)
                    angleDiff = 360f - angleDiff;

                var validAngleSpan = 80f;
                var distance = math.distance(cursorTransformPosition.Value, transformLocalPosition.Value);
                if (distance > 0.01f && angleDiff < validAngleSpan)
                {
                    distance += distance * 2f * angleDiff / validAngleSpan; // Apply distance penalty for objects at wide angle
                    if (distance < closestDistance || !EntityManager.Exists(closestEntity))
                    {
                        closestDistance = distance;
                        closestEntity = entity;
                    }
                }
            });

            if (EntityManager.Exists(closestEntity))
            {
                selectionCursor.SelectedEntity = closestEntity;
                selectionCursor.IsVisible = true;
            }
        }

        public bool LeftDown()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();
            return inputSystem.GetKeyDown(KeyCode.LeftArrow) || inputSystem.GetKeyDown(KeyCode.A);
        }

        public bool RightDown()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();
            return inputSystem.GetKeyDown(KeyCode.RightArrow) || inputSystem.GetKeyDown(KeyCode.D);
        }

        public bool UpDown()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();
            return inputSystem.GetKeyDown(KeyCode.UpArrow) || inputSystem.GetKeyDown(KeyCode.W);
        }

        public bool DownDown()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();
            return inputSystem.GetKeyDown(KeyCode.DownArrow) || inputSystem.GetKeyDown(KeyCode.S);
        }
    }
}
