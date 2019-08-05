using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Mathematics;

public class RotateSpriteSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var environment = World.TinyEnvironment();

        Entities.WithAll<Rotation, Rotate>().ForEach((ref Rotation localRotation, ref Rotate rotate) =>
        {
            localRotation.Value = quaternion.RotateZ((float)environment.frameTime * rotate.speed);
        });
    }
}
