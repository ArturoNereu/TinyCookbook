namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(game.Velocity, ut.HitBox2D.HitBoxOverlapResults)
  export class GroundCollisionSystem extends ut.ComponentSystem {

    /**
     * Whenever any entity `hits` another entity with the `Ground` component; remove
     * it's `Velocity` component.
     */
    OnUpdate(): void {
      this.world.forEach([ut.Entity, game.Velocity, ut.HitBox2D.HitBoxOverlapResults],
        (entity, velocity, overlap) => {
          let grounded = false;
          let overlaps = overlap.overlaps;
          for (let i = 0; i < overlaps.length; i++) {
            let other = overlaps[i].otherEntity;
            if (this.world.hasComponent(other, game.Ground)) {
              grounded = true;
              break;
            }
          }

          if (grounded) {
            this.world.removeComponent(entity, game.Velocity);

            // @hack for FlyingYolk
            // we want the player to sit nicely on the ground
            // ideally this value should be computed or passed in
            this.world.usingComponentData(entity, [ut.Core2D.TransformLocalPosition], (position) => {
              let p = position.position;
              if (p.y < -0.95) {
                p.y = -0.95;
              }
            });
          }
        });
    }
  }
}
