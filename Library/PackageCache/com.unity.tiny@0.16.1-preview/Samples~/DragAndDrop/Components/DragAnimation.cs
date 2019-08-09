using Unity.Authoring.Core;
using Unity.Entities;

namespace DragAndDrop
{
    /// <summary>
    /// Add this component to an entity along the Draggable component to
    /// animate the scale and change the sprite sort order of a dragged entity.
    /// </summary>
    public struct DragAnimation : IComponentData
    {
        public Entity SpriteRenderer;
        [HideInInspector]
        public float AnimationProgress;
        public float DefaultScale;
        public float DraggedScale;
        [HideInInspector]
        public bool IsSnapped;
        public int DefaultSortOrder;
        public int DraggedSortOrder;
    }
}
