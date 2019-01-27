
namespace game {

    export class UpdateTutorialPointerSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game) {
                return;
            }

            let tutorialHighlightEntity = this.world.getEntityByName("TutorialHighlight");
            let isTutorialHighlightVisible = this.world.exists(tutorialHighlightEntity) && this.world.getComponentData(tutorialHighlightEntity, game.TutorialHighlight).StartDelay <= 0;
            
            let deltaTime = this.scheduler.deltaTime();
            this.world.forEach([game.TutorialPointer],
                (tutorialPointer) => {

                    if (isTutorialHighlightVisible) {
                        tutorialPointer.Timer += deltaTime;
                    }
                    
                    // Move animation
                    let progress = (tutorialPointer.Timer % tutorialPointer.Duration) / tutorialPointer.Duration;
                    let moveProgress = ut.Interpolation.InterpolationService.evaluateCurveFloat(this.world, progress, tutorialPointer.MoveCurve);
                    let pointerEntity = tutorialPointer.Pointer;
                    let pointerTransform = this.world.getComponentData(pointerEntity, ut.Core2D.TransformLocalPosition);
                    if (tutorialPointer.StretchToMove && this.world.hasComponent(pointerEntity, ut.Core2D.Sprite2DRendererOptions)) {
                        pointerTransform.position.x = tutorialPointer.StartPosition.x;
                        pointerTransform.position.y = tutorialPointer.StartPosition.y;
                        let spriteRendererOptions = this.world.getComponentData(pointerEntity, ut.Core2D.Sprite2DRendererOptions);
                        spriteRendererOptions.size.y = Math.abs(moveProgress * (tutorialPointer.EndPosition.y - tutorialPointer.StartPosition.y));
                        this.world.setComponentData(pointerEntity, spriteRendererOptions);
                    }
                    else {
                        pointerTransform.position.x = tutorialPointer.StartPosition.x + moveProgress * (tutorialPointer.EndPosition.x - tutorialPointer.StartPosition.x);
                        pointerTransform.position.y = tutorialPointer.StartPosition.y + moveProgress * (tutorialPointer.EndPosition.y - tutorialPointer.StartPosition.y);
                    }
                    this.world.setComponentData(pointerEntity, pointerTransform);


                    // Fade animation
                    let fadeProgress = ut.Interpolation.InterpolationService.evaluateCurveFloat(this.world, progress, tutorialPointer.AlphaCurve);
                    let pointerSpriteRenderer = this.world.getComponentData(pointerEntity, ut.Core2D.Sprite2DRenderer);
                    pointerSpriteRenderer.color = new ut.Core2D.Color(1, 1, 1, fadeProgress);
                    this.world.setComponentData(pointerEntity, pointerSpriteRenderer);
                });
        }
    }
}
