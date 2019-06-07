using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Audio;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace DragAndDrop
{
    /// <summary>
    /// Check if all puzzle pieces are connected and trigger success state
    /// </summary>
    public class PuzzleValidationSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var env = World.TinyEnvironment();
            var puzzleConfig = env.GetConfigData<PuzzleConfiguration>();

            if (puzzleConfig.IsCompleted)
            {
                return;
            }

            var connectedPieceCount = 0;
            Entities.ForEach((Entity entity, ref PuzzlePiece puzzlePiece) =>
            {
                var isPieceValid = (IsConnected(entity, puzzlePiece, -1, 0) || puzzlePiece.Coords.x == 0) &&
                    (IsConnected(entity, puzzlePiece, 1, 0) || puzzlePiece.Coords.x == puzzleConfig.WidthPieceCount - 1) &&
                    (IsConnected(entity, puzzlePiece, 0, 1) || puzzlePiece.Coords.y == puzzleConfig.HeightPieceCount - 1) &&
                    (IsConnected(entity, puzzlePiece, 0, -1) || puzzlePiece.Coords.y == 0);

                if (isPieceValid)
                    connectedPieceCount++;
            });

            if (connectedPieceCount == puzzleConfig.WidthPieceCount * puzzleConfig.HeightPieceCount)
            {
                Entities.ForEach((Entity entity, ref PuzzlePiece puzzlePiece, ref Draggable draggable) =>
                {
                    draggable.IsLocked = true;
                });

                puzzleConfig.IsCompleted = true;
                env.SetConfigData(puzzleConfig);

                // Show replay button
                var buttonReplayEntity = Entity.Null;
                var query = EntityManager.CreateEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[] { typeof(ButtonReplay) },
                    Options = EntityQueryOptions.IncludeDisabled
                });

                var buttonArray = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                foreach ( var entity in buttonArray)
                {
                    buttonReplayEntity = entity;
                }
                query.Dispose();
                buttonArray.Dispose();
                EntityManager.RemoveComponent<Disabled>(buttonReplayEntity);

                // TODO: re-integrate this when audio system works
                //var successSoundEntity = Entity.Null;
                //Entities.WithAll<SuccessAudioTag>().ForEach((Entity entity) => { successSoundEntity = entity; });
                //EntityManager.AddComponentData(successSoundEntity, new AudioSourceStart());

                var selectionCursorEntity = Entity.Null;
                Entities.WithAll<SelectionCursor>().ForEach((Entity entity) => { selectionCursorEntity = entity; });
                if (EntityManager.Exists(selectionCursorEntity))
                {
                    var selectionCursor = EntityManager.GetComponentData<SelectionCursor>(selectionCursorEntity);
                    selectionCursor.IsLocked = true;
                    selectionCursor.IsVisible = false;
                    EntityManager.SetComponentData(selectionCursorEntity, selectionCursor);
                }
            }
        }

        bool IsConnected(Entity entity, PuzzlePiece puzzlePiece, int xCoordsOffset, int yCoordsOffset)
        {
            bool isConnected = false;
            Entities.ForEach((Entity otherEntity, ref PuzzlePiece otherPuzzlePiece) =>
            {
                if (otherPuzzlePiece.Coords.x == puzzlePiece.Coords.x + xCoordsOffset && otherPuzzlePiece.Coords.y == puzzlePiece.Coords.y + yCoordsOffset)
                {
                    var piece1Position = EntityManager.GetComponentData<Translation>(entity);
                    var piece2Position = EntityManager.GetComponentData<Translation>(otherEntity);
                    var draggable1 = EntityManager.GetComponentData<Draggable>(entity);
                    var draggable2 = EntityManager.GetComponentData<Draggable>(otherEntity);

                    var piece1HalfWidth = draggable1.Size.x / 2f;
                    var piece1HalfHeight = draggable1.Size.x / 2f;
                    var piece2HalfWidth = draggable2.Size.x / 2f;
                    var piece2HalfHeight = draggable2.Size.x / 2f;

                    isConnected = math.abs(piece1Position.Value.x + xCoordsOffset * (piece1HalfWidth + piece2HalfWidth) - piece2Position.Value.x) < 0.001f &&
                        math.abs(piece1Position.Value.y + yCoordsOffset * (piece1HalfHeight + piece2HalfHeight) - piece2Position.Value.y) < 0.001f;
                }
            });

            return isConnected;
        }
    }
}
