using Unity.Entities;
using Unity.Tiny.UIControls;
using Unity.Tiny.Scenes;
using Unity.Mathematics;
using Unity.Tiny.Core;

namespace SpawnAndDestroy
{
    /// <summary>
    /// Spawn ships when user presses a button that has the ButtonSpawnShips component.
    /// This system takes a random SceneReference found on the Configuration entity and spawns a ship using LoadSceneAsync.
    /// </summary>
    public class SpawnShipSystem : ComponentSystem
    {
        Random _random;

        protected override void OnCreate()
        {
            _random = new Random();
            _random.InitState();
        }

        protected override void OnUpdate()
        {
            var env = World.TinyEnvironment();

            // Check if the user clicked the button to spawn ships
            var spawnShips = false;
            Entities.WithAll<ButtonSpawnShips>().ForEach((Entity entity, ref PointerInteraction pointerInteraction) =>
            {
                spawnShips = pointerInteraction.clicked;
            });

            // Spawn twice the number of ships we currently have
            if (spawnShips)
            {
                var existingShipCount = 0;
                Entities.WithAll<Ship>().ForEach((Entity shipEntity) => { existingShipCount++; });

                var allyShips = env.GetConfigBufferData<AllyShips>().Reinterpret<SceneReference>().ToNativeArray(Unity.Collections.Allocator.Temp);
                var enemyShips = env.GetConfigBufferData<EnemyShips>().Reinterpret<SceneReference>().ToNativeArray(Unity.Collections.Allocator.Temp);

                var toSpawnCount = existingShipCount == 0 ? 2 : existingShipCount;
                for (int i = 0; i < toSpawnCount; i++)
                {
                    if (i % 2 == 0)
                        SceneService.LoadSceneAsync(allyShips[_random.NextInt(allyShips.Length)]);
                    else
                        SceneService.LoadSceneAsync(enemyShips[_random.NextInt(enemyShips.Length)]);
                }

                allyShips.Dispose();
                enemyShips.Dispose();
            }
        }
    }
}
