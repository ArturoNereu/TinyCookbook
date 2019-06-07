using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Text;

namespace SpawnAndDestroy
{
    /// <summary>
    /// Count the number of ships, bullets and entities and update the UI labels
    /// </summary>
    public class DisplayStatsSystem : ComponentSystem
    {
        float _lastTimeUpdatedFPS;

        protected override void OnUpdate()
        {
            var env = World.TinyEnvironment();

            var shipCount = 0;
            var bulletCount = 0;
            var entityCount = 0;

            Entities.WithAll<Ship>().ForEach((Entity entity) => { shipCount++; });
            Entities.WithAll<Bullet>().ForEach((Entity entity) => { bulletCount++; });
            Entities.ForEach((Entity entity) => { entityCount++; });

            Entities.WithAll<LabelShipCount>().ForEach((Entity entity) =>
            {
                EntityManager.SetBufferFromString<TextString>(entity, shipCount.ToString() + " ships");
            });

            Entities.WithAll<LabelBulletCount>().ForEach((Entity entity) =>
            {
                EntityManager.SetBufferFromString<TextString>(entity, bulletCount.ToString() + " bullets");
            });

            Entities.WithAll<LabelEntityCount>().ForEach((Entity entity) =>
            {
                EntityManager.SetBufferFromString<TextString>(entity, entityCount.ToString() + " entities");
            });

            // Refresh the frame per second display label
            if (env.frameTime - _lastTimeUpdatedFPS > 0.25f)
            {
                _lastTimeUpdatedFPS = (float)env.frameTime;

                Entities.WithAll<LabelFrameRate>().ForEach((Entity entity) =>
                {
                    var frameRate = (int)math.round(1f / env.frameDeltaTime);
                    EntityManager.SetBufferFromString<TextString>(entity, frameRate.ToString() + " FPS");
                });
            }
        }
    }
}
