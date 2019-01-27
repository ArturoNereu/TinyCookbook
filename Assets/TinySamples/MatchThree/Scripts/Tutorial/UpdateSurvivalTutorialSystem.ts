
namespace game {

    export class UpdateSurvivalTutorialSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game ||
                !this.world.hasComponent(GameService.getCurrentLevelEntity(this.world), game.LevelSurvival) || 
                TutorialService.getIsSurvivalTutorialDone(this.world)) {
                return;
            }

            let tutorialHighlightEntity = this.world.getEntityByName("TutorialHighlight");
            if (!this.world.exists(tutorialHighlightEntity) && GameService.getGameState(this.world).CurrentScore == 0) {
                // Spawn tutorial highlight over the survival timer bar.
                let newTutorialHighlightEntity = TutorialService.spawnTutorialHighlight(this.world, new Vector2(0, 31.5), new Vector2(198, 30));
                let tutorialHighlight = this.world.getComponentData(newTutorialHighlightEntity, game.TutorialHighlight);

                tutorialHighlight.AutoCloseDelay = 3.5;
                this.world.setComponentData(newTutorialHighlightEntity, tutorialHighlight);

                let labelInstructions = this.world.getComponentData(tutorialHighlight.LabelInstructions, ut.Text.Text2DRenderer);
                labelInstructions.text = LocalizationService.getText(this.world, "Tutorial_Survival");
                this.world.setComponentData(tutorialHighlight.LabelInstructions, labelInstructions);

                // Animate an arrow pointer from the right to the left of the time bar to indicate that it's depleting.
                let tutorialPointerEntity = ut.EntityGroup.instantiate(this.world, "game.TutorialSurvivalPointer")[0];
                let tutorialPointer = this.world.getComponentData(tutorialPointerEntity, game.TutorialPointer);
                tutorialPointer.StartPosition.x = 88;
                tutorialPointer.StartPosition.y = -4;
                tutorialPointer.EndPosition.x = -88;
                tutorialPointer.EndPosition.y = -4;
                this.world.setComponentData(tutorialPointerEntity, tutorialPointer);
            }
            else if (this.world.exists(tutorialHighlightEntity)) {
                // Auto close the tutorial after a delay.
                let tutorialHighlight = this.world.getComponentData(tutorialHighlightEntity, game.TutorialHighlight);
                if (!tutorialHighlight.IsClosing && tutorialHighlight.Timer > tutorialHighlight.AutoCloseDelay) {
                    tutorialHighlight.IsClosing = true;
                    tutorialHighlight.Timer = 0;
                    this.world.setComponentData(tutorialHighlightEntity, tutorialHighlight);
                    TutorialService.setIsSurvivalTutorialDone(this.world, true);

                    // Give time back.
                    this.world.forEach([ut.Entity, game.LevelSurvival], (entity, levelSurvival) => {
                        levelSurvival.SurvivalTimer = levelSurvival.MaxSurvivalTime;
                    });
                }
            }
        }
    }
}
