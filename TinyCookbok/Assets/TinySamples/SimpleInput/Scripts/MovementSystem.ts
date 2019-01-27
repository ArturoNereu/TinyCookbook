namespace game {

  @ut.executeAfter(ut.Shared.InputFence)
  @ut.executeBefore(ut.Shared.RenderingFence)
  @ut.requiredComponents(ut.Core2D.TransformLocalPosition)
  @ut.requiredComponents(game.Movement)
  export class MovementSystem extends ut.ComponentSystem {

    OnUpdate(): void {
      let dt = this.scheduler.deltaTime();

      this.world.forEach([ut.Core2D.TransformLocalPosition, game.Movement],
        (transformLocalPosition, movement) => {
          let direction = new Vector3(0, 0, 0);

          if (ut.Runtime.Input.getKey(ut.Core2D.KeyCode.W)) {
            direction.y += 1;
          }

          if (ut.Runtime.Input.getKey(ut.Core2D.KeyCode.S)) {
            direction.y -= 1;
          }

          if (ut.Runtime.Input.getKey(ut.Core2D.KeyCode.D)) {
            direction.x += 1;
          }

          if (ut.Runtime.Input.getKey(ut.Core2D.KeyCode.A)) {
            direction.x -= 1;
          }

          direction.normalize();
          direction.multiplyScalar(movement.speed * dt);

          let position = transformLocalPosition.position;
          position.add(direction);
          transformLocalPosition.position = position;

        });
    }

  }

}
