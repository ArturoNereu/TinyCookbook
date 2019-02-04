
namespace game {

    /** New System */
    export class HelicopterSystem extends ut.ComponentSystem {
        
        OnUpdate(): void
        {
            this.world.forEach([ut.HitBox2D.HitBoxOverlapResults, game.Helicopter, game.Movement], (hitBoxOverlapResults, helicopter, movement) => {

                //TODO: Cache the reference

                //if (hitBoxOverlapResults.overlaps.length > 0)
                //{

                let dinosaurEntity = this.world.getEntityByName("Dinosaur");
                let dinosaur = this.world.getComponentData(dinosaurEntity, game.Dinosaur);

                if (dinosaur.dinosaurState == DinosaurStates.attacking)
                {
                    movement.isMoving = false;
                    console.log("Died Helicopter");
                    //sprite2dSequencePlayer.sequence = this.world.getEntityByName("HelicopterDestroyed");
                }
            });
        }
    }
}
