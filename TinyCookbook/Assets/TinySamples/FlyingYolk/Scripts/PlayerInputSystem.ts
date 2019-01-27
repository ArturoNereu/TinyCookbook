namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(game.PlayerInput, game.Velocity)
  export class PlayerInputSystem extends ut.ComponentSystem {

    /**
    * Applies some upward velocity when the left mouse button is pressed
    */
    OnUpdate(): void {

      if (this.world.getConfigData(game.GameConfig).state != game.GameState.Play) {
        return;
      }

      // check for user input outside of iteration
      if (!ut.Runtime.Input.getMouseButtonDown(0)) {
        return;
      }

      // play flap wing sound
      game.AudioService.playAudioSourceByName(this.world, 'audio/sfx_wing');

      // apply input to each entity with the `PlayerInput` component
      this.world.forEach(
        [ut.Entity, game.PlayerInput, game.Velocity],
        (entity, input, velocity) => {
          velocity.velocity = new ut.Math.Vector2(0, input.force);
        });
    }
  }
}
