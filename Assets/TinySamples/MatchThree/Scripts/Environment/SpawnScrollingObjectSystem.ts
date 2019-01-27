namespace game {

    /**
     * Spawn scrolling objects to fill the screen scenery with repeated tiled objects.
     */
    export class SpawnScrollingObjectSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            let currentSkin = SkinService.getCurrentSkin(this.world);

            let halfSceneWidth = game.GameService.getGameState(this.world).EnvironmentSceneWidth / 2;
            let minPositionX = -halfSceneWidth;
            let maxPositionX = halfSceneWidth;

            let rightMostObjectPositionsBySpawner = {};
            this.world.forEach([ut.Entity, game.ScrollingObject, ut.Core2D.TransformLocalPosition], (entity, scrollingObject, transformLocalPosition) => {

                let position = transformLocalPosition.position;
                if (!(scrollingObject.SpawnerName in rightMostObjectPositionsBySpawner) || position.x > rightMostObjectPositionsBySpawner[scrollingObject.SpawnerName]) {
                    rightMostObjectPositionsBySpawner[scrollingObject.SpawnerName] = position.x;
                }
            });

            let rightMostObjectPosition = minPositionX;
            this.world.forEach([ut.Entity, game.ScrollingObjectSpawner, ut.Core2D.TransformLocalPosition], (spawnerEntity, scrollingObjectSpawner, transformLocalPosition) => {
                scrollingObjectSpawner.SpawnDetailsBySkin.forEach(spawnDetails => {
                    if (spawnDetails.Skin == currentSkin) {
                        let name = this.world.getEntityName(spawnerEntity);
                        rightMostObjectPosition = name in rightMostObjectPositionsBySpawner ? rightMostObjectPositionsBySpawner[name] : minPositionX;

                        let spawnPositionY = transformLocalPosition.position.y;
                        let currentSpawnPositionX = rightMostObjectPosition + scrollingObjectSpawner.NextObjectDistance;

                        while (currentSpawnPositionX < maxPositionX) {
                            let randomObjectIndex = Math.floor(Math.random() * scrollingObjectSpawner.EntitiesToSpawn.length);
                            let randomObject = scrollingObjectSpawner.EntitiesToSpawn[randomObjectIndex];
                            let scrollingObjectEntity = ut.EntityGroup.instantiate(this.world, randomObject)[0];

                            let scrollingObject = this.world.getComponentData(scrollingObjectEntity, game.ScrollingObject);
                            scrollingObject.SpawnerName = this.world.getEntityName(spawnerEntity);
                            this.world.setComponentData(scrollingObjectEntity, scrollingObject);

                            let scrollingObjectTransformScale = this.world.getComponentData(scrollingObjectEntity, ut.Core2D.TransformLocalScale);
                            let scrollingObjectScale = scrollingObjectTransformScale.scale;
                            if (scrollingObjectSpawner.RandomizeFlipX && Math.random() < 0.5) {
                                scrollingObjectScale.x *= -1;
                            }
                            if (scrollingObjectSpawner.RandomizeFlipY && Math.random() < 0.5) {
                                scrollingObjectScale.y *= -1;
                            }
                            scrollingObjectTransformScale.scale = scrollingObjectScale;
                            this.world.setComponentData(scrollingObjectEntity, scrollingObjectTransformScale);

                            
                            let xPosition = currentSpawnPositionX + scrollingObjectSpawner.NextObjectDistance;
                            let yPosition = spawnPositionY + Math.floor(Math.random() * scrollingObjectSpawner.MaxRandomYOffset);
                            let scrollingObjectTransform = this.world.getComponentData(scrollingObjectEntity, ut.Core2D.TransformLocalPosition);
                            scrollingObjectTransform.position = new Vector3(currentSpawnPositionX, yPosition, 0);
                            this.world.setComponentData(scrollingObjectEntity, scrollingObjectTransform);

                            currentSpawnPositionX = xPosition;

                            scrollingObjectSpawner.NextObjectDistance = spawnDetails.MinSpawnIntervalX;
                            if (Math.random() < spawnDetails.SpawnIntervalFrequency) {
                                scrollingObjectSpawner.NextObjectDistance += Math.floor(Math.random() * (spawnDetails.MaxSpawnIntervalX - spawnDetails.MinSpawnIntervalX));
                            } 
                        }
                    }
                });
            });
        }
    }
}