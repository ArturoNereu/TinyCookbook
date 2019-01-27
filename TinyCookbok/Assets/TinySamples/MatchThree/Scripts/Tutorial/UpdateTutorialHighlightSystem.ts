
namespace game {

    export class UpdateTutorialHighlightSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game) {
                return;
            }

            let deltaTime = this.scheduler.deltaTime();
            this.world.forEach([ut.Entity, game.TutorialHighlight],
                (entity, tutorialHighlight) => {

                    let progress = Math.min(1, tutorialHighlight.Timer / tutorialHighlight.FadeDuration);
                    let alpha = progress;
                    let labelFloatAnimationOffset = Math.sin(tutorialHighlight.Timer) * 3.5;

                    // Fade out.
                    if (tutorialHighlight.IsClosing) {
                        alpha = 1 - alpha;
                        labelFloatAnimationOffset = 0;
                        if (progress == 1) {
                            ut.Core2D.TransformService.destroyTree(this.world, entity, true);
                            this.world.forEach([ut.Entity, game.TutorialPointer],
                                (pointerEntity, tutorialPointer) => {
                                    ut.Core2D.TransformService.destroyTree(this.world, pointerEntity, true);
                                });
                            return;
                        }
                    }

                    // Set opacity.
                    let color = new ut.Core2D.Color(0, 0, 0, alpha * tutorialHighlight.MaxAlpha);
                    tutorialHighlight.SpriteRenderers.forEach(spriteRendererEntity => {
                        let spriteRenderer = this.world.getComponentData(spriteRendererEntity, ut.Core2D.Sprite2DRenderer);
                        spriteRenderer.color = color;
                        this.world.setComponentData(spriteRendererEntity, spriteRenderer);
                    });

                    // Set text opacity.
                    let labelInstructions = this.world.getComponentData(tutorialHighlight.LabelInstructions, ut.Text.Text2DStyle);
                    labelInstructions.color = new ut.Core2D.Color(1, 1, 1, alpha * alpha * tutorialHighlight.LabelMaxAlpha);
                    this.world.setComponentData(tutorialHighlight.LabelInstructions, labelInstructions);

                    // Animate text pulsing position animation.
                    if (labelFloatAnimationOffset != 0) {
                        let labelInstructionsRectTransform = this.world.getComponentData(tutorialHighlight.LabelInstructions, ut.UILayout.RectTransform);
                        labelInstructionsRectTransform.anchoredPosition.y = tutorialHighlight.LabelDefaultPositionY + labelFloatAnimationOffset;
                        this.world.setComponentData(tutorialHighlight.LabelInstructions, labelInstructionsRectTransform);
                    }

                    if (tutorialHighlight.StartDelay > 0) {
                        tutorialHighlight.StartDelay -= deltaTime;
                    }
                    else {
                        tutorialHighlight.Timer += deltaTime;
                    }
                });
        }
    }
}
