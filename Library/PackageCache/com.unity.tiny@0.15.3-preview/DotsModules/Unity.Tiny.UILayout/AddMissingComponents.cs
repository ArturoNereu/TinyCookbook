using Unity.Entities;
using Unity.Collections;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.UILayout
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    internal class AddMissingComponents : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            Entities.
                WithNone<Sprite2DRendererOptions>().
                WithAll<RectTransform, Sprite2DRenderer>().
                ForEach(e => ecb.AddComponent(e, new Sprite2DRendererOptions()));

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
