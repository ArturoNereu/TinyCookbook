
namespace game {

    @ut.executeAfter(ut.Shared.InputFence)
    @ut.executeBefore(ut.Shared.RenderingFence)
    export class CarMovementSystem extends ut.ComponentSystem
    {
        OnUpdate():void 
        {
            let dt = this.scheduler.deltaTime();
           
            this.world.forEach([ut.Core2D.TransformLocalPosition, game.Car], (localPosition, car) =>
            {
                let direction = car.direction;

                if (ut.Runtime.Input.getKey(ut.Core2D.KeyCode.LeftArrow))
                    direction.x = -1;
                else if (ut.Runtime.Input.getKey(ut.Core2D.KeyCode.RightArrow))
                    direction.x = 1;
                else
                    direction.x = 0;

                let position = localPosition.position;
                position.x += direction.x * car.speed * dt;

                localPosition.position = position;
                car.direction = direction;
3            });
        }
    }
}
