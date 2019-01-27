
namespace game {

    /**
     * Spawn an helicopter if none exists.
     */
    export class SpawnHelicopterSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            let activeHelicopterCount = 0;
            this.world.forEach([game.Helicopter],
                (helicopter) => {
                    activeHelicopterCount++;
                });

            if (activeHelicopterCount > 0) {
                return;
            }

            let sceneWidth = game.GameService.getGameState(this.world).EnvironmentSceneWidth;
            this.world.forEach([game.HelicopterSpawner, ut.Core2D.TransformLocalPosition],
                (helicopterSpawner, transformLocalPosition) => {
                    let helicopterEntity = ut.EntityGroup.instantiate(this.world, "game.Helicopter")[0];
                    let helicopterTransformPosition = this.world.getComponentData(helicopterEntity, ut.Core2D.TransformLocalPosition);
                    let helicopterTransformScale = this.world.getComponentData(helicopterEntity, ut.Core2D.TransformLocalScale);
                    helicopterTransformPosition.position.x = Math.random() * sceneWidth - sceneWidth / 2;
                    helicopterTransformScale.scale.x = helicopterTransformPosition.position.x > 0 ? 1 : -1;
                    helicopterTransformPosition.position.y = transformLocalPosition.position.y;
                    this.world.setComponentData(helicopterEntity, helicopterTransformPosition);
                    this.world.setComponentData(helicopterEntity, helicopterTransformScale);
                });
        }
    }
}
