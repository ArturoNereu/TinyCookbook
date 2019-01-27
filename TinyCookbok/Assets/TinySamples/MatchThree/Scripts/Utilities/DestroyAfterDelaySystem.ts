
namespace game {

    /**
     * Destroy an entity after a delay of time.
     */
    export class DestroyAfterDelaySystem extends ut.ComponentSystem {
        
        OnUpdate(): void {
            
            let deltaTime = this.scheduler.deltaTime();
            this.world.forEach([ut.Entity, game.DestroyAfterDelay],
                (entity, destroyAfterDelay) => {
                    
                    let destroyDelay = destroyAfterDelay.Delay;
                    destroyDelay -= deltaTime;
                    destroyAfterDelay.Delay = destroyDelay;

                    if (destroyDelay <= 0) {
                        ut.Core2D.TransformService.destroyTree(this.world, entity, true);
                    }
                });
        }
    }
}
