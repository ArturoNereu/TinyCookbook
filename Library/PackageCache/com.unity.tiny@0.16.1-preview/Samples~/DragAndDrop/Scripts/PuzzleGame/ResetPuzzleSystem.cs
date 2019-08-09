using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Input;
using Unity.Tiny.UIControls;
using Unity.Tiny.Scenes;

#if !UNITY_WEBGL
using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
    using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

namespace DragAndDrop
{
    /// <summary>
    /// If puzzle is complete, check if user clicked the replay button to reset the game.
    /// </summary>
    public class ResetPuzzleSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var env = World.TinyEnvironment();
            var inputSystem = World.GetExistingSystem<InputSystem>();
            var puzzleConfig = env.GetConfigData<PuzzleConfiguration>();
            if (!puzzleConfig.IsCompleted)
                return;

            var buttonReplayEntity = Entity.Null;
            Entities.WithAll<ButtonReplay>().ForEach((Entity entity) => { buttonReplayEntity = entity; });
            if (!EntityManager.Exists(buttonReplayEntity))
                return;

            var buttonReplayPointerInteraction = EntityManager.GetComponentData<PointerInteraction>(buttonReplayEntity);

            if (buttonReplayPointerInteraction.clicked || inputSystem.GetKeyDown(KeyCode.Return))
            {
                // Reload the scene
                var buttonReplay = EntityManager.GetComponentData<ButtonReplay>(buttonReplayEntity);
                SceneService.UnloadAllSceneInstances(buttonReplay.SceneToLoad);
                SceneService.LoadSceneAsync(buttonReplay.SceneToLoad);

                puzzleConfig.IsCompleted = false;
                env.SetConfigData(puzzleConfig);
            }
        }
    }
}
