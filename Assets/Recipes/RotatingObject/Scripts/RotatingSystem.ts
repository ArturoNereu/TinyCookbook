
namespace game {

    /** New System */
    export class RotatingSystem extends ut.ComponentSystem
    {    
        OnUpdate(): void
        {
            let dt = this.scheduler.deltaTime();

            this.world.forEach([ut.Core2D.TransformLocalRotation, game.Rotate], (rotation, rotate) =>
            {
                let rot = new Euler().setFromQuaternion(rotation.rotation);
                    
                rot.z += rotate.rotationSpeed * dt;

                rotation.rotation = new Quaternion().setFromEuler(rot);
            });
        }
    }
}
