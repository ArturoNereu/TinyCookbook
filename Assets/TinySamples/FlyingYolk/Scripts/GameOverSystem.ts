namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(game.PlayerInput, ut.HitBox2D.HitBoxOverlapResults, ut.Core2D.Sprite2DSequencePlayer)
  export class GameOverSystem extends ut.ComponentSystem {

    /**
     * Triggers game over when the `PlayerInput` entity touches ANY hitbox in the game
     */
    OnUpdate(): void {
      let gameOver = false;

      // game over on any collision contact
      this.world.forEach(
        [ut.Entity, game.PlayerInput, ut.HitBox2D.HitBoxOverlapResults, ut.Core2D.Sprite2DSequencePlayer],
        (entity, input, overlap, sequencePlayer) => {

          // disable input
          this.world.removeComponent(entity, game.PlayerInput);

          // stop the animation
          sequencePlayer.paused = true;

          // play hit sound
          game.AudioService.playAudioSourceByName(this.world, 'audio/sfx_hit');

          // fix: defer the game over call until after iteration
          gameOver = true;
        });

      if (gameOver) {
        game.GameService.gameOver(this.world);
      }
    }
  }
}
