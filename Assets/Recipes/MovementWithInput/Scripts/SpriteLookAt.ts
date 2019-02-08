
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

                if(carDirectionX < 0)
                {
                    localScale.scale = new Vector3(1, 1, 1);
                }
                else if(carDirectionX > 0)
                {
                    localScale.scale = new Vector3(-1, 1, 1);
                }

               
                console.log(localScale.scale.x);

            });    
        }
    }
}
