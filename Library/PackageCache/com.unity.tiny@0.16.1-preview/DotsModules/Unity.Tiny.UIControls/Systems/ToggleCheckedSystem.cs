using Unity.Entities;

namespace Unity.Tiny.UIControls
{
    /// <summary>
    ///  Changes the value of Toggle.isOn from true to false and
    ///  from false to true every time the Toggle control is clicked.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PointerInteractionSystem))]
    public class ToggleCheckedSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.
                WithNone<InactiveUIControl>().
                ForEach((Entity entity, ref Toggle toggle, ref PointerInteraction interaction) =>
                {
                    if (interaction.clicked)
                        toggle.isOn = !toggle.isOn;
                });
        }
    }
}
