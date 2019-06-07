using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Input;

public class MoveSpriteSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var direction = new float3(0, 0, 0);
        var input = World.GetExistingSystem<InputSystem>();
        if (input.GetKey(KeyCode.RightArrow) || input.GetKey(KeyCode.D))
        {
            direction.x += 1;
        }
        if (input.GetKey(KeyCode.LeftArrow) || input.GetKey(KeyCode.A))
        {
            direction.x += -1;
        }
        if (input.GetKey(KeyCode.UpArrow) || input.GetKey(KeyCode.W))
        {
            direction.y += 1;
        }
        if (input.GetKey(KeyCode.DownArrow) || input.GetKey(KeyCode.S))
        {
            direction.y += -1;
        }

        var env = World.TinyEnvironment();
        Entities
            .WithAll<Sprite2DRenderer>()
            .ForEach((ref Translation lclPos) =>
        {
            lclPos.Value += direction * env.frameDeltaTime;
        });
    }
}
