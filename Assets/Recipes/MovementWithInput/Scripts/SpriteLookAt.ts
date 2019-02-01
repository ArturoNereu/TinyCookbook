
namespace game {

    /** New System */
    export class SpriteLookAt extends ut.ComponentSystem
    {
        Onst

        OnUpdate(): void
        {
            this.world.forEach([ut.Core2D.TransformLocalScale, game.Car], (localScale, car) =>
            {
                let carDirectionX = car.direction.x;

                // TODO: Avoid asignation every frame
                if (carDirectionX < 0)
                    localScale.scale.x = -0.16;
                else
                    localScale.scale.x = 0.16

            });    
        }
    }
}
