namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeAfter(game.DamageSystem)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(game.Laser)
  @ut.requiredComponents(ut.Core2D.TransformLocalPosition)
  export class LaserSystem extends ut.ComponentSystem {
    
    /**
     * Moves a laser in the y-axis
     */
    OnUpdate(): void {
      let dt = this.scheduler.deltaTime();

      this.world.forEach([ut.Entity, game.Laser, ut.Core2D.TransformLocalPosition],
        (entity, laser, transformlocalposition) => {
          
          let direction = new Vector3(0, 1, 0);

          direction.normalize();
          direction.multiplyScalar(laser.speed * dt);

          transformlocalposition.position = transformlocalposition.position.add(direction);
        });
    }

  }

}
