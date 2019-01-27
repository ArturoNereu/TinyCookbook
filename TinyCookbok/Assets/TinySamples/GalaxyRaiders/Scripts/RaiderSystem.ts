namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeAfter(game.DamageSystem)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(game.Raider)
  export class RaiderSystem extends ut.ComponentSystem {
    /**
     * Enables an raider to shoot from its position
     */
    OnUpdate(): void {
      let context = this.world.getConfigData(game.GameContext);
      let index = 0;
      this.world.forEach([ut.Entity, game.Raider],
        (entity, raider) => {
          
          if (context.RaiderIndex == index && 
            context.TimeElapsedSinceStart > context.StartShootingDelay &&
            !game.LaserUtilities.laserExists(this.world, game.LaserTag.Raider)) {
            game.LaserUtilities.spawnLaser(this.world, entity, 'game.RaiderLaser', -12); 
          }

          ++index;
        });
    }

  }

}
