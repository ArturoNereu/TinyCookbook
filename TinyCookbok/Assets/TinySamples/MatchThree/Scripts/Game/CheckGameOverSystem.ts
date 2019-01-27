
namespace game {

    export class CheckGameOverSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game || GridService.isGridFrozen(this.world)) {
                return;
            }

            let levelEntity = game.GameService.getCurrentLevelEntity(this.world);

            // Trigger game over in limited move count modes.
            if (!game.GameService.hasRemainingMoves(this.world)) {
                if (!this.isGridAnimating()) {
                    GameStateLoadingService.setGameState(this.world, GameStateTypes.GameOver);
                }
            }
            // Trigger game over in survival mode.
            else if (this.world.hasComponent(levelEntity, game.LevelSurvival)) {
                let levelSurvival = this.world.getComponentData(levelEntity, game.LevelSurvival);
                if (levelSurvival.SurvivalTimer <= 0 && !this.isGridAnimating()) {
                    GameStateLoadingService.setGameState(this.world, GameStateTypes.GameOver);
                }
            }
        }

        // Wait for the gems to finish their animations before triggering the game over state
        isGridAnimating(): boolean {
            let isGridAnimating = false;
            
            this.world.forEach([game.Gem], (gem) => {
                if (gem.IsFalling || gem.IsSwapping) {
                    isGridAnimating = true;
                }
            });

            return isGridAnimating;
        }
    }
}
