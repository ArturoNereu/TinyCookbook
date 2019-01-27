
namespace game {

    /**
     * Set gems IsSwapping field to false after its swaping animation ended.
     */
    @ut.executeBefore(game.CheckMatchSystem)
    export class GemSwapTweenEndSystem extends ut.ComponentSystem {
        
        OnUpdate(): void {
            this.world.forEach([ut.Entity, ut.Tweens.TweenComponent, game.GemSwapTweenEndCallback], (tweenEntity, tween, callback) => {
                let gem = this.world.getComponentData(callback.GemEntity, game.Gem);
                if (tween.ended) {
                    if (gem.IsSwapping) {
                        gem.IsSwapping = false;
                        this.world.setComponentData(callback.GemEntity, gem);
                    }
                    ut.Core2D.TransformService.destroyTree(this.world, tweenEntity, true);
                }
            });
        }
    }
}
