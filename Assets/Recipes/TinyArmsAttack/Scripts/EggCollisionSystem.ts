
namespace game 
{
    export class EggCollisionSystem extends ut.ComponentSystem 
    {    
        OnUpdate():void 
        {
            this.world.forEach([ut.Entity, ut.HitBox2D.HitBoxOverlapResults, ut.Core2D.TransformLocalPosition, game.Egg], (entity, hitBoxOverlapResults, transformLocalPosition, egg) =>
            {
                for(let i = 0; i < hitBoxOverlapResults.overlaps.length; i++)
                {
                    let otherEntity = hitBoxOverlapResults.overlaps[0].otherEntity;

                    if(this.world.exists(otherEntity) && this.world.hasComponent(otherEntity, game.Helicopter))
                    {
                        game.GameService.restart(this.world);
                    }
                }
            });
        }
    }
}
