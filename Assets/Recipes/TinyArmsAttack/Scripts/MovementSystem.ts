
namespace game {

    /** New System */
    export class MovementSystem extends ut.ComponentSystem {
        
        OnUpdate(): void
        {
            let dt = this.scheduler.deltaTime();

            this.world.forEach([ut.Core2D.TransformLocalPosition, game.Movement], (transformLocal, movement) => {

                let position = transformLocal.position;

                position.x += movement.speed * dt;

                console.log(movement.speed);

                transformLocal.position = position;
            });
        }
    }
}
