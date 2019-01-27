
namespace game {

    export class SpawnKidOnBikeSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game || game.GameService.getCurrentLevel(this.world).LevelID != 3) {
                return;
            }

            let kidOnBikeSpawnerEntity = this.world.getEntityByName("KidOnBikeSpawner");
            if (!this.world.exists(kidOnBikeSpawnerEntity)) {
                return;
            }

            let activeKidCount = 0;
            this.world.forEach([game.KidOnBike],
                (kid) => {
                    activeKidCount++;
                });

            if (activeKidCount > 0) {
                return;
            }

            let kidOnBikeSpawner = this.world.getComponentData(kidOnBikeSpawnerEntity, game.KidOnBikeSpawner);
            kidOnBikeSpawner.Timer += this.scheduler.deltaTime();
            if (kidOnBikeSpawner.Timer > kidOnBikeSpawner.SpawnDelay) {
                kidOnBikeSpawner.Timer = 0;
                let kidEntity = ut.EntityGroup.instantiate(this.world, "game.KidOnBike")[0];
                let kidTransformPosition = this.world.getComponentData(kidEntity, ut.Core2D.TransformLocalPosition);
                kidTransformPosition.position.x = game.GameService.getGameState(this.world).EnvironmentSceneWidth / 2;
                kidTransformPosition.position.y = this.world.getComponentData(kidOnBikeSpawnerEntity, ut.Core2D.TransformLocalPosition).position.y;
                this.world.setComponentData(kidEntity, kidTransformPosition);
            }
            this.world.setComponentData(kidOnBikeSpawnerEntity, kidOnBikeSpawner);
        }
    }
}
