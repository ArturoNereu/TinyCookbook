using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace SpawnAndDestroy
{
    public class MoveShipSystem : ComponentSystem
    {
        Random _random;

        protected override void OnCreate()
        {
            _random = new Random();
            _random.InitState();
        }

        protected override void OnUpdate()
        {
            var deltaTime = World.TinyEnvironment().frameDeltaTime;
            Entities.ForEach((ref Ship ship, ref Translation translation) =>
            {
                if (!ship.Initialized)
                    return;

                translation.Value = math.lerp(translation.Value, ship.DestinationPosition, deltaTime * ship.MoveSpeed);
            });
        }
    }
}
