using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Scenes;
using Unity.Tiny.UIControls;

namespace SpawnAndDestroy
{
    /// <summary>
    /// Destroy all the ships in the world when the user presses the button
    /// that has the ButtonDestroyShips tag component.
    /// </summary>
    public class DestroyAllShipsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            bool destroyAllShips = false;
            Entities.WithAll<ButtonDestroyShips>().ForEach((Entity entity, ref PointerInteraction pointerInteraction) =>
            {
                if (pointerInteraction.clicked)
                    destroyAllShips = true;
            });

            if (destroyAllShips)
            {
                var env = World.TinyEnvironment();

                var allyShips = env.GetConfigBufferData<AllyShips>().Reinterpret<SceneReference>().ToNativeArray(Unity.Collections.Allocator.Temp);
                for (int i = 0; i < allyShips.Length; i++)
                    SceneService.UnloadAllSceneInstances(allyShips[i]);
                allyShips.Dispose();

                var enemyShips = env.GetConfigBufferData<EnemyShips>().Reinterpret<SceneReference>().ToNativeArray(Unity.Collections.Allocator.Temp);
                for (int i = 0; i < enemyShips.Length; i++)
                    SceneService.UnloadAllSceneInstances(enemyShips[i]);
                enemyShips.Dispose();
            }
        }
    }
}
