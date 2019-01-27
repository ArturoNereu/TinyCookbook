
namespace game {

    export class UpdateGameStateSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            this.world.forEach([game.GameState], (gameState) => {
                GameStateUpdateService.updateGameState(this.world, gameState);
            });
        }
    }
}
