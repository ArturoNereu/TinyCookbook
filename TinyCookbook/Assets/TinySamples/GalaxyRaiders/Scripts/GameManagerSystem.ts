namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeAfter(game.DamageSystem)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  export class GameManagerSystem extends ut.ComponentSystem {
    
    /** 
      * State machine to manage the global game state
      * We forward all calls to the 'GameService'. This gives us a hook in to the main game loop
      */
    OnUpdate(): void {
      let context = this.world.getConfigData(game.GameContext);

      switch (context.State) {
        case game.GameState.Initialize:
          {
            game.GameService.initialize(this.world, context);
          }
          break;

        case game.GameState.Menu:
          {
            if (ut.Runtime.Input.getMouseButtonDown(0)) {
              game.GameService.start(this.world, context);
            }
          }
          break;

        case game.GameState.Play:
          {
            context.TimeElapsedSinceStart += this.scheduler.deltaTime();
          }
          break;

        case game.GameState.GameOver:
          {
            if (ut.Runtime.Input.getMouseButtonDown(0)) {
              context.State = game.GameState.Initialize;
            }
          }
          break;
      }

      this.world.setConfigData(context);
    }
  }
}
