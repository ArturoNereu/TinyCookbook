/// <reference path="UpdateScoreSystem.ts" />
/// <reference path="DeleteMatchedGemSystem.ts" />

namespace game {

    /**
     * Spawn special gem bomb power up after the player makes a special match (4-match, 5-match, T-match and L-match).
     */
    @ut.executeAfter(game.UpdateScoreSystem)
    @ut.executeBefore(game.DeleteMatchedGemSystem)
    @ut.executeBefore(game.RestoreGemSwapSystem)
    export class SpawnComboPowerUpSystem extends ut.ComponentSystem {
        
        OnUpdate():void
        {
            let grid = GridService.getGridConfiguration(this.world);
            if (GridService.isGridFrozen(this.world)) {
                return;
            }

            let gemSwapCount = 0;
            this.world.forEach([ut.Entity, game.Gem, game.Matched, game.GemSwap], (gemEntity, gem, matched, swap) => {

                // Swaped matched gems will turn into a gem power up if a match combo was made 
                gemSwapCount++;

                let gemPosition = GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
                let leftStretch = this.calculateStretch(grid, gemEntity, gem, new Vector2(gemPosition.x, gemPosition.y), new Vector2(-1, 0));
                let rightStretch = this.calculateStretch(grid, gemEntity, gem, new Vector2(gemPosition.x, gemPosition.y), new Vector2(1, 0));
                let upStretch = this.calculateStretch(grid, gemEntity, gem, new Vector2(gemPosition.x, gemPosition.y), new Vector2(0, 1));
                let downStretch = this.calculateStretch(grid, gemEntity, gem, new Vector2(gemPosition.x, gemPosition.y), new Vector2(0, -1));

                let createdPowerUp = GemPowerUpTypes.None;

                // 5 in a line
                if (leftStretch + rightStretch == 4 || upStretch + downStretch == 4) {
                    createdPowerUp = game.GemPowerUpTypes.SameColor;
                }
                // L shape
                else if ((leftStretch == 2 && upStretch == 2) || (upStretch == 2 && rightStretch == 2) || 
                    (rightStretch == 2 && downStretch == 2) || (downStretch == 2 && leftStretch == 2)) {
                    createdPowerUp = game.GemPowerUpTypes.Square;
                }
                // T shape
                else if (leftStretch + rightStretch + downStretch + upStretch == 4) { 
                    createdPowerUp = game.GemPowerUpTypes.DiagonalCross;
                }
                // 4 in a row
                else if (leftStretch + rightStretch == 3) {
                    createdPowerUp = game.GemPowerUpTypes.Row;
                }
                // 4 in a column
                else if (upStretch + downStretch == 3) {
                    createdPowerUp = game.GemPowerUpTypes.Column;
                }

                matched.CreatedPowerUp = createdPowerUp;
                if (createdPowerUp != GemPowerUpTypes.None) {
                    gemSwapCount++;
                    game.GemService.setGemPowerUp(this.world, gemEntity, gem, createdPowerUp);
                }
            });

            if (gemSwapCount > 0) {
                return;
            }

            this.world.forEach([ut.Entity, game.Gem, game.Matched], (gemEntity, gem, matched) => {

                let isGemSwap = this.world.hasComponent(gemEntity, game.GemSwap);
                if (!isGemSwap) {
                    // Falling gem cascade (not triggered by player swap) can also trigger power up creation.

                    let gemPosition = GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
                    let leftStretch = this.calculateStretch(grid, gemEntity, gem, new Vector2(gemPosition.x, gemPosition.y), new Vector2(-1, 0));
                    let rightStretch = this.calculateStretch(grid, gemEntity, gem, new Vector2(gemPosition.x, gemPosition.y), new Vector2(1, 0));
                    let upStretch = this.calculateStretch(grid, gemEntity, gem, new Vector2(gemPosition.x, gemPosition.y), new Vector2(0, 1));
                    let downStretch = this.calculateStretch(grid, gemEntity, gem, new Vector2(gemPosition.x, gemPosition.y), new Vector2(0, -1));

                    let createdPowerUp = GemPowerUpTypes.None;
                    
                    // 5 in a line
                    if (rightStretch == 4 || upStretch == 4) {
                        createdPowerUp = game.GemPowerUpTypes.SameColor;
                    }
                    // L shape
                    else if ((leftStretch == 2 && upStretch == 2) || (upStretch == 2 && rightStretch == 2) || 
                        (rightStretch == 2 && downStretch == 2) || (downStretch == 2 && leftStretch == 2)) {
                        createdPowerUp = game.GemPowerUpTypes.Square;
                    }
                    // T shape
                    else if (leftStretch == 1 && rightStretch == 1 && downStretch == 2) { 
                        createdPowerUp = game.GemPowerUpTypes.DiagonalCross;
                    }
                    else if (leftStretch == 1 && rightStretch == 1 && upStretch == 2) { 
                        createdPowerUp = game.GemPowerUpTypes.DiagonalCross;
                    }
                    else if (downStretch == 1 && upStretch == 1 && leftStretch == 2) { 
                        createdPowerUp = game.GemPowerUpTypes.DiagonalCross;
                    }
                    else if (downStretch == 1 && upStretch == 1 && rightStretch == 2) { 
                        createdPowerUp = game.GemPowerUpTypes.DiagonalCross;
                    }
                    // 4 in a row
                    else if (rightStretch == 3) {
                        createdPowerUp = game.GemPowerUpTypes.Row;
                    }
                    // 4 in a column
                    else if (upStretch == 3) {
                        createdPowerUp = game.GemPowerUpTypes.Column;
                    }

                    matched.CreatedPowerUp = createdPowerUp;
                    if (createdPowerUp != GemPowerUpTypes.None) {
                        game.GemService.setGemPowerUp(this.world, gemEntity, gem, createdPowerUp);
                    }
                }
            });
        }

        calculateStretch(grid: game.GridConfiguration, gemEntity: ut.Entity, gem: game.Gem, origin: Vector2, direction: Vector2): number {
            let count = 0;

            let currentPosition = new Vector2(origin.x + direction.x, origin.y + direction.y);
            for (let i = 0; i < grid.Width; i++) {
                let currentGem = GemService.getGemAtPosition(this.world, grid, currentPosition.x, currentPosition.y);
                if (currentGem != null && currentGem.GemType == gem.GemType) {
                    currentPosition = new Vector2(currentPosition.x + direction.x, currentPosition.y + direction.y);
                    count++;
                }
                else {
                    break;
                }
            }

            return count;
        }
    }
}
