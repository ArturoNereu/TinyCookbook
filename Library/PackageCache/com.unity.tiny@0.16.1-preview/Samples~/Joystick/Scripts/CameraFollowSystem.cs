using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace Joystick
{
    public class CameraFollowSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var deltaTime = World.TinyEnvironment().frameDeltaTime;

            Entities.ForEach((Entity entity, ref CameraFollow cameraFollow, ref Translation transformPosition) =>
            {
                if (EntityManager.HasComponent<Translation>(cameraFollow.TargetToFollow))
                {
                    var transformPositionToFollow = EntityManager.GetComponentData<Translation>(cameraFollow.TargetToFollow);
                    var currentPos = transformPosition.Value;
                    var targetPos = transformPositionToFollow.Value;
                    var destination = transformPosition.Value;

                    // Follow the target if it's outside of the center dead zone rect
                    var deadZoneRadius = cameraFollow.DeadZone * 0.5f;
                    if (targetPos.x - currentPos.x > deadZoneRadius.x)
                        destination.x = targetPos.x - deadZoneRadius.x;
                    else if (currentPos.x - targetPos.x > deadZoneRadius.x)
                        destination.x = targetPos.x + deadZoneRadius.x;
                    if (targetPos.y - currentPos.y > deadZoneRadius.y)
                        destination.y = targetPos.y - deadZoneRadius.y;
                    else if (currentPos.y - targetPos.y > deadZoneRadius.y)
                        destination.y = targetPos.y + deadZoneRadius.y;

                    transformPosition.Value = math.lerp(currentPos, destination, deltaTime * cameraFollow.FollowSpeed);
                }
            });
        }
    }
}
