
namespace game 
{
    /** New System */
    export class PlayerCollisionSystem extends ut.ComponentSystem 
	{  
		static explosionGroupName : string = "game.ExplosionGroup";

        OnUpdate():void 
		{
			let isGameOver = false;

			this.world.forEach([ut.Entity, ut.Core2D.TransformLocalPosition, ut.HitBox2D.HitBoxOverlapResults, game.PlayerTag], (entity, position, contacts, tag) => 
			{
				let explosion = ut.EntityGroup.instantiate(this.world, game.PlayerCollisionSystem.explosionGroupName)[0];

				this.world.usingComponentData(explosion, [ut.Core2D.TransformLocalPosition], (explosionPos) => 
				{
                    explosionPos.position = position.position;
                });
				
				this.world.destroyEntity(entity);
				
				isGameOver = true;
				
			});

			if(isGameOver)
				game.GameService.restart(this.world);
        }
    }
}
