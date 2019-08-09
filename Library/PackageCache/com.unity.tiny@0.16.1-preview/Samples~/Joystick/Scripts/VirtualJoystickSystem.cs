using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Input;
using Unity.Tiny.UILayout;

#if !UNITY_WEBGL
using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
    using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

namespace Joystick
{
    /// <summary>
    /// Captures the input from a virtual Joystick.
    /// To use, add the Joystick component along with a RectTransform 
    /// on the entity that has the joystick sprite.
    /// </summary>
    public class VirtualJoystickSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var displayInfo = World.TinyEnvironment().GetConfigData<DisplayInfo>();
            var input = EntityManager.World.GetExistingSystem<InputSystem>();

            Entities.ForEach((Entity entity, ref Joystick joystick, ref RectTransform rectTransform) =>
            {
                var stickPosition = float2.zero;
                joystick.Direction = float2.zero;
                Touch currentTouch = default;
                var isTouchPressed = false;

                // Start mouse joystick press
                if (!joystick.IsPressed && input.GetMouseButtonDown(0) && IsPointerOver(entity, rectTransform, input.GetInputPosition()))
                {
                    joystick.IsPressed = true;
                    joystick.CurrentFingerID = -1;
                }

                // Start touch joystick press
                for (int i = 0; i < input.TouchCount(); i++)
                {
                    var touch = input.GetTouch(i);
                    if (!joystick.IsPressed && joystick.CurrentFingerID < 0 && touch.phase == TouchState.Began)
                    {
                        var touchScreenPosition = new float2(touch.x, touch.y);
                        if (IsPointerOver(entity, rectTransform, touchScreenPosition))
                        {
                            currentTouch = touch;
                            joystick.IsPressed = true;
                            joystick.CurrentFingerID = touch.fingerId;
                        }
                    }

                    if (touch.fingerId == joystick.CurrentFingerID &&
                        (touch.phase == TouchState.Began || touch.phase == TouchState.Moved || touch.phase == TouchState.Stationary))
                    {
                        isTouchPressed = true;
                        currentTouch = touch;
                    }
                }

                // End joystick press
                if (joystick.IsPressed)
                {
                    if ((joystick.CurrentFingerID == -1 && input.GetMouseButtonUp(0)) || (joystick.CurrentFingerID >= 0 && !isTouchPressed))
                    {
                        joystick.IsPressed = false;
                        joystick.CurrentFingerID = -1;
                    }
                }

                // Update joystick pressed direction
                {
                    var joystickCenterWorldPosition = ComputeWorldPosition(entity);
                    var pointerWorldPosition = joystickCenterWorldPosition;
                    var direction = float3.zero;
                    var normalizedDirection = float3.zero;
                    if (joystick.IsPressed)
                    {
                        pointerWorldPosition = isTouchPressed ? GetWorldPosition(new float2(currentTouch.x, currentTouch.y)) : input.GetWorldInputPosition();
                        direction = pointerWorldPosition - joystickCenterWorldPosition;
                        normalizedDirection = math.normalize(direction);
                    }

                    var magnitude = math.min(joystick.StickZoneRadius, math.distance(pointerWorldPosition, joystickCenterWorldPosition));
                    var magnitudeRatio = magnitude / joystick.StickZoneRadius;

                    if (input.GetKey(KeyCode.W) || input.GetKey(KeyCode.UpArrow))
                    {
                        normalizedDirection.y += 1f;
                        magnitudeRatio = 1f;
                    }
                    if (input.GetKey(KeyCode.A) || input.GetKey(KeyCode.LeftArrow))
                    {
                        normalizedDirection.x -= 1f;
                        magnitudeRatio = 1f;
                    }
                    if (input.GetKey(KeyCode.S) || input.GetKey(KeyCode.DownArrow))
                    {
                        normalizedDirection.y -= 1f;
                        magnitudeRatio = 1f;
                    }
                    if (input.GetKey(KeyCode.D) || input.GetKey(KeyCode.RightArrow))
                    {
                        normalizedDirection.x += 1f;
                        magnitudeRatio = 1f;
                    }

                    if (normalizedDirection.x != 0f || normalizedDirection.y != 0f)
                        normalizedDirection = math.normalize(normalizedDirection);

                    stickPosition = normalizedDirection.xy * joystick.StickZoneRadius * magnitudeRatio;
                    var joystickDirection = normalizedDirection.xy * magnitudeRatio;
                    if (magnitudeRatio <= joystick.DeadZone)
                    {
                        joystickDirection = float2.zero;
                    }

                    joystick.Direction = joystickDirection;
                }

                // Update the position of the stick in the UI
                if (EntityManager.HasComponent<RectTransform>(joystick.Stick))
                {
                    var stickRectTransform = EntityManager.GetComponentData<RectTransform>(joystick.Stick);
                    stickRectTransform.anchoredPosition = stickPosition;
                    EntityManager.SetComponentData(joystick.Stick, stickRectTransform);
                }
            });
        }

        bool IsPointerOver(Entity entity, RectTransform rectTransform, float2 screenPosition)
        {
            var input = EntityManager.World.GetExistingSystem<InputSystem>();
            var pointerWorldPosition = GetWorldPosition(screenPosition);
            var worldPosition = ComputeWorldPosition(entity);

            var isPointerOverButton = pointerWorldPosition.x < worldPosition.x + rectTransform.sizeDelta.x / 2f &&
                pointerWorldPosition.x > worldPosition.x - rectTransform.sizeDelta.x / 2f &&
                pointerWorldPosition.y < worldPosition.y + rectTransform.sizeDelta.y / 2f &&
                pointerWorldPosition.y > worldPosition.y - rectTransform.sizeDelta.y / 2f;
            return isPointerOverButton;
        }

        float3 GetWorldPosition(float2 screenPosition)
        {
            var env = World.TinyEnvironment();
            var displayInfo = env.GetConfigData<DisplayInfo>();
            var inputSystem = World.GetExistingSystem<InputSystem>();

            var cameraEntity = Entity.Null;
            Entities.WithAll<Camera2D>().ForEach((Entity entity) => { cameraEntity = entity; });
            var windowPosition = new float2(screenPosition.x, screenPosition.y);
            var windowSize = new float2(displayInfo.width, displayInfo.height);
            return TransformHelpers.WindowToWorld(this, cameraEntity, windowPosition, windowSize);
        }

        float3 ComputeWorldPosition(Entity entity)
        {
            if (!EntityManager.HasComponent<LocalToWorld>(entity))
                return float3.zero;

            return EntityManager.GetComponentData<LocalToWorld>(entity).Value.c3.xyz;
        }
    }
}
