
namespace game {

    export class UpdateEggTutorialSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game ||
                !this.world.hasComponent(GameService.getCurrentLevelEntity(this.world), game.LevelEggObjective) ||
                TutorialService.getIsEggTutorialDone(this.world)) {
                return;
            }

            let tutorialHighlightEntity = this.world.getEntityByName("TutorialHighlight");
            if (!this.world.exists(tutorialHighlightEntity) && GameService.getGameState(this.world).CurrentScore == 0) {

                // Find gem positions in the bottom row.
                let grid = game.GridService.getGridConfiguration(this.world);
                let gemTransformPositions: Vector2[] = [];
                this.world.forEach([game.Gem],
                    (gem) => {
                        let gemPosition = GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
                        if (gemPosition.y == 0) {
                            let gemWorldPosition = GemService.getGemWorldPosition(grid, gem);
                            gemTransformPositions.push(new Vector2(gemWorldPosition.x, gemWorldPosition.y));
                        }
                    });
                
                if (gemTransformPositions.length > 0) {
                    // Highlight gems in the bottom row.
                    let newTutorialHighlightEntity = TutorialService.spawnTutorialHighlightOnGems(this.world, gemTransformPositions, 35);
                    let tutorialHighlight = this.world.getComponentData(newTutorialHighlightEntity, game.TutorialHighlight);

                    tutorialHighlight.AutoCloseDelay = 4;
                    this.world.setComponentData(newTutorialHighlightEntity, tutorialHighlight);

                    let labelInstructions = this.world.getComponentData(tutorialHighlight.LabelInstructions, ut.Text.Text2DRenderer);
                    labelInstructions.text = LocalizationService.getText(this.world, "Tutorial_Eggs");
                    this.world.setComponentData(tutorialHighlight.LabelInstructions, labelInstructions);

                    this.world.forEach([game.Gem],
                        (gem) => {
                            if (gem.GemType == game.GemTypes.Egg) {
                                // Spawn an arrow pointer that will animate from the egg to the bottom row.
                                let tutorialPointerEntity = ut.EntityGroup.instantiate(this.world, "game.TutorialEggPointer")[0];
                                let tutorialPointer = this.world.getComponentData(tutorialPointerEntity, game.TutorialPointer);
                                let startWorldPosition = GemService.getGemWorldPosition(grid, gem);
                                let endWorldPosition = game.GridService.getGridToWorldPosition(grid, game.GridService.getPositionFromCellHashCode(grid, gem.CellHashKey).x, 0);
                                tutorialPointer.StartPosition.x = startWorldPosition.x;
                                tutorialPointer.StartPosition.y = startWorldPosition.y - 12;
                                tutorialPointer.EndPosition.x = endWorldPosition.x;
                                tutorialPointer.EndPosition.y = endWorldPosition.y + 12;
                                this.world.setComponentData(tutorialPointerEntity, tutorialPointer);

                                let tutorialEggPointer = this.world.getComponentData(tutorialPointerEntity, game.TutorialEggPointer);
                                let tutorialPointerTransform = this.world.getComponentData(tutorialEggPointer.ImageEgg, ut.Core2D.TransformLocalPosition);
                                tutorialPointerTransform.position.x = startWorldPosition.x;
                                tutorialPointerTransform.position.y = startWorldPosition.y;
                                this.world.setComponentData(tutorialEggPointer.ImageEgg, tutorialPointerTransform);
                            }
                        });
                }
            }
            else if (this.world.exists(tutorialHighlightEntity)) {
                // Auto close the egg tutorial after a delay.
                let tutorialHighlight = this.world.getComponentData(tutorialHighlightEntity, game.TutorialHighlight);
                if (!tutorialHighlight.IsClosing && tutorialHighlight.Timer > tutorialHighlight.AutoCloseDelay) {
                    tutorialHighlight.IsClosing = true;
                    tutorialHighlight.Timer = 0;
                    this.world.setComponentData(tutorialHighlightEntity, tutorialHighlight);
                    TutorialService.setIsEggTutorialDone(this.world, true);
                }
            }
        }
    }
}
