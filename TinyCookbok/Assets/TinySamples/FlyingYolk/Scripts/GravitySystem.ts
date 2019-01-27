namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(game.Gravity, game.Velocity)
  export class GravitySystem extends ut.ComponentSystem {

    /**
    * Applies constant forces to all entities with the `Gravity` component.
    */
    OnUpdate():void {
      let dt = this.scheduler.deltaTime();
      this.world.forEach([game.Gravity, game.Velocity], 
        (gravity, velocity) => {
          let v = velocity.velocity;
          let g = gravity.gravity;
          v.x += g.x * dt;
          v.y += g.y * dt;
          velocity.velocity = v;
        });
    }
  }
}