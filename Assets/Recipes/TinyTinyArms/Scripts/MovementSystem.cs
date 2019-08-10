using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

public class MovementSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var dt = (float)World.TinyEnvironment().frameDeltaTime;

        Entities.ForEach((ref Translation translation, ref Movement movement) =>
        {
            translation.Value.x += movement.speed * dt;
        });
    }
}
