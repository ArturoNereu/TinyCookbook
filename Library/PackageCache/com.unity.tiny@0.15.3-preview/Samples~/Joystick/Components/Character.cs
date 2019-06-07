using Unity.Authoring.Core;
using Unity.Entities;

namespace Joystick
{
    public struct Character : IComponentData
    {
        public Entity CharacterVisual;
        public float MoveTiltAngle;
        public float MoveTiltSpeed;
        [HideInInspector]
        public float CurrentTiltAngle;
        public float MoveSpeed;
    }
}
