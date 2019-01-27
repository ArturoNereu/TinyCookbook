namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeAfter(ut.HTML.InputHandler)
  @ut.executeAfter(game.DamageSystem)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(game.Spaceship)
  
  export class SpaceshipSystem extends ut.ComponentSystem {    
    /**
     * Enables a spaceship to shoot from its position
     */
    OnUpdate(): void {
      let context = this.world.getConfigData(game.GameContext);

      this.world.forEach([ut.Entity, game.Spaceship],
        (entity, spaceship) => {
          if ((ut.Runtime.Input.getKey(ut.Core2D.KeyCode.Space) || 
              ut.Runtime.Input.getMouseButtonDown(0)) && 
              context.TimeElapsedSinceStart > context.StartShootingDelay &&
              !game.LaserUtilities.laserExists(this.world, game.LaserTag.Spaceship)) {
            game.LaserUtilities.spawnLaser(this.world, entity, 'game.SpaceshipLaser', 2);
          } 
        });
    }

  }

}
