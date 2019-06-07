using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Scenes;

namespace SpawnAndDestroy
{
    /// <summary>
    /// Update the attack countdown timer of all ships.
    /// When the timer reaches zero, spawn a bullet and reset the attack timer.
    /// </summary>
    public class SpawnBulletSystem : ComponentSystem
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
            var isAttacking = env.GetConfigData<AttackConfiguration>().IsAttacking;

            var deltaTime = env.frameDeltaTime;

            // Update ship attack timer and count the total number of bullets to spawn
            var newBulletCount = 0;
            SceneReference bulletSceneToSpawn = new SceneReference();
            Entities.ForEach((Entity entity, ref Ship ship) =>
            {
                if (!ship.Initialized)
                    return;

                if (isAttacking)
                {
                    ship.FireTimer -= deltaTime;
                    if (ship.FireTimer <= 0f)
                    {
                        bulletSceneToSpawn = ship.Bullet;
                        newBulletCount++;
                        ship.FireTimer = _random.NextFloat(ship.MinFireCooldown, ship.MaxFireCooldown);
                        ship.SpawningBullet = true;
                    }
                }
                else
                {
                    ship.SpawningBullet = false;
                }
            });

            // Spawn all the bullets needed
            if (newBulletCount > 0)
            {
                for (int i = 0; i < newBulletCount; i++)
                {
                    SceneService.LoadSceneAsync(bulletSceneToSpawn);
                }
            }
        }
    }
}
