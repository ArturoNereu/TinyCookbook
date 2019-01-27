namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeAfter(game.DamageSystem)
  @ut.executeBefore(game.RaiderSystem)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(game.Raiders)
  @ut.requiredComponents(ut.Core2D.TransformLocalPosition)
  export class RaidersSystem extends ut.ComponentSystem {  
    OnUpdate(): void {
      let dt = this.scheduler.deltaTime();
      let context = this.world.getConfigData(game.GameContext);
      let isGameWon = false;

      this.world.forEach([ut.Entity, game.Raiders, ut.Core2D.TransformLocalPosition],
        (entity, raiders, transformlocalposition) => {
          /*
          * Choose a random raider to shoot
          */
          if (!game.LaserUtilities.laserExists(this.world, game.LaserTag.Raider)) {
            let raiderCount = 0;
            this.world.forEach([ut.Entity, game.Raider], (entity, raider) => {
              raiderCount++;
            });

            // no raider
            if (raiderCount == 0) {
              // Player has won!
              isGameWon = true;
              return;
            }

            // choose an raider
            context.RaiderIndex = Math.floor(Math.random() * raiderCount) + 1;
          }

          /*
          * Move all raiders
          */
          let direction = new Vector3(0, 0, 0);
          let position = transformlocalposition.position as Vector3;

          if (raiders.isMovingRight) {
            direction.x += 1;

            if (position.x > raiders.threshold) {
              // Change direction and move down
              raiders.isMovingRight = false;
              position.y -= 4;
            }
          }
          else {
            direction.x -= 1;

            if (position.x < -raiders.threshold) {
              // Change direction and move down
              raiders.isMovingRight = true;
              position.y -= 4;

              // Move faster
              raiders.speed += 5;

              // Make raider's sprite2d sequence faster
              this.world.forEach([ut.Entity, game.Raider, ut.Core2D.Sprite2DSequencePlayer], (entity, raider, sprite2dsequenceplayer) => {
                sprite2dsequenceplayer.speed += 0.1;
              });
            }
          }

          direction.normalize();
          direction.multiplyScalar(raiders.speed * dt);

          position.add(direction);
          transformlocalposition.position = position;
        });

        if(isGameWon){
          game.GameService.gameWin(this.world, context);
        }
        
        this.world.setConfigData(context);
    }

  }

}
