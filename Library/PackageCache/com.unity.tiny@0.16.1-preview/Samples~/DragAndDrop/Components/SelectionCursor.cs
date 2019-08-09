using Unity.Authoring.Core;
using Unity.Entities;

namespace DragAndDrop
{
    /// <summary>
    /// Add this component to an entity with a sprite to make it act as a virtual cursor
    /// that can be used to select and drag and drop objects with the keyboard by the
    /// VirtualCursorSelectSystem and VirtualCursorDragSystem.
    /// </summary>
    public struct SelectionCursor : IComponentData
    {
        public Entity SelectedEntity;
        public bool IsVisible;
        [HideInInspector]
        public float ScaleAnimationTimer;
        public bool IsLocked;
    }
}
