using Unity.Entities;

namespace Unity.Tiny.UIControls
{
    /// <summary>
    ///  Updates the appearance of the toggle control based on the pointer
    ///  interaction (pointer over, pointer down, etc).
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(ToggleCheckedSystem))]
    public class ToggleSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Collections.Allocator.Temp);

            Entities.
                WithAll<InactiveUIControl>().
                ForEach((Entity entity, ref Toggle toggle) =>
                {
                    var rendererEntity = UIControlsCommon.GetSprite2DRendererEntity(EntityManager, toggle.sprite2DRenderer, entity);
                    if (rendererEntity == Entity.Null)
                        return;

                    UIControlsCommon.ApplyTransition(EntityManager, ecb, rendererEntity,
                        toggle.isOn ? toggle.transitionChecked : toggle.transition, TransitionType.Disabled);
                });

            Entities.
                WithNone<InactiveUIControl>().
                ForEach((Entity entity, ref Toggle toggle, ref PointerInteraction interaction) =>
                {
                    var rendererEntity = UIControlsCommon.GetSprite2DRendererEntity(EntityManager, toggle.sprite2DRenderer, entity);
                    if (rendererEntity == Entity.Null)
                        return;

                    UIControlsCommon.ApplyTransition(EntityManager, ecb, rendererEntity, toggle.isOn ? toggle.transitionChecked : toggle.transition,
                        UIControlsCommon.GetTransitionType(interaction));
                });

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
