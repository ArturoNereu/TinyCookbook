using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace DragAndDrop
{
    /// <summary>
    /// Snap and connect the dragged puzzle piece to other pieces
    /// </summary>
    [UpdateAfter(typeof(MouseDragSystem))]
    [UpdateAfter(typeof(MultiTouchDragSystem))]
    [UpdateAfter(typeof(VirtualCursorDragSystem))]
    public class ConnectPuzzlePieceSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // Find piece being dragged
            var draggedPiece = Entity.Null;
            Entities.ForEach((Entity entity, ref Draggable draggable, ref DragAnimation dragAnimation, ref PuzzlePiece puzzlePiece) =>
            {
                dragAnimation.IsSnapped = false;
                if (draggable.InMouseDrag || draggable.InKeyboardDrag || draggable.TouchID >= 0)
                {
                    draggedPiece = entity;
                }
            });

            if (!EntityManager.Exists(draggedPiece))
                return;

            // Snap dragged piece to others
            var snapPosition = new float3(0f, 0f, 0f);
            var puzzleConfig = World.TinyEnvironment().GetConfigData<PuzzleConfiguration>();

            Entities.ForEach((Entity entity, ref Draggable draggable, ref PuzzlePiece puzzlePiece) =>
            {
                if (!draggable.InMouseDrag && !draggable.InKeyboardDrag)
                {
                    snapPosition = SnapPieces(puzzleConfig, draggedPiece, entity);
                    if (snapPosition.x != 0f || snapPosition.y != 0f)
                    {

                        var draggingPieceTransform = EntityManager.GetComponentData<Translation>(draggedPiece);
                        draggingPieceTransform.Value = snapPosition;
                        EntityManager.SetComponentData(draggedPiece, draggingPieceTransform);

                        var dragAnimation = EntityManager.GetComponentData<DragAnimation>(draggedPiece);
                        dragAnimation.IsSnapped = true;
                        EntityManager.SetComponentData(draggedPiece, dragAnimation);
                    }
                }
            });
        }

        /** Snap two pieces together and get the position where piece1 snaps to piece2.
        *   Returns float3.zero if pieces do not connect.
        */
        float3 SnapPieces(PuzzleConfiguration puzzleConfig, Entity piece1Entity, Entity piece2Entity)
        {
            var snapPosition = float3.zero;

            var piece1 = EntityManager.GetComponentData<PuzzlePiece>(piece1Entity);
            var piece2 = EntityManager.GetComponentData<PuzzlePiece>(piece2Entity);
            var piece1Position = EntityManager.GetComponentData<Translation>(piece1Entity);
            var piece2Position = EntityManager.GetComponentData<Translation>(piece2Entity);
            var draggable1 = EntityManager.GetComponentData<Draggable>(piece1Entity);
            var draggable2 = EntityManager.GetComponentData<Draggable>(piece2Entity);

            var piece1HalfWidth = draggable1.Size.x / 2f;
            var piece1HalfHeight = draggable1.Size.x / 2f;
            var piece1Left = new float3(piece1Position.Value.x - piece1HalfWidth, piece1Position.Value.y, 0f);
            var piece1Right = new float3(piece1Position.Value.x + piece1HalfWidth, piece1Position.Value.y, 0f);
            var piece1Up = new float3(piece1Position.Value.x, piece1Position.Value.y + piece1HalfHeight, 0f);
            var piece1Down = new float3(piece1Position.Value.x, piece1Position.Value.y - piece1HalfHeight, 0f);

            var piece2HalfWidth = draggable2.Size.x / 2f;
            var piece2HalfHeight = draggable2.Size.x / 2f;
            var piece2Left = new float3(piece2Position.Value.x - piece2HalfWidth, piece2Position.Value.y, 0f);
            var piece2Right = new float3(piece2Position.Value.x + piece2HalfWidth, piece2Position.Value.y, 0f);
            var piece2Up = new float3(piece2Position.Value.x, piece2Position.Value.y + piece2HalfHeight, 0f);
            var piece2Down = new float3(piece2Position.Value.x, piece2Position.Value.y - piece2HalfHeight, 0f);

            if (piece2.Coords.x == piece1.Coords.x + 1 && piece1.Coords.y == piece2.Coords.y && math.distance(piece1Right, piece2Left) < puzzleConfig.PieceDragSnapDistance)
            {
                snapPosition = new float3(piece2Position.Value.x - piece1HalfWidth - piece2HalfWidth, piece2Position.Value.y, 0f);
            }
            else if (piece2.Coords.x == piece1.Coords.x - 1 && piece1.Coords.y == piece2.Coords.y && math.distance(piece1Left, piece2Right) < puzzleConfig.PieceDragSnapDistance)
            {
                snapPosition = new float3(piece2Position.Value.x + piece1HalfWidth + piece2HalfWidth, piece2Position.Value.y, 0f);
            }
            else if (piece2.Coords.y == piece1.Coords.y + 1 && piece1.Coords.x == piece2.Coords.x && math.distance(piece1Up, piece2Down) < puzzleConfig.PieceDragSnapDistance)
            {
                snapPosition = new float3(piece2Position.Value.x, piece2Position.Value.y - piece1HalfHeight - piece2HalfHeight, 0f);
            }
            else if (piece2.Coords.y == piece1.Coords.y - 1 && piece1.Coords.x == piece2.Coords.x && math.distance(piece1Down, piece2Up) < puzzleConfig.PieceDragSnapDistance)
            {
                snapPosition = new float3(piece2Position.Value.x, piece2Position.Value.y + piece1HalfHeight + piece2HalfHeight, 0f);
            }

            EntityManager.SetComponentData(piece1Entity, piece1);
            EntityManager.SetComponentData(piece2Entity, piece2);

            return snapPosition;
        }
    }
}
