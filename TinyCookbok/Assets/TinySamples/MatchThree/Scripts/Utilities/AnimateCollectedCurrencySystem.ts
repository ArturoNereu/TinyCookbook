
namespace game {

    /**
     * Animate collected currencies (eggs) from collected position to an end (UI) position.
     */
    export class AnimateCollectedCurrencySystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            let deltaTime = this.scheduler.deltaTime();
            this.world.forEach([ut.Entity, game.CollectedCurrency, ut.Core2D.TransformLocalPosition, ut.Core2D.TransformLocalScale], (entity, collectedCurrency, transformLocalPosition, transformLocalScale) => {
                
                if (collectedCurrency.StartDelay > 0) {
                    collectedCurrency.StartDelay -= deltaTime;
                    return;
                }

                let progress = collectedCurrency.Timer / collectedCurrency.Duration;
                if (progress > 1) {
                    ut.Core2D.TransformService.destroyTree(this.world, entity, true);
                    return;
                }
                
                let scale = ut.Interpolation.InterpolationService.evaluateCurveFloat(this.world, progress, collectedCurrency.ScaleCurve);
                transformLocalScale.scale = new Vector3(scale, scale, 1);
                
                progress = ut.Interpolation.InterpolationService.evaluateCurveFloat(this.world, progress, collectedCurrency.ProgressCurve);
                
                // Calculate bezier curve.
                let xa = this.getPoint(progress, collectedCurrency.StartPosition.x, collectedCurrency.MidPosition.x);
                let ya = this.getPoint(progress, collectedCurrency.StartPosition.y, collectedCurrency.MidPosition.y);
                let xb = this.getPoint(progress, collectedCurrency.MidPosition.x, collectedCurrency.EndPosition.x);
                let yb = this.getPoint(progress, collectedCurrency.MidPosition.y, collectedCurrency.EndPosition.y);
                
                let position = transformLocalPosition.position;
                position.x = this.getPoint(progress, xa, xb);
                position.y = this.getPoint(progress, ya, yb);
                
                transformLocalPosition.position = position;

                collectedCurrency.Timer += deltaTime;
            });
        }

        getPoint(progress: number, n1: number, n2: number) {
            let diff = n2 - n1;
            return n1 + (progress * diff);
        }
    }
}
