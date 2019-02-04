
namespace game {

    /** New System */
    export class HelicopterSystem extends ut.ComponentSystem {
        
        OnUpdate(): void
        {
            this.world.forEach([ut.Entity, ut.HitBox2D.HitBoxOverlapResults, ut.Core2D.TransformLocalPosition, game.Helicopter, game.Movement], (entity, hitBoxOverlapResults, transformLocalPositon, helicopter, movement) => {

                //TODO: Cache the reference

                //if (hitBoxOverlapResults.overlaps.length > 0)
                //{

                let dinosaurEntity = this.world.getEntityByName("Dinosaur");
                let dinosaur = this.world.getComponentData(dinosaurEntity, game.Dinosaur);

                let markToBeDestroyed = false;

                if (dinosaur.dinosaurState == DinosaurStates.attacking)
                {
                    movement.isMoving = false;
                    console.log("Died Helicopter");
                    //sprite2dSequencePlayer.sequence = this.world.getEntityByName("HelicopterDestroyed");

                    let explosion = ut.EntityGroup.instantiate(this.world, "game.Explosion")[0];

                    this.world.usingComponentData(explosion, [ut.Core2D.TransformLocalPosition], (explosionPos) => {
                        explosionPos.position = transformLocalPositon.position;
                    });

                    //ut.Core2D.TransformService.destroyTree(this.world, entity);
                }
            });
        }
    }
}
