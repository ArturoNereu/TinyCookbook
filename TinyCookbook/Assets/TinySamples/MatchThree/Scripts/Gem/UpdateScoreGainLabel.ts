
namespace game {

    /**
     * Update the gem match flying score gain label animation.
     */
    export class UpdateScoreGainLabel extends ut.ComponentSystem {
        
        OnUpdate():void {
            
            let deltaTime = this.scheduler.deltaTime();
            this.world.forEach([ut.Entity, game.ScoreGainLabel, ut.Core2D.TransformLocalPosition],
                (entity, scoreGainLabel, transformLocalPosition) => {

                    let progress = scoreGainLabel.Timer / scoreGainLabel.Duration;
                    if (progress > 1) {
                        ut.Core2D.TransformService.destroyTree(this.world, entity, true);
                    }
                    else {
                        scoreGainLabel.Timer += deltaTime;

                        let position = transformLocalPosition.position;
                        position.y += deltaTime * scoreGainLabel.SpeedY;
                        transformLocalPosition.position = position;

                        let alpha = ut.Interpolation.InterpolationService.evaluateCurveFloat(this.world, progress, scoreGainLabel.AlphaCurve);
                        let childCount = ut.Core2D.TransformService.countChildren(this.world, entity);
                        for (let i = 0; i < childCount; i++) {
                            let characterEntity = ut.Core2D.TransformService.getChild(this.world, entity, i);
                            let spriteRenderer = this.world.getComponentData(characterEntity, ut.Core2D.Sprite2DRenderer);
                            spriteRenderer.color = new ut.Core2D.Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
                            this.world.setComponentData(characterEntity, spriteRenderer);
                        }
                    }
                });
        }
    }
}
