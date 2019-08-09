using Unity.Entities;

namespace Unity.Tiny.UIControls
{
    /// <summary>
    /// Updates internal components related to UI controls.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class UIControlsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.
                WithAll<InactiveUIControl>().
                ForEach((ref PointerInteraction interaction) =>
            {
                interaction.clicked = false;
                interaction.down = false;
            });
        }
    }
}
