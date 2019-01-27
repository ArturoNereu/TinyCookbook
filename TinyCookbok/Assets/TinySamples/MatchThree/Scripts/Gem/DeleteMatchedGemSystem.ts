/// <reference path="CollectEggSystem.ts" />

namespace game {

    @ut.executeAfter(game.CollectEggSystem)
    @ut.executeBefore(game.RestoreGemSwapSystem)
    export class DeleteMatchedGemSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            let grid = game.GridService.getGridConfiguration(this.world);
            if (GridService.isGridFrozen(this.world)) {
                return;
            }

            let matchedGemCount = 0;
            this.world.forEach([ut.Entity, game.Gem, game.Matched], (entity, gemToDestroy, matched) => {
                // Do not destroy gem if it was just turned into a power up from a match combo.
                
                if (matched.CreatedPowerUp == GemPowerUpTypes.None) {
                    game.GemService.deleteGem(this.world, grid, entity, gemToDestroy);
                }
                else {
                    this.world.removeComponent(entity, game.Matched);
                }
                matchedGemCount++;
            });

            if (matchedGemCount <= 0) {
                return;
            }

            SoundService.play(this.world, "MatchSound");

            // Make existing gems fall to fill the gap left by the destroyed gems.    
            for (let i = 0; i < grid.Width; i++) {
                let fallOffset = 0;
                for (let j = 0; j < grid.Height; j++) {
                    let gemEntity = game.GemService.getGemEntityAtPosition(this.world, grid, i, j);
                    let gem = game.GemService.getGemFromEntity(this.world, gemEntity);

                    if (gem == null) {
                        fallOffset++;
                    }
                    else {
                        if (this.world.hasComponent(gemEntity, game.MatchPossibility)) {
                            this.world.removeComponent(gemEntity, game.MatchPossibility);
                        }

                        let lastCellHashKey = gem.CellHashKey;
                        let currentCellHashKey = game.GridService.getCellHashCode(grid, i, j - fallOffset);
                        if (currentCellHashKey != lastCellHashKey) {
                            game.GemService.setGem(this.world, grid, lastCellHashKey, null);
                            gem.CellHashKey = currentCellHashKey;
                            this.world.setComponentData(gemEntity, gem);
                            game.GemService.setGem(this.world, grid, currentCellHashKey, gemEntity);

                            game.GemService.animateGemFall(this.world, grid, gemEntity, gem, fallOffset);
                        }
                    }
                }
            }

            this.world.forEach([ut.Entity, game.Gem, game.GemSwap], (entity, gem, swap) => {
                this.world.removeComponent(entity, game.GemSwap);
            });
        }
    }
}
