using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace SpawnAndDestroy
{
    /// <summary>
    /// Init ships after they have been loaded (loading a scene is async).
    /// Set their start position on the edge of the screen and set a random destination.
    /// </summary>
    public class InitShipSystem : ComponentSystem
    {
        Random _random;
        float _orthographicSize = 200f;

        protected override void OnCreate()
        {
            _random = new Random();
            _random.InitState();
        }

        protected override void OnUpdate()
        {
            var displayInfo = World.TinyEnvironment().GetConfigData<DisplayInfo>();
            var aspectRatio = (float)displayInfo.width / displayInfo.height;
            var screenEdgeXPos = aspectRatio * _orthographicSize + 20f;

            Entities.ForEach((ref Ship ship, ref Translation translation) =>
            {
                if (!ship.Initialized)
                {
                    ship.Initialized = true;

                    // Set random start position
                    translation.Value = _random.NextFloat3(new float3(screenEdgeXPos, -_orthographicSize, 0f), new float3(screenEdgeXPos, _orthographicSize, 0f));
                        if (ship.IsAlly)
                            translation.Value.x *= -1f;

                    // Set random destination
                    SetRandomShipDestination(ref ship);
                }
            });
        }

        void SetRandomShipDestination(ref Ship ship)
        {
            ship.DestinationPosition = _random.NextFloat3(new float3(0f, -_orthographicSize, 0f), new float3(_orthographicSize * 2f, _orthographicSize, 0f));

            if (ship.IsAlly)
                ship.DestinationPosition.x *= -1f;
        }
    }
}
