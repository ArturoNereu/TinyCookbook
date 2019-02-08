
namespace game {

    /** New System */
    export class HelicopterSystem extends ut.ComponentSystem {
        
        OnUpdate(): void
        {
            this.world.forEach([ut.Entity, ut.HitBox2D.HitBoxOverlapResults, ut.Core2D.TransformLocalPosition, game.Helicopter, game.Movement], (entity, hitBoxOverlapResults, transformLocalPositon, helicopter, movement) => {

                //TODO: Cache the reference

                let dinosaurEntity = this.world.getEntityByName("Dinosaur");
                let dinosaur = this.world.getComponentData(dinosaurEntity, game.Dinosaur);

                if (dinosaur.dinosaurState == DinosaurStates.attacking)
                {
                    movement.isMoving = false;

                    let explosion = ut.EntityGroup.instantiate(this.world, "game.Explosion")[0];
                    let expPos = new ut.Core2D.TransformLocalPosition();
                    expPos = transformLocalPositon;
                    this.world.setComponentData(explosion, expPos);

                    //Destroy the hellicopter inmediatly
                    let destroyAfterDelay = new game.DestroyedAfterSeconds();
                    destroyAfterDelay.ttl = 0;
                    this.world.addComponentData(entity, destroyAfterDelay);

                    //Destroy the explosion after x seconds
                    destroyAfterDelay = new game.DestroyedAfterSeconds();
                    destroyAfterDelay.ttl = 0.5;
                    this.world.addComponentData(explosion, destroyAfterDelay);
                }
            });
        }
    }
}
