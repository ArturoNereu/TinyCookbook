using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace HelloWorld
{
    public class RotateSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var time = (float)World.TinyEnvironment().frameTime;

            Entities.ForEach((ref Rotate rotate, ref Rotation rotation) =>
            {
                rotation.Value = quaternion.RotateZ(time * rotate.Speed);
            });
        }
    }
}

