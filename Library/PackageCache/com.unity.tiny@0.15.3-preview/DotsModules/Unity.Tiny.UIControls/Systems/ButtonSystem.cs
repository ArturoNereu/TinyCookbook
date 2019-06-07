using Unity.Entities;

namespace Unity.Tiny.UIControls
{
    public enum TransitionType { Normal, Hover, Pressed, Disabled };

    /// <summary>
    /// Updates the appearance of the button based on the pointer interaction
    /// (pointer over, pointer down, etc).
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PointerInteractionSystem))]
    public class ButtonSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Collections.Allocator.Temp);

            Entities.
                WithAll<InactiveUIControl>().
                ForEach((Entity entity, ref Button button) =>
            {
                var rendererEntity = UIControlsCommon.GetSprite2DRendererEntity(EntityManager, button.sprite2DRenderer, entity);
                if (rendererEntity == Entity.Null)
                    return;

                UIControlsCommon.ApplyTransition(EntityManager, ecb, rendererEntity, button.transition, TransitionType.Disabled);
            });

            Entities.
                WithNone<InactiveUIControl>().
                ForEach((Entity entity, ref Button button, ref PointerInteraction interaction) =>
            {
                var rendererEntity = UIControlsCommon.GetSprite2DRendererEntity(EntityManager, button.sprite2DRenderer, entity);
                if (rendererEntity == Entity.Null)
                    return;

                UIControlsCommon.ApplyTransition(EntityManager, ecb, rendererEntity, button.transition,
                    UIControlsCommon.GetTransitionType(interaction));
            });

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
