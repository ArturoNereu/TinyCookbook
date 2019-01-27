
namespace game {

    /**
     * Spawn people characters from the side of buildings
     */
    export class SpawnPeopleSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            
            let peopleCount = 0;
            this.world.forEach([game.Person],
                (person) => {
                    peopleCount++;
                });

            let maxPeopleCount;
            let inSpawnCooldown = false;
            let deltaTime = this.scheduler.deltaTime();
            this.world.forEach([game.PeopleSpawner],
                (peopleSpawner) => {

                    peopleSpawner.Timer += deltaTime;
                    inSpawnCooldown = peopleSpawner.Timer < peopleSpawner.SpawnInterval;
                    maxPeopleCount = peopleSpawner.MaxPeopleCount;
                });

            if (inSpawnCooldown || peopleCount >= maxPeopleCount) {
                return;
            }

            let closestBuildingPosition = this.getClosestBuildingPosition();
            
            this.world.forEach([game.PeopleSpawner, ut.Core2D.TransformLocalPosition],
                (peopleSpawner, spawnerLocalPosition) => {
                
                    peopleSpawner.Timer = 0;

                    let personEntity = ut.EntityGroup.instantiate(this.world, "game.Person")[0];
                    let person = this.world.getComponentData(personEntity, game.Person);
                    person.MoveSpeedMultiplier = 1 + Math.random() * 3;
                    person.WalkPositionY = spawnerLocalPosition.position.y;
                    this.world.setComponentData(personEntity, person);
                    let transformLocalPosition = this.world.getComponentData(personEntity, ut.Core2D.TransformLocalPosition);
                    transformLocalPosition.position.x = closestBuildingPosition.x + 20;
                    transformLocalPosition.position.y = spawnerLocalPosition.position.y + 6;
                    this.world.setComponentData(personEntity, transformLocalPosition);
                });
        }

        getClosestBuildingPosition(): Vector3 {
            let targetBuildingXPosition = 80;

            let closestBuildingEntity: ut.Entity;
            let closestBuildingPosition: Vector3;
            this.world.forEach([ut.Entity, game.Building, ut.Core2D.TransformLocalPosition],
                (entity, building, transformLocalPosition) => {

                    if (closestBuildingEntity == null || Math.abs(transformLocalPosition.position.x - targetBuildingXPosition) < Math.abs(closestBuildingPosition.x - targetBuildingXPosition)) {
                        closestBuildingEntity = entity;
                        closestBuildingPosition = transformLocalPosition.position;
                    }
                });

            return closestBuildingPosition;
        }
    }
}
