using Unity.Entities;
using Unity.Tiny;

namespace Unity.Tiny.Video
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VideoSystem))]
    public class VideoNativeSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            //throw new NotImplementedException();
        }
    }
}
