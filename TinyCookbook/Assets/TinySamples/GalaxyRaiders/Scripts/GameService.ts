namespace game {
    export class GameService {
        /**
         * @desc invoked once when the game is launched
         */
        static initialize(world: ut.World, context: game.GameContext) {
            this.reset(world, context);
            ut.EntityGroup.instantiate(world, 'game.GameMenu');
            context.State = game.GameState.Menu;
        };

        /**
         * @desc clears entities and resets the game
         */
        static reset(world: ut.World, context: game.GameContext) {
            ut.EntityGroup.destroyAll(world, 'game.GameScene');
            ut.EntityGroup.destroyAll(world, 'game.GameOver');
            ut.EntityGroup.destroyAll(world, 'game.GameWin');

            context.Life = 2;
            context.Score = 0;
        };

        /**
         * @desc starts the game
         */
        static start(world: ut.World, context: game.GameContext) {
            ut.EntityGroup.destroyAll(world, 'game.GameMenu');

            ut.EntityGroup.instantiate(world, 'game.GameScene');

            context.State = game.GameState.Play;
            context.TimeElapsedSinceStart = 0;
        };

        /**
         * @desc Increase player's score
         */
        static increaseScore(world: ut.World, context: game.GameContext, points : number) {
            context.Score += points;
        };

        /**
         * @desc Decrease player's life
         */
        static decreaseLife(world: ut.World, context: game.GameContext) { 
            context.Life -= 1;

            // "Respawn" the spaceship
            world.forEach([ut.Entity, game.Spaceship, ut.Core2D.Sprite2DSequencePlayer],(entity, spaceship, sprite2DSequencePlayer)=>{
                sprite2DSequencePlayer.paused = true;
                sprite2DSequencePlayer.paused = false;
            });
        };

        /**
         * @desc Stops the spaceship from shooting and moving
         */
        static stopSpaceship(world: ut.World) {
            world.forEach([ut.Entity, game.Spaceship],
                (entity, spaceship) => {
                    world.removeComponent(entity, game.Spaceship);
                });
        };

        /**
         * @desc Stops the hit process for the damage system
         */
        static stopHits(world: ut.World){
            world.forEach([ut.Entity, game.Hit],
                (entity, hit) => {
                    world.removeComponent(entity, game.Hit);
                });
        }

        /**
         * @desc Stops all the lasers
         */
        static stopLasers(world: ut.World){
            world.forEach([ut.Entity, game.Laser],
                (entity, laser) => {
                    world.destroyEntity(laser.reference);
                });
        }

        /**
         * @desc Stops the raiders from shooting and moving
         */
        static stopRaiders(world: ut.World) {
            world.forEach([ut.Entity, game.Raiders],
                (entity, raiders) => {
                    world.removeComponent(entity, game.Raiders);
                });

            world.forEach([ut.Entity, game.Raider],
                (entity, raider) => {
                    world.removeComponent(entity, game.Raider);
                });
        };

        /**
         * @desc Stops the movement of the ship
         */
        static stopMovement(world: ut.World) {
            world.forEach([ut.Entity, game.Move],
                (entity, move) => {
                    world.removeComponent(entity, game.Move);
                });
        };

        /**
         * @desc Ends the current game, player has lost
         */
        static gameOver(world: ut.World, context: game.GameContext) {
            ut.EntityGroup.instantiate(world, 'game.GameOver');

            // Play the spaceship destroy animation
            world.forEach([ut.Entity, game.Spaceship, ut.Core2D.Sprite2DSequencePlayer],(entity, spaceship, sprite2DSequencePlayer)=>{
                sprite2DSequencePlayer.loop = ut.Core2D.LoopMode.ClampForever; 
                sprite2DSequencePlayer.paused = false;  
            });

            this.stopGame(world);

            context.State = game.GameState.GameOver;
        };

        /**
         * @desc Ends the current game, player has won
         */
        static gameWin(world: ut.World, context: game.GameContext) {
            ut.EntityGroup.instantiate(world, 'game.GameWin');

            this.stopGame(world);

            context.State = game.GameState.GameOver;
        };

        /**
         * @desc Remove components related to an active playing session
         */
        static stopGame(world: ut.World) {
            this.stopLasers(world);
            this.stopHits(world);
            this.stopSpaceship(world);
            this.stopRaiders(world);
            this.stopMovement(world);
        };
    }
}