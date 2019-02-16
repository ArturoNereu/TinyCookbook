
namespace game 
{
    export class DestroyAfterTimeSystem extends ut.ComponentSystem 
    {
        OnUpdate():void 
        {
            let dt = this.scheduler.deltaTime();

            this.world.forEach([ut.Entity, game.DestroyedAfterSeconds], (entity, destroyAfterSeconds) => 
            {
                destroyAfterSeconds.time += dt;

                if(destroyAfterSeconds.time >= destroyAfterSeconds.ttl)
                {
                    // This function call is slow, be mindful when using it
                    ut.Core2D.TransformService.destroyTree(this.world, entity, true);
                }
            }); 
        }
    }
}
