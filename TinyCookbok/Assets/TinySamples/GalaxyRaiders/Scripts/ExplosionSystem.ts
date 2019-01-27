namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeAfter(game.DamageSystem)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(game.Explosion)
  export class ExplosionSystem extends ut.ComponentSystem {
    
    /**
     * Unspawn explosion entity when its timer has reached the end
     */
    OnUpdate(): void {
      this.world.forEach([ut.Entity, game.Explosion],
        (entity, explosion) => {

          explosion.duration = explosion.duration - 1;

          if (explosion.duration < 0) {
            this.world.destroyEntity(entity);
          }

        });
    }

  }

}
