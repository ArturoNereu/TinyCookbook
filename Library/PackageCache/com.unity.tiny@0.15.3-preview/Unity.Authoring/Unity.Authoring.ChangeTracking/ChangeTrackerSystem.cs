#if !NET_DOTS
using Unity.Entities;

namespace Unity.Authoring.ChangeTracking
{
    /// <summary>
    /// This system is used internally by the change tracking system to update the GlobalSystemVersion
    /// </summary>
    [DisableAutoCreation]
    internal class ChangeTrackerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
        }
    }
}
#endif
