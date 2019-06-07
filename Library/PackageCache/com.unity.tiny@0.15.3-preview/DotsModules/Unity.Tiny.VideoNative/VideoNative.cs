using Unity.Entities;
using Unity.Tiny;

[assembly: ModuleDescription("Unity.Tiny.VideoNative", "VideoNative module")]
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
