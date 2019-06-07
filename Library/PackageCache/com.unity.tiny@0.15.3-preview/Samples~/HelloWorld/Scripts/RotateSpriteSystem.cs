using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

public class RotateSpriteSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var env = World.TinyEnvironment();
        Entities
            .WithAll<Sprite2DRenderer>()
            .ForEach((ref Rotation lclRot) =>
        {
            lclRot.Value = quaternion.RotateZ((float)env.frameTime);
        });
    }
}
