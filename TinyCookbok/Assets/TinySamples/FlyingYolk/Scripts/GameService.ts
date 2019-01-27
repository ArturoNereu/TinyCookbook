namespace game {
  export class GameService {

    /**
     * Special entity names
     */
    private static kPlayerEntityName: string = 'Player';
    private static kSpawnerEntityName: string = 'Spawner';
    
    /**
     * Scene names
     * 
     * @note these should be actual entity group references eventually
     */
    private static kTutorialSceneName: string = 'game.Tutorial';
    private static kGameSceneName: string = 'game.GameScene';
    private static kPipesSceneName: string = 'game.Pipes';
    private static kScoreSceneName: string = 'game.Score';
    private static kGameOverSceneName: string = 'game.GameOver';

    /**
     * Clears all game entities from the world to prepare for a new game
     */
    static clear(world: ut.World) {
      ut.Tweens.TweenService.removeAllTweensInWorld(world);

      ut.EntityGroup.destroyAll(world, this.kGameSceneName);
      ut.EntityGroup.destroyAll(world, this.kScoreSceneName);
      ut.EntityGroup.destroyAll(world, this.kGameOverSceneName);
      ut.EntityGroup.destroyAll(world, this.kPipesSceneName);
    };

    /**
     * invoked once when the game is launched
     */
    static initialize(world: ut.World) {
      this.startTutorial(world);
    };

    /**
     * starts the tutorial for the game
     */
    static startTutorial(world: ut.World) {
      this.newGame(world);

      this.setSpawnerPaused(world, true);

      // setup a 'fake' flying animation
      let player = world.getEntityByName(this.kPlayerEntityName);

      // disable gravity
      world.usingComponentData(player, [game.Gravity], (gravity) => {
        gravity.gravity = new ut.Math.Vector2();
      });

      // tween the bird up and down to make it look like its flying
      let transform = world.getComponentData(player,ut.Core2D.TransformLocalPosition);

      ut.Tweens.TweenService.addTween(world, 
        player, // on which entity
        ut.Core2D.TransformLocalPosition.position.y, // what component + field
        transform.position.y, transform.position.y + .1, // from -> to 
        0.4, // duration 
        0, // start time offset
        ut.Core2D.LoopMode.PingPong, 
        ut.Tweens.TweenFunc.InOutQuad,
        false // remove tween when done (ignored when looping)
        );

      // Day skin theme by default
      let skinConfig = world.getConfigData(game.SkinConfig);
      skinConfig.theme = game.SkinType.Day;
      skinConfig.forced = true;
      world.setConfigData(skinConfig);

      let gameConfig = world.getConfigData(game.GameConfig);
      gameConfig.state = game.GameState.Tutorial;
      world.setConfigData(gameConfig);

      // load in the tutorial scene
      // this scene shows the 'get ready' and 'tutorial' icon
      ut.EntityGroup.instantiate(world, this.kTutorialSceneName);

      // fade in the get ready
      let eReady = world.getEntityByName("Image_GetReady");
      ut.Tweens.TweenService.setValue(world, 
        eReady, // on which entity
        ut.Core2D.Sprite2DRenderer.color.a, // what component + field
        0 // value to set
        );

      ut.Tweens.TweenService.addTween(world, 
        eReady, // on which entity
        ut.Core2D.Sprite2DRenderer.color.a, // what component + field
        0, 1, // from -> to 
        4.0, // duration 
        -2.0, // start time offset
        ut.Core2D.LoopMode.Once, 
        ut.Tweens.TweenFunc.OutQuad,
        true // remove tween when done
        );

      // fade in the tutorial image
      ut.Tweens.TweenService.addTween(world, 
        world.getEntityByName("Image_Controls"), // on which entity
        ut.Core2D.Sprite2DRenderer.color.a, // what component + field
        0, 1, // from -> to 
        4, // duration 
        0, // start time offset
        ut.Core2D.LoopMode.Once, 
        ut.Tweens.TweenFunc.OutQuad,
        true // remove tween when done
        );
    };

    /**
     * starts a new game
     */
    static newGame(world: ut.World) {
      // clear all world objects
      this.clear(world);

      // create a new game scene and score
      ut.EntityGroup.instantiate(world, this.kGameSceneName);

      // setup the initial state for the game
      let config = world.getConfigData(game.GameConfig);
      config.currentScore = 0;
      config.currentScrollSpeed = config.scrollSpeed;
      config.state = game.GameState.Play;
      world.setConfigData(config);
    };

    /**
     * @desc ends the current tutorial
     */
    static endTutorial(world: ut.World) {
      // destroy the tutorial scene
      ut.EntityGroup.destroyAll(world, this.kTutorialSceneName);

      // un-pause the pipe spawner
      this.setSpawnerPaused(world, false);

      let player = world.getEntityByName(this.kPlayerEntityName);
      let gameConfig = world.getConfigData(game.GameConfig);

      // re-enable gravity
      world.usingComponentData(player, [game.Gravity], (gravity) => {
        gravity.gravity = new ut.Math.Vector2(0, gameConfig.gravity);
      });

      // stop all tweens from tutorial
      ut.Tweens.TweenService.removeAllTweensInWorld(world);

      gameConfig.state = game.GameState.Play;
      world.setConfigData(gameConfig);

      ut.EntityGroup.instantiate(world, this.kScoreSceneName);
    };

    /**
     * @desc ends the current game and shows the scoreboard
     */
    static gameOver(world: ut.World) {
      // hide the score view
      ut.EntityGroup.destroyAll(world, this.kScoreSceneName);

      // pause the pipe spawner
      this.setSpawnerPaused(world, true);
      
      let gameConfig = world.getConfigData(game.GameConfig);

      // stop scrolling the world
      gameConfig.currentScrollSpeed = 0;

      // update the highscore
      if (gameConfig.currentScore > gameConfig.highScore) {
        gameConfig.highScore = gameConfig.currentScore;
      }

      gameConfig.state = game.GameState.GameOver;

      world.setConfigData(gameConfig);

      // show the game over view
      ut.EntityGroup.instantiate(world, this.kGameOverSceneName);

      // tween in the game over text, position and alpha
      let eGameOver = world.getEntityByName("Image_GameOver");
      let transform = world.getComponentData(eGameOver, ut.Core2D.TransformLocalPosition);
      let end = transform.position;
      let start = new Vector3(end.x, end.y + 1.0, end.z);
      ut.Tweens.TweenService.addTween (
        world,
        eGameOver,
        ut.Core2D.TransformLocalPosition.position,
        start,
        end,
        1.35,
        0.0,
        ut.Core2D.LoopMode.Once,
        ut.Tweens.TweenFunc.OutBounce,
        true
        );

      ut.Tweens.TweenService.addTween (
        world,
        eGameOver,
        ut.Core2D.Sprite2DRenderer.color.a,
        0,
        1,
        0.45,
        0.0,
        ut.Core2D.LoopMode.Once,
        ut.Tweens.TweenFunc.OutBounce,
        true
        );

      // tween in the score board from the bottom
      let eBoard = world.getEntityByName("Image_ScoreBoard");
      transform = world.getComponentData(eBoard,ut.Core2D.TransformLocalPosition);
      end = transform.position;
      start = new Vector3(end.x, end.y - 1.0, end.z);
      ut.Tweens.TweenService.addTween (
        world,
        eBoard,
        ut.Core2D.TransformLocalPosition.position,
        start,
        end,
        0.35,
        0.0,
        ut.Core2D.LoopMode.Once,
        ut.Tweens.TweenFunc.OutQuad,
        true
        );

    };

    /**
     * Sets the paused flag of the `PipeSpawner` component
     */
    static setSpawnerPaused(world: ut.World, paused: boolean) {
      let entity = world.getEntityByName(this.kSpawnerEntityName);
      let spawner = world.getComponentData(entity, game.Spawner);
      spawner.paused = paused
      world.setComponentData(entity, spawner);
    }
  }
}