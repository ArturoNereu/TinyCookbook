
namespace game {

    /**
     * Animate a shake effect on a Transform.
     */
    export class AnimateShakeSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            let deltaTime = this.scheduler.deltaTime();
            this.world.forEach([ut.Entity, game.ShakeAnimation, game.ShakeAnimationPlayer, ut.Core2D.TransformLocalPosition],
                (entity, shakeAnimation, shakeAnimationPlayer, transformLocalPosition) => {

                    if (shakeAnimationPlayer.StartDelay > 0) {
                        shakeAnimationPlayer.StartDelay -= deltaTime;
                        return;
                    }
                    
                    this.setShakeDefaultPosition(shakeAnimation, transformLocalPosition);

                    let animationTime = shakeAnimationPlayer.Timer;
                    animationTime += deltaTime;
                    let isPlaying = animationTime < shakeAnimationPlayer.Duration;
                    shakeAnimationPlayer.Timer = animationTime;

                    let position = new Vector3(shakeAnimation.DefaultX, shakeAnimation.DefaultY, shakeAnimation.DefaultZ);

                    if (isPlaying) {
                        let shakeRatio = 1 - (animationTime / shakeAnimationPlayer.Duration);
                        let randomXOffset = Math.random() * shakeAnimationPlayer.ShakeRadiusX * shakeRatio;
                        if (Math.random() < 0.5) {
                            randomXOffset *= -1;
                        }

                        let randomYOffset = Math.random() * shakeAnimationPlayer.ShakeRadiusY * shakeRatio;
                        if (Math.random() < 0.5) {
                            randomYOffset *= -1;
                        }

                        position = new Vector3(position.x + randomXOffset, position.y + randomYOffset, 0);
                    }
                    else {
                        this.world.removeComponent(entity, game.ShakeAnimationPlayer);
                    }

                    transformLocalPosition.position = position;
            });
        }

        setShakeDefaultPosition(shakeAnimation: game.ShakeAnimation, transformLocalPosition: ut.Core2D.TransformLocalPosition): void {
            
            if (shakeAnimation.IsDefaultPositionSet) {
                return;
            }
            
            shakeAnimation.DefaultX = transformLocalPosition.position.x;
            shakeAnimation.DefaultY = transformLocalPosition.position.y;
            shakeAnimation.DefaultZ = transformLocalPosition.position.z;
            shakeAnimation.IsDefaultPositionSet = true;
        }
    }
}
