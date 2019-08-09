using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.UIControls;

namespace TimerButton
{
    /// <summary>
    /// Starts a timer on the press of a button.
    /// To use, add the StartTimerButton component to any entity that has a RectTransform.
    /// </summary>
    public class StartTimerButtonSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref StartTimerButton startButton, ref PointerInteraction pointerInteraction) =>
            {
                if (pointerInteraction.clicked)
                {
                    // Do your button actions here:
                    OnStartTimer(startButton.TimerEntity);
                }
            });
        }

        void OnStartTimer(Entity timerEntity)
        {
            var env = World.TinyEnvironment();

            // Start a timer
            var timer = EntityManager.GetComponentData<Timer>(timerEntity);
            timer.RemainingTime = 5f;
            EntityManager.SetComponentData(timerEntity, timer);

            // Start sprite sequence animation
            var batteryWarningEntity = Entity.Null;
            Entities.WithAll<NoBatteryWarningTag>().ForEach((Entity entity) => { batteryWarningEntity = entity; });
            var spriteSequencePlayer = EntityManager.GetComponentData<Sprite2DSequencePlayer>(batteryWarningEntity);
            spriteSequencePlayer.time = 0f;
            spriteSequencePlayer.paused = false;
            EntityManager.SetComponentData(batteryWarningEntity, spriteSequencePlayer);

            // Open light
            var lightEntity = Entity.Null;
            Entities.WithAll<PowerLightTag>().ForEach((Entity entity) => { lightEntity = entity; });
            var lightSpriteRenderer = EntityManager.GetComponentData<Sprite2DRenderer>(lightEntity);
            lightSpriteRenderer.color.a = 1f;
            EntityManager.SetComponentData(lightEntity, lightSpriteRenderer);
        }
    }
}
