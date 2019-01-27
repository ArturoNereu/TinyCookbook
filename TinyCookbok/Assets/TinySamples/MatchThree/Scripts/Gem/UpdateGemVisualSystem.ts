
namespace game {

    @ut.executeAfter(ut.Shared.UserCodeEnd)
    @ut.executeBefore(ut.Shared.RenderingFence)
    export class UpdateGemVisualSystem extends ut.ComponentSystem {

        OnUpdate():void {
            let showHintDelay = game.GameService.getGameState(this.world).ShowHintDelay;
            let deltaTime = this.scheduler.deltaTime();
            this.world.forEach([ut.Entity, game.Gem], (gemEntity, gem) => {
                this.updateHighlightAlpha(gemEntity, gem, showHintDelay, deltaTime);
                this.updatePowerUpVisual(gem);
            });
        }

        updateHighlightAlpha(gemEntity: ut.Entity, gem: game.Gem, showHintDelay: number, deltaTime: number) {
            let highlightAlpha = 0;
            if (gem.IsSelected && gem.GemType != GemTypes.ColorBomb) {
                // Highlight gem if it's selected.
                highlightAlpha = 1;
            }
            else if (this.world.hasComponent(gemEntity, game.MatchPossibility)) {
                // Update the gem highlight opacity animation for match hint.
                let matchPossibility = this.world.getComponentData(gemEntity, game.MatchPossibility);
                let timer = matchPossibility.HintTimer;
                timer += deltaTime;
                matchPossibility.HintTimer = timer;
                this.world.setComponentData(gemEntity, matchPossibility);
                
                if (timer > showHintDelay) {
                    highlightAlpha = 1 - ((Math.cos(6 * (timer - showHintDelay)) + 1) / 2);
                }
            }

            if (highlightAlpha != gem.HighlightAlpha) {
                gem.HighlightAlpha = highlightAlpha;

                let highlightSpriteRenderer = this.world.getComponentData(gem.SpriteRendererHighlightGem, ut.Core2D.Sprite2DRenderer);
                let color = highlightSpriteRenderer.color;
                color.a = highlightAlpha;
                highlightSpriteRenderer.color = color;
                this.world.setComponentData(gem.SpriteRendererHighlightGem, highlightSpriteRenderer);
            }
        }

        updatePowerUpVisual(gem: game.Gem) {
            if (gem.CurrentPowerUpVisual != gem.PowerUp) {
                gem.CurrentPowerUpVisual = gem.PowerUp;

                GameService.setEntityEnabled(this.world, gem.RowPowerUpVisual, gem.PowerUp == GemPowerUpTypes.Row);
                GameService.setEntityEnabled(this.world, gem.ColumnPowerUpVisual, gem.PowerUp == GemPowerUpTypes.Column);
                GameService.setEntityEnabled(this.world, gem.SquarePowerUpVisual, gem.PowerUp == GemPowerUpTypes.Square);
                GameService.setEntityEnabled(this.world, gem.DiagonalPowerUpVisual, gem.PowerUp == GemPowerUpTypes.DiagonalCross);
                GameService.setEntityEnabled(this.world, gem.SameColorPowerUpVisual, gem.PowerUp == GemPowerUpTypes.SameColor);
            }
        }
    }
}
