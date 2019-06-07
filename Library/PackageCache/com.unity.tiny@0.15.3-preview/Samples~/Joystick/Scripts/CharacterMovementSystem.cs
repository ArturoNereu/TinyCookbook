using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace Joystick
{
    /// <summary>
    /// Control the character's movement based on the current Joystick input direction.
    /// </summary>
    public class CharacterMovementSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var deltaTime = World.TinyEnvironment().frameDeltaTime;
            var moveDirection = float2.zero;
            var moveMagnitude = 0f;

            // Fetch joystick input movement
            Entities.ForEach((Entity entity, ref Joystick joystick) =>
            {
                if (joystick.Direction.x != 0f || joystick.Direction.y != 0f)
                {
                    moveMagnitude = math.min(1f, math.distance(float2.zero, joystick.Direction));
                    moveDirection = moveMagnitude * math.normalize(joystick.Direction);
                }
            });

            Entities.ForEach((Entity entity, ref Character character, ref Translation transformPosition) =>
            {
                // Move the character
                var position = transformPosition.Value;
                position.x += moveDirection.x * character.MoveSpeed * deltaTime;
                position.y += moveDirection.y * character.MoveSpeed * deltaTime;

                // TODO: Remove these hardcoded boundary checks once we have physical walls on the scene
                if (position.x > 655f)
                    position.x = 655f;
                else if (position.x < -610f)
                    position.x = -610f;
                if (position.y > 96f)
                    position.y = 96f;
                else if (position.y < -200f)
                    position.y = -200f;

                transformPosition.Value = position;

                // Tilt the character when moving and flip the scale according to the facing direction
                var targetTiltAngle = 0f;
                if (moveDirection.x != 0f && EntityManager.HasComponent<NonUniformScale>(character.CharacterVisual))
                {
                    targetTiltAngle = character.MoveTiltAngle * moveMagnitude * math.sign(moveDirection.x);

                    var transformScale = EntityManager.GetComponentData<NonUniformScale>(character.CharacterVisual);
                    transformScale.Value.x = -math.sign(moveDirection.x);
                    EntityManager.SetComponentData(character.CharacterVisual, transformScale);
                }

                character.CurrentTiltAngle = math.lerp(character.CurrentTiltAngle, targetTiltAngle, deltaTime * character.MoveTiltSpeed);
                if (EntityManager.HasComponent<Rotation>(character.CharacterVisual))
                {
                    var transformRotation = EntityManager.GetComponentData<Rotation>(character.CharacterVisual);
                    transformRotation.Value = quaternion.RotateZ(math.radians(character.CurrentTiltAngle));
                    EntityManager.SetComponentData(character.CharacterVisual, transformRotation);
                }
            });
        }
    }
}
