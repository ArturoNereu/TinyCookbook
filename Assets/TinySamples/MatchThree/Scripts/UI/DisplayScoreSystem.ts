
namespace game {

    /**
     * Refresh the current score label when it changes.
     */
    export class DisplayScoreSystem extends ut.ComponentSystem {
        
        OnUpdate(): void {
            this.world.forEach([game.ScoreDisplay, ut.Text.Text2DRenderer],
                (score, tr) => {
                    
                    let strScore = game.GameService.formatNumber(GameService.getGameState(this.world).CurrentScore);
                    if (strScore != tr.text) {
                        tr.text = strScore;
                    }
                });
        }
    }
}

