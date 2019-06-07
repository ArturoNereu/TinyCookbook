using Unity.Entities;
using Unity.Tiny.Watchers;

namespace Unity.Tiny.UIControls
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PointerInteractionSystem))]
    public class UIControlsWatchersSystem : WatchersSystem
    {
    }
}
