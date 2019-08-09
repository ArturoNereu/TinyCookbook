using Unity.Entities;
using Unity.Mathematics;

namespace Joystick
{
    public struct CameraFollow : IComponentData
    {
        public Entity TargetToFollow;
        public float2 DeadZone;
        public float FollowSpeed;
    }
}
