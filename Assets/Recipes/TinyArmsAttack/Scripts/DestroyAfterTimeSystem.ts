
namespace game {

    /** New System */
    export class DestroyAfterTimeSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            this.world.forEach([ut.Entity, game.DestroyedAfterSeconds], (entity, destroyAfterSeconds) => 
            {
                destroyAfterSeconds.time += this.scheduler.deltaTime();

                if(destroyAfterSeconds.time >= destroyAfterSeconds.ttl)
                {
                    //TODO: Remove the tree destroy as is slow
                    ut.Core2D.TransformService.destroyTree(this.world, entity, true);
                }
            }); 
        }
    }
}
