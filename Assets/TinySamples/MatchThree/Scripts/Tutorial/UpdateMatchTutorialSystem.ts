
namespace game {

    export class UpdateMatchTutorialSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game ||
                GameService.getCurrentLevel(this.world).LevelID != 1 ||
                TutorialService.getIsMatchTutorialDone(this.world)) {
                return;
            }

            let tutorialHighlightEntity = this.world.getEntityByName("TutorialHighlight");
            if (!this.world.exists(tutorialHighlightEntity) && GameService.getGameState(this.world).CurrentScore == 0) {

                let grid = game.GridService.getGridConfiguration(this.world);
                let gemTransformPositions: Vector2[] = [];
                let gemToSwap = -1;
                let gem1ToSwap = -1;
                let gem2ToSwap = -1;
                this.world.forEach([game.Gem, game.MatchPossibility, ut.Core2D.TransformLocalPosition],
                    (gem, matchPossibility, transformPosition) => {
                        // Change hint timer to show hint highlight right away.
                        matchPossibility.HintTimer = game.GameService.getGameState(this.world).ShowHintDelay;

                        gemTransformPositions.push(new Vector2(transformPosition.position.x, transformPosition.position.y));
                        gem1ToSwap = matchPossibility.SwapGem1HashKey;
                        gem2ToSwap = matchPossibility.SwapGem2HashKey;
                        if (matchPossibility.NeedsSwap) {
                            gemToSwap = gem.CellHashKey;
                        }
                    });
                
                if (gemTransformPositions.length > 0) {
                    let gemToSwapWith = gemToSwap == gem1ToSwap ? gem2ToSwap : gem1ToSwap;

                    let newTutorialHighlightEntity = TutorialService.spawnTutorialHighlightOnGems(this.world, gemTransformPositions);
                    let tutorialHighlight = this.world.getComponentData(newTutorialHighlightEntity, game.TutorialHighlight);

                    let labelInstructions = this.world.getComponentData(tutorialHighlight.LabelInstructions, ut.Text.Text2DRenderer);
                    labelInstructions.text = LocalizationService.getText(this.world, "Tutorial_Match");
                    this.world.setComponentData(tutorialHighlight.LabelInstructions, labelInstructions);

                    let tutorialPointerEntity = ut.EntityGroup.instantiate(this.world, "game.TutorialMatchPointer")[0];
                    let tutorialPointer = this.world.getComponentData(tutorialPointerEntity, game.TutorialPointer);
                    let startPosition = GridService.getPositionFromCellHashCode(grid, gemToSwap);
                    let endPosition = GridService.getPositionFromCellHashCode(grid, gemToSwapWith);
                    let startWorldPosition = GridService.getGridToWorldPosition(grid, startPosition.x, startPosition.y);
                    let endWorldPosition = GridService.getGridToWorldPosition(grid, endPosition.x, endPosition.y);
                    tutorialPointer.StartPosition.x = startWorldPosition.x;
                    tutorialPointer.StartPosition.y = startWorldPosition.y;
                    tutorialPointer.EndPosition.x = endWorldPosition.x;
                    tutorialPointer.EndPosition.y = endWorldPosition.y;
                    this.world.setComponentData(tutorialPointerEntity, tutorialPointer);
                }
            }
            else if (this.world.exists(tutorialHighlightEntity) && GameService.getGameState(this.world).CurrentScore > 0) {
                let tutorialHighlight = this.world.getComponentData(tutorialHighlightEntity, game.TutorialHighlight);
                if (!tutorialHighlight.IsClosing) {
                    tutorialHighlight.IsClosing = true;
                    tutorialHighlight.Timer = 0;
                    this.world.setComponentData(tutorialHighlightEntity, tutorialHighlight);
                    TutorialService.setIsMatchTutorialDone(this.world, true);
                }
            }
        }
    }
}
