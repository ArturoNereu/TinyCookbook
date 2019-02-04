
namespace game {

    /** New System */
    export class HelicopterSystem extends ut.ComponentSystem {
        
        OnUpdate(): void
        {
            this.world.forEach([ut.HitBox2D.HitBoxOverlapResults, game.Helicopter, game.Movement], (hitBoxOverlapResults, helicopter, movement) => {

                //TODO: Cache the reference

                if (hitBoxOverlapResults.overlaps.length > 0)
                {
                    movement.isMoving = false;

                    //sprite2dSequencePlayer.sequence = this.world.getEntityByName("HelicopterDestroyed");
                }
            });
        }
    }
}
