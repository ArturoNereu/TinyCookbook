using Unity.Authoring.Core;
using Unity.Entities;

namespace DragAndDrop
{
    public struct PuzzleConfiguration : IComponentData
    {
        public float PieceDragSnapDistance;
        public int WidthPieceCount;
        public int HeightPieceCount;
        //[HideInInspector]
        public bool IsCompleted;
    }
}
