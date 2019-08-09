using Unity.Collections;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Scenes;

namespace SpawnAndDestroy
{
    public class DestroyAfterDelaySystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var deltaTime = World.TinyEnvironment().frameDeltaTime;
            var entitiesToUnload = new NativeList<Entity>(Allocator.Temp);

            Entities.ForEach((Entity entity, ref DestroyAfterDelay destroyAfterDelay) =>
            {
                destroyAfterDelay.Timer += deltaTime;
                if (destroyAfterDelay.Timer > destroyAfterDelay.DestroyDelay)
                {
                    entitiesToUnload.Add(entity);
                }
            });

            // Unload scenes outside the ForEach since structural changes cannot be made inside a ForEach
            for (int i = 0; i < entitiesToUnload.Length; i++)
                SceneService.UnloadSceneInstance(entitiesToUnload[i]);
            entitiesToUnload.Dispose();
        }
    }
}
