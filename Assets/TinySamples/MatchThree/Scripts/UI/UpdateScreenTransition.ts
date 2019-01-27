
namespace game {

    /**
     * Update the screen transition animation played between game state changes.
     */
    @ut.executeAfter(ut.Shared.UserCodeEnd)
    @ut.executeBefore(ut.Shared.RenderingFence)
    export class UpdateScreenTransition extends ut.ComponentSystem {
        
        OnUpdate():void {

            let deltaTime = this.scheduler.deltaTime();
            let entitiesToDestroy: ut.Entity[] = [];
            this.world.forEach([ut.Entity, game.ScreenTransition], (entity, screenTransition) => {

                    if (!screenTransition.IsTransitionIn && screenTransition.Timer >= screenTransition.OutDuration) {
                        screenTransition.Timer = 0;
                        screenTransition.IsTransitionIn = true;

                        GameStateLoadingService.setGameState(this.world, screenTransition.TransitionToState);
                    }

                    screenTransition.Timer += deltaTime;

                    if (screenTransition.IsTransitionIn && screenTransition.SkipFrameCount < 6) {
                        screenTransition.Timer = 0;
                        screenTransition.SkipFrameCount++;
                    }

                    let duration = screenTransition.IsTransitionIn ? screenTransition.InDuration : screenTransition.OutDuration;
                    let progress = Math.min(1, Math.max(0,screenTransition.Timer / duration));

                    if (screenTransition.IsTransitionIn) {
                        progress = 1 - progress;
                    }

                    // Destroy screen transition when it's done.
                    if (screenTransition.IsTransitionIn && screenTransition.Timer >= screenTransition.InDuration) {
                        let entityToDestroy = new ut.Entity();
                        entityToDestroy.version = entity.version;
                        entityToDestroy.index = entity.index;
                        entitiesToDestroy.push(entityToDestroy);
                    }

                    if (screenTransition.IsScaleHoleTransition) {
                        // Update scaled hole transition animation.
                        let scaleHoleRectTransform = this.world.getComponentData(screenTransition.ScaleHole, ut.UILayout.RectTransform);

                        let size = 2000 * (1 - ut.Interpolation.InterpolationService.evaluateCurveFloat(this.world, progress, screenTransition.ScaleHoleCurve));
                        scaleHoleRectTransform.sizeDelta = new Vector2(size, size);
                        this.world.setComponentData(screenTransition.ScaleHole, scaleHoleRectTransform);
                    }
                    else {
                        // Update fade transition animation.
                        let curtainSpriteRenderer = this.world.getComponentData(screenTransition.BlackCurtain, ut.Core2D.Sprite2DRenderer);
                        curtainSpriteRenderer.color.a = progress;
                        this.world.setComponentData(screenTransition.BlackCurtain, curtainSpriteRenderer);
                    }
                });

            // TODO: Replace destroyAllEntityGroups by foreach loop on entitiesToDestroy.
            if (entitiesToDestroy.length > 0) {
                ut.EntityGroup.destroyAll(this.world, "game.ScreenTransition");
            }
        }
    }
}
