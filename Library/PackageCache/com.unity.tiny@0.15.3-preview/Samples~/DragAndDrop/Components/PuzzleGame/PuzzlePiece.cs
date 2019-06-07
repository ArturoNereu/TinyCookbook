using Unity.Entities;
using Unity.Mathematics;

namespace DragAndDrop
{
    public struct PuzzlePiece : IComponentData
    {
        public float2 Coords;
    }
}
