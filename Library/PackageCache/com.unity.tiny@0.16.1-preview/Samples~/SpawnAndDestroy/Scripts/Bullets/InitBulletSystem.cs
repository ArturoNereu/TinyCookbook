using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using Unity.Tiny.Scenes;

namespace SpawnAndDestroy
{
    /// <summary>
    /// When a bullet has been loaded (scene loading is async),
    /// find a ship that wants to spawn a bullet (SpawningBullet is true) and
    /// set the bullet position at that ship's position.
    /// </summary>
    public class InitBulletSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var entitiesToUnload = new NativeList<Entity>(Allocator.Temp);

            Entities.ForEach((Entity bulletEntity, ref Bullet bullet, ref Translation bulletTransformPosition) =>
            {
                if (!bullet.Initialized)
                {
                    bullet.Initialized = true;

                    var shipFound = false;
                    var bulletPosition = float3.zero;
                    var isAllyShip = false;
                    Entities.ForEach((ref Ship ship, ref Translation shipTranslation) =>
                    {
                        if (!shipFound && ship.SpawningBullet)
                        {
                            shipFound = true;
                            ship.SpawningBullet = false;
                            isAllyShip = ship.IsAlly;
                            bulletPosition = shipTranslation.Value + ship.BulletSpawnOffset;
                        }
                    });

                    if (shipFound)
                    {
                        bulletTransformPosition.Value = bulletPosition;
                        if (!isAllyShip)
                            bullet.Velocity.x *= -1f;
                    }
                    else
                    {
                        entitiesToUnload.Add(bulletEntity);
                    }
                }
            });

            // Unload bullets that did not have a ship to spawn in front of (the ship was probably deleted)
            // Unload scenes outside the ForEach since structural changes cannot be made inside a ForEach
            for (int i = 0; i < entitiesToUnload.Length; i++)
                SceneService.UnloadSceneInstance(entitiesToUnload[i]);
            entitiesToUnload.Dispose();
        }
    }
}
