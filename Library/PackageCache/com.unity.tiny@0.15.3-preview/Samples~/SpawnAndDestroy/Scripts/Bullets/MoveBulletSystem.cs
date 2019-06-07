using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace SpawnAndDestroy
{
    public class MoveBulletSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var deltaTime = World.TinyEnvironment().frameDeltaTime;
            Entities.ForEach((Entity entity, ref Bullet bullet, ref Translation translation) =>
            {
                translation.Value += bullet.Velocity * deltaTime;
            });
        }
    }
}
