using Unity.Entities;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.UILayout
{
    /// <summary>
    ///  SetSprite2DSizeSystem works on entities that have Transform, RectTransform,
    ///  Sprite2DRenderer, and Sprite2DRendererOptions components.
    ///  It automatically updates the Sprite2DRendererOptions.size property
    ///  based on the size of the RectTransform.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SetRectTransformSizeSystem))]
    public class SetSprite2DSizeSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((
                Entity e, ref Parent node, ref RectTransform rt, ref Sprite2DRenderer sprite,
                ref Sprite2DRendererOptions sprite2DRendererOptions) =>
            {
                var size = UILayoutService.GetRectTransformSizeOfEntity(this, e);
                sprite2DRendererOptions.size = size;
            });
        }
    }
}
