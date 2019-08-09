using Unity.Authoring.Core;
using Unity.Entities;
using Unity.Mathematics;

namespace DragAndDrop
{
    /// <summary>
    /// Add this component on an entity to make it draggable by the drag systems.
    /// </summary>
    public struct Draggable : IComponentData
    {
        [HideInInspector]
        public float3 DragOffset;
        [HideInInspector]
        public bool InMouseDrag;
        [HideInInspector]
        public bool InKeyboardDrag;
        [HideInInspector]
        public int TouchID;
        public bool IsLocked;
        public float KeyboardDragMoveSpeed;
        [HideInInspector]
        public float3 DragStartPosition;
        public float2 Size;

        public static Draggable Default
        {
            get
            {
                var draggable = new Draggable();
                draggable.TouchID = -1;
                return draggable;
            }
        }
    }
}
