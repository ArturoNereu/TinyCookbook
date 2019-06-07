using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace DragAndDrop
{
    /// <summary>
    /// Scale up transforms and change the sprite sort order when the object is being dragged
    /// </summary>
    public class DragAnimationSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var deltaTime = World.TinyEnvironment().frameDeltaTime;

            Entities.ForEach((ref Draggable draggable, ref DragAnimation dragAnimation, ref NonUniformScale transformLocalScale) =>
            {
                var isScaledUp = (draggable.InMouseDrag || draggable.InKeyboardDrag || draggable.TouchID >= 0) && !dragAnimation.IsSnapped;
                var animationIncrement = 10f * (isScaledUp ? deltaTime : -deltaTime);

                // Scale object
                dragAnimation.AnimationProgress = math.clamp(dragAnimation.AnimationProgress + animationIncrement, 0f, 1f);
                var scale = dragAnimation.DefaultScale + dragAnimation.AnimationProgress * (dragAnimation.DraggedScale - dragAnimation.DefaultScale);
                transformLocalScale.Value = new float3(scale, scale, 1f);

                // Order sprite on top of every other sprites whike dragging
                var spriteEntity = dragAnimation.SpriteRenderer;
                if (EntityManager.HasComponent<LayerSorting>(spriteEntity))
                {
                    var sortingLayer = EntityManager.GetComponentData<LayerSorting>(spriteEntity);
                    sortingLayer.order = (short)(isScaledUp ? dragAnimation.DraggedSortOrder : dragAnimation.DefaultSortOrder);
                    EntityManager.SetComponentData(spriteEntity, sortingLayer);
                }
            });
        }
    }
}
