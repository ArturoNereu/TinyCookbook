using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Text;

namespace TimerButton
{
    /// <summary>
    /// Update a countdown timer, display it in a label and perform actions when it reaches zero.
    /// To use, add the Timer component to an entity that has a Text2DRenderer component and a TextString component.
    /// </summary>
    public class TimerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var deltaTime = (float)World.TinyEnvironment().frameDeltaTime;

            Entities.ForEach((Entity entity, ref Timer timer) =>
            {
                if (timer.RemainingTime > 0f)
                {
                    timer.RemainingTime -= deltaTime;

                    if (timer.RemainingTime <= 0f)
                    {
                        // Do your timer complete actions here:
                        OnTimerCountdownEnded();
                    }

                    // Display the remaining countdown time in a label if it has not ended
                    var displayTime = string.Empty;
                    if (timer.RemainingTime > 0f)
                        displayTime = ((int)math.ceil(timer.RemainingTime)).ToString();
                    EntityManager.SetBufferFromString<TextString>(entity, displayTime);
                }
            });
        }

        void OnTimerCountdownEnded()
        {
            var env = World.TinyEnvironment();

            // Stop battery warning sprite sequence (set current sequence frame to the second frame, the empty one)
            var batteryWarningEntity = Entity.Null;
            Entities.WithAll<NoBatteryWarningTag>().ForEach((Entity entity) => { batteryWarningEntity = entity; });
            var spriteSequencePlayer = EntityManager.GetComponentData<Sprite2DSequencePlayer>(batteryWarningEntity);
            spriteSequencePlayer.time = 1f / EntityManager.GetComponentData<Sprite2DSequenceOptions>(batteryWarningEntity).frameRate;
            spriteSequencePlayer.paused = true;
            EntityManager.SetComponentData(batteryWarningEntity, spriteSequencePlayer);

            // Close light
            var lightEntity = Entity.Null;
            Entities.WithAll<PowerLightTag>().ForEach((Entity entity) => { lightEntity = entity; });
            var lightSpriteRenderer = EntityManager.GetComponentData<Sprite2DRenderer>(lightEntity);
            lightSpriteRenderer.color.a = 0f;
            EntityManager.SetComponentData(lightEntity, lightSpriteRenderer);
        }
    }
}

