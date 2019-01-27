
namespace game {

    /**
     * Animate sprite opacity to create a warning pulse animation when the user is close to the end of the game.
     */
    export class AnimateNearDeathOpacitySystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            let deltaTime = this.scheduler.deltaTime();
            this.world.forEach([ut.Entity, game.NearDeathOpacityAnimation, ut.Core2D.Sprite2DRenderer], (entity, nearDeathAnimation, spriteRenderer) => {

                let alpha = 0;
                if (GameService.isNearDeath(this.world)) {
                    nearDeathAnimation.Timer += deltaTime * nearDeathAnimation.Speed;
                    let animationRatio = (Math.sin(nearDeathAnimation.Timer) + 1) / 2;
                    alpha = nearDeathAnimation.MinAlpha + animationRatio * (nearDeathAnimation.MaxAlpha - nearDeathAnimation.MinAlpha);
                }
                spriteRenderer.color = new ut.Core2D.Color(1, 1, 1, alpha);
            });
        }
    }
}
