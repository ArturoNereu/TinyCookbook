
namespace game {

    /** New System */
    export class HellicopterSystem extends ut.ComponentSystem {
        
        OnUpdate(): void
        {
            this.world.forEach([ut.HitBox2D.HitBoxOverlapResults, ut.Core2D.Sprite2DSequencePlayer, game.Hellicopter, game.Movement], (hitBoxOverlapResults, sprite2dSequencePlayer, hellicopter, movement) => {

                //TODO: Cache the reference

                //if (hitBoxOverlapResults.overlaps.length > 0)
                //{
                               movement.isMoving = false;

                    sprite2dSequencePlayer.sequence = this.world.getEntityByName("HelicopterDestroyed");
                //}
            });
        }
    }
}
