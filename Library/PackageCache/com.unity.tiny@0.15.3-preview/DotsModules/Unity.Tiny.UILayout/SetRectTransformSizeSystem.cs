using Unity.Entities;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.UILayout
{
    /// <summary>
    ///  SetRectTransformSizeSystem automatically updates the size of a RectTransformFinalSize
    ///  component attached to the same entity as a RectTransform component.
    ///  Required when adding a RectTransform component to the same entity as a
    ///  Text2DRenderer component.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(UILayoutSystem))]
    public class SetRectTransformSizeSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity e, ref RectTransform rect, ref RectTransformFinalSize size) =>
            {
                var s = UILayoutService.GetRectTransformSizeOfEntity(this, e);
                EntityManager.SetComponentData(e, new RectTransformFinalSize() { size = s });
            });
        }
    }
}
