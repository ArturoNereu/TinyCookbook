
namespace game {

    @ut.executeAfter(ut.Shared.UserCodeEnd)
    @ut.executeBefore(ut.Shared.RenderingFence)
    export class AnimateCutsceneSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Cutscene && GameService.getGameState(this.world).GameStateType != game.GameStateTypes.CutsceneEnd) {
                return;
            }
            
            let cutsceneDirectorEntity = this.world.getEntityByName("CutsceneDirector");
            if (!this.world.exists(cutsceneDirectorEntity)) {
                return;
            }

            let cutsceneDirector = this.world.getComponentData(cutsceneDirectorEntity, game.CutsceneDirector);
            let deltaTime = this.scheduler.deltaTime();

            if (cutsceneDirector.PauseDelayTimer > 0) {
                cutsceneDirector.PauseDelayTimer -= deltaTime * cutsceneDirector.Speed;
                this.world.setComponentData(cutsceneDirectorEntity, cutsceneDirector);
                return;
            }

            if (ut.Core2D.Input.getMouseButtonDown(0)) {
                this.exitCutscene();
            }

            let hasElementInProgress = false;
            let clearPreviousElements = false;
            let lastItemIndex = -1;

            // Update current cutscene element animations.
            this.world.forEach([ut.Entity, game.CutsceneElement, ut.Core2D.Sprite2DRenderer], (entity, cutsceneElement, spriteRenderer) => {

                if (cutsceneElement.Index == cutsceneDirector.CurrentElementIndex) {
                    if (cutsceneElement.ClearPreviousElements) {
                        clearPreviousElements = true;
                    }
                    if (cutsceneElement.Timer < cutsceneElement.Duration) {
                        hasElementInProgress = true;
                    }
                }

                if (cutsceneElement.Index <= cutsceneDirector.CurrentElementIndex) {
                    if (cutsceneElement.Timer == 0) {
                        this.animateCutSceneElement(cutsceneDirector, entity, cutsceneElement);
                    }
                    cutsceneElement.Timer += deltaTime * cutsceneDirector.Speed;
                }

                if (cutsceneElement.Index > lastItemIndex) {
                    lastItemIndex = cutsceneElement.Index;
                }

                spriteRenderer.color = new ut.Core2D.Color(1, 1, 1, this.getElementAlpha(cutsceneDirector, entity, cutsceneElement));
            }); 

            // If no currently animating cutscene element, move to the next one.
            let isLastElement = cutsceneDirector.CurrentElementIndex > lastItemIndex;
            if (!hasElementInProgress) {

                if (isLastElement) {
                    this.exitCutscene();
                }
                else if (clearPreviousElements) {
                    let displayInfo = this.world.getConfigData(ut.Core2D.DisplayInfo);
                    let aspectRatio = displayInfo.width / displayInfo.height;
                    let isLandscape = aspectRatio >= 16 / 9 - 0.01;
                    if (!isLandscape) {
                        this.clearPreviousElements(cutsceneDirector);
                        cutsceneDirector.PauseDelayTimer = 1.2;
                        this.world.setComponentData(cutsceneDirectorEntity, cutsceneDirector);
                    }
                }

                if (!isLastElement) {
                    cutsceneDirector.CurrentElementIndex++;
                    this.world.setComponentData(cutsceneDirectorEntity, cutsceneDirector);
                }
            }
        }

        /**
         * Start cutscene element move and scale animations.
         */
        animateCutSceneElement(cutsceneDirector: game.CutsceneDirector, cutsceneElementEntity: ut.Entity, cutsceneElement: game.CutsceneElement): void {
            
            // Move
            if (this.world.hasComponent(cutsceneElementEntity, game.CutsceneMove)) {
                let moveAnimation = this.world.getComponentData(cutsceneElementEntity, game.CutsceneMove);
                
                let transformPosition = this.world.getComponentData(cutsceneElementEntity, ut.Core2D.TransformLocalPosition);
                let defaultPosition = transformPosition.position;
                let startPosition = new Vector3(defaultPosition.x + moveAnimation.StartOffset.x, defaultPosition.y + moveAnimation.StartOffset.y, 0);
                let endPosition = defaultPosition;

                let moveTween = new ut.Tweens.TweenDesc;
                moveTween.cid = ut.Core2D.TransformLocalPosition.cid;
                moveTween.offset = 0;
                moveTween.duration = moveAnimation.Duration * (1 / cutsceneDirector.Speed);
                moveTween.func = moveAnimation.Easing;
                moveTween.loop = ut.Core2D.LoopMode.Once;
                moveTween.destroyWhenDone = true;
                moveTween.t = 0.0;

                ut.Tweens.TweenService.addTweenVector3(
                    this.world,
                    cutsceneElementEntity,
                    startPosition,
                    endPosition,
                    moveTween);

                transformPosition.position = startPosition;
                this.world.setComponentData(cutsceneElementEntity, transformPosition);
            }

            // Scale
            if (this.world.hasComponent(cutsceneElementEntity, game.CutsceneScale)) {
                let scaleAnimation = this.world.getComponentData(cutsceneElementEntity, game.CutsceneScale);

                let defaultScale = this.world.getComponentData(cutsceneElementEntity, ut.Core2D.TransformLocalScale).scale;
                let startScale = new Vector3(scaleAnimation.StartScale.x, scaleAnimation.StartScale.y, scaleAnimation.StartScale.z);
                let endScale = defaultScale;

                let scaleTween = new ut.Tweens.TweenDesc;
                scaleTween.cid = ut.Core2D.TransformLocalScale.cid;
                scaleTween.offset = 0;
                scaleTween.duration = scaleAnimation.Duration * (1 / cutsceneDirector.Speed);
                scaleTween.func = scaleAnimation.Easing;
                scaleTween.loop = ut.Core2D.LoopMode.Once;
                scaleTween.destroyWhenDone = true;
                scaleTween.t = 0.0;

                ut.Tweens.TweenService.addTweenVector3(
                    this.world,
                    cutsceneElementEntity,
                    startScale,
                    endScale,
                    scaleTween);
            }
        }

        getElementAlpha(cutsceneDirector: game.CutsceneDirector, cutsceneElementEntity: ut.Entity, cutsceneElement: game.CutsceneElement): number {
            if (cutsceneElement.Index <= cutsceneDirector.CurrentElementIndex) {
                if (this.world.hasComponent(cutsceneElementEntity, game.CutsceneFade)) {
                    let fadeAnimation = this.world.getComponentData(cutsceneElementEntity, game.CutsceneFade);
                    return Math.min(1, cutsceneElement.Timer / fadeAnimation.Duration);
                }
                else {
                    return 1;
                }
            }
            else {
                return 0;
            }
        }

        /**
         * Animate all visible cutscene elements by moving them off screen.
         */
        clearPreviousElements(cutsceneDirector: game.CutsceneDirector): void {
            this.world.forEach([ut.Entity, game.CutsceneElement, ut.Core2D.Sprite2DRenderer], (entity, cutsceneElement, spriteRenderer) => {

                if (cutsceneElement.Index <= cutsceneDirector.CurrentElementIndex) {
                    let defaultPosition = this.world.getComponentData(entity, ut.Core2D.TransformLocalPosition).position;
                    let startPosition = defaultPosition;
                    let endPosition = new Vector3(defaultPosition.x - 300, defaultPosition.y, 0);

                    let moveTween = new ut.Tweens.TweenDesc;
                    moveTween.cid = ut.Core2D.TransformLocalPosition.cid;
                    moveTween.offset = 0;
                    moveTween.duration = 1.5 * (1 / cutsceneDirector.Speed);
                    moveTween.func = ut.Tweens.TweenFunc.InCubic;
                    moveTween.loop = ut.Core2D.LoopMode.Once;
                    moveTween.destroyWhenDone = true;
                    moveTween.t = 0.0;

                    ut.Tweens.TweenService.addTweenVector3(
                        this.world,
                        entity,
                        startPosition,
                        endPosition,
                        moveTween);
                    }
            }); 
        }
        
        /**
         * Skip or end the cutscene and return to previous menu.
         */
        exitCutscene(): void {
            if (GameService.getGameState(this.world).GameStateType == game.GameStateTypes.Cutscene) {
                GameStateLoadingService.transitionToGameStateWithDuration(this.world, game.GameStateTypes.MainMenu, 1, 0.25);
            }
            else {
                GameStateLoadingService.transitionToGameStateWithDuration(this.world, game.GameStateTypes.WorldMap, 1, 0.25);
            }
        }
    }
}
