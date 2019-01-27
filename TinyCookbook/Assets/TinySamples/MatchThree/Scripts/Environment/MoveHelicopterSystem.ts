
namespace game {

    export class MoveHelicopterSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            let deltaTime = this.scheduler.deltaTime();
            this.world.forEach([game.Helicopter, ut.Core2D.TransformLocalPosition],
                (helicopter, localPosition) => {

                    let currentPosition = localPosition.position;
                    if (helicopter.ReachedDestination) {
                        // Hover on place in a ∞ pattern.
                        helicopter.Timer += deltaTime;
                        let moveRadius = 4;
                        let cycleDuration = 7;
                        let progress = ((helicopter.Timer % cycleDuration) / cycleDuration + 0.25) * (2 * Math.PI);
                        let offsetPosition = new Vector3(moveRadius * 2 * Math.cos(progress), moveRadius * Math.sin(2 * progress), 0);

                        currentPosition.x = helicopter.DestinationPositionX + offsetPosition.x;
                        currentPosition.y = helicopter.DestinationPositionY + offsetPosition.y;
                    }
                    else {
                        // Move to destination.
                        let distanceToDestination = currentPosition.distanceTo(new Vector3(helicopter.DestinationPositionX, helicopter.DestinationPositionY, 0));
                        let moveVector = new Vector3(helicopter.DestinationPositionX - currentPosition.x, helicopter.DestinationPositionY - currentPosition.y, 0);
                        moveVector = moveVector.normalize();

                        let speedRatio = 1;
                        if (distanceToDestination < helicopter.SlowDownDistance)
                            speedRatio = distanceToDestination / helicopter.SlowDownDistance;
                        let speed = helicopter.MoveSpeed * Math.max(0.2, speedRatio);
                        currentPosition.x += moveVector.x * deltaTime * speed;
                        currentPosition.y += moveVector.y * deltaTime * speed;

                        helicopter.ReachedDestination = distanceToDestination < 0.5;
                    }

                    localPosition.position = currentPosition;
                });
        }
    }
}
