
namespace game {

    /**
     * Update the dinosaur claw/stratch animation and the laser gem destroy animations.
     */
    @ut.executeAfter(ut.Shared.UserCodeEnd)
    @ut.executeBefore(ut.Shared.RenderingFence)
    export class UpdateDestroyLineAnimation extends ut.ComponentSystem {
        
        OnUpdate():void {
            let deltaTime = this.scheduler.deltaTime();

            // Update the claw/stratch animation when using a special gem power up.
            this.world.forEach([game.DestroyLineAnimation, ut.Core2D.TransformLocalPosition, ut.Core2D.TransformLocalRotation, ut.Core2D.Sprite2DRendererOptions, ut.Core2D.Sprite2DRenderer],
                (destroyLineAnimation, transformPosition, transformRotation, rendererOptions, spriteRenderer) => {
                
                destroyLineAnimation.Timer += deltaTime;

                let position = transformPosition.position;
                position.x = destroyLineAnimation.StartPositionX;
                position.y = destroyLineAnimation.StartPositionY;
                transformPosition.position = position;
                
                let scaleRatio = Math.min(1, destroyLineAnimation.Timer / destroyLineAnimation.ScaleDuration);
                let startPosition = new Vector2(destroyLineAnimation.StartPositionX, destroyLineAnimation.StartPositionY);
                let endPosition = new Vector2(destroyLineAnimation.EndPositionX, destroyLineAnimation.EndPositionY);
                let startToEndDistance = startPosition.distanceTo(endPosition);
                rendererOptions.size = new Vector2(rendererOptions.size.x, scaleRatio * startToEndDistance);

                let colorRatio = Math.min(1, destroyLineAnimation.Timer / destroyLineAnimation.Duration);
                spriteRenderer.color = ut.Interpolation.InterpolationService.evaluateCurveColor(this.world, colorRatio, destroyLineAnimation.ColorGradient);

                let angle = Math.atan2(endPosition.y - startPosition.y, endPosition.x - startPosition.x) - Math.PI / 2;
                transformRotation.rotation = new Quaternion().setFromAxisAngle(new Vector3(0, 0, 1), angle);
            });

            // Update the gem laser animation when using a special 5-match same color bomb power up.
            this.world.forEach([game.DestroyLaserAnimation, ut.Core2D.TransformLocalPosition, ut.Core2D.TransformLocalRotation, ut.Core2D.Sprite2DRendererOptions],
                (destroyLaserAnimation, transformPosition, transformRotation, rendererOptions) => {
                
                destroyLaserAnimation.Timer += deltaTime;

                let position = transformPosition.position;
                position.x = destroyLaserAnimation.StartPositionX;
                position.y = destroyLaserAnimation.StartPositionY;
                transformPosition.position = position;
                
                let startPosition = new Vector2(destroyLaserAnimation.StartPositionX, destroyLaserAnimation.StartPositionY);
                let endPosition = new Vector2(destroyLaserAnimation.EndPositionX, destroyLaserAnimation.EndPositionY);
                let startToEndDistance = startPosition.distanceTo(endPosition);
                rendererOptions.size = new Vector2(startToEndDistance, rendererOptions.size.y);

                let angle = Math.atan2(endPosition.y - startPosition.y, endPosition.x - startPosition.x);
                transformRotation.rotation = new Quaternion().setFromAxisAngle(new Vector3(0, 0, 1), angle);
            });
        }
    }
}
