
namespace game {

    /**
     * Set gems IsFalling field to false after its falling animation ended.
     */
    @ut.executeBefore(game.CheckMatchSystem)
    export class GemFallTweenEndSystem extends ut.ComponentSystem {
        
        OnUpdate(): void {
            this.world.forEach([ut.Entity, ut.Tweens.TweenComponent, game.GemFallTweenEndCallback], (tweenEntity, tween, callback) => {
                let gem = this.world.getComponentData(callback.GemEntity, game.Gem);
                if (tween.ended) {
                    if (gem.IsFalling) {
                        gem.IsFalling = false;
                        this.world.setComponentData(callback.GemEntity, gem);
                    }
                    ut.Core2D.TransformService.destroyTree(this.world, tweenEntity, true);
                }
            });
        }
    }
}
