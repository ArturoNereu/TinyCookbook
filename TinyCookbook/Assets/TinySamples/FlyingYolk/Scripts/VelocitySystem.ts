namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(game.PlayerInput, game.Velocity)
  export class VelocitySystem extends ut.ComponentSystem {

    /**
    * Updates the `TransformLocalPosition` based on the `Velocity` component each frame.
    */
    OnUpdate(): void {
      this.world.forEach([ut.Core2D.TransformLocalPosition, game.Velocity],
        (transform, velocity) => {
          let p = transform.position;
          let v = velocity.velocity;

          p.x += v.x;
          p.y += v.y;

          transform.position = p;
        });
    }
  }
}
