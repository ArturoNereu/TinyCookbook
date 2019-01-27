namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(game.ScorePoint)
  @ut.optionalComponents(ut.Audio.AudioSource)
  export class ScorePointSystem extends ut.ComponentSystem {

    OnUpdate(): void {
      let player = this.world.getEntityByName('Player');
      if (player.isNone()) {
        return;
      }

      let gameConfig = this.world.getConfigData(game.GameConfig);
      let playerPosition = ut.Core2D.TransformService.computeWorldPosition(this.world, player)

      this.world.forEach([ut.Entity, game.ScorePoint, ut.Core2D.TransformObjectToWorld],
        (entity, scorepoint, o2w) => {
          let position = new Vector3(o2w.matrix.elements[12], o2w.matrix.elements[13], o2w.matrix.elements[14]);

          // when the player passes this entity add some value to the players score
          if (position.x < playerPosition.x) {
            gameConfig.currentScore = gameConfig.currentScore + scorepoint.value;
            this.world.removeComponent(entity, game.ScorePoint);

            // play score point sound
            game.AudioService.playAudioSourceByName(this.world, 'audio/sfx_point');
          }
        });

      this.world.setConfigData(gameConfig);
    }
  }
}