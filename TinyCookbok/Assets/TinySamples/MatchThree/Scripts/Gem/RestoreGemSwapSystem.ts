namespace game {

    /**
     * Swap gems back after an unsuccesful gem swap match attempt.
     */
    export class RestoreGemSwapSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            if (GridService.isGridFrozen(this.world)) {
                return;
            }

            let gem1HashKey = -1;
            let gem2HashKey = -1;

            // Find if there are 2 gems to swap back.
            this.world.forEach([ut.Entity, game.Gem, game.GemSwap], (entity, gem, gemSwap) => {

                if (gem.IsSwapping) {
                    return;
                }

                if (gem1HashKey == -1) {
                    gem1HashKey = gem.CellHashKey;
                    this.world.removeComponent(entity, game.GemSwap);
                }
                else if (gem2HashKey == -1) {
                    gem2HashKey = gem.CellHashKey;
                    this.world.removeComponent(entity, game.GemSwap);
                }
            }); 
    
            // Swap gems.
            if (gem1HashKey != -1 && gem2HashKey != -1) {
                let grid = GridService.getGridConfiguration(this.world);
                let gemEntity1 = game.GemService.getGemEntity(this.world, grid, gem1HashKey);
                let gem1 = game.GemService.getGem(this.world, grid, gem1HashKey);
                let gemEntity2 = game.GemService.getGemEntity(this.world, grid, gem2HashKey);
                let gem2 = game.GemService.getGem(this.world, grid, gem2HashKey);
                game.GemService.swapGems(this.world, grid, gemEntity1, gem1, gemEntity2, gem2);
                game.GemService.animateGemsSwap(this.world, grid, gemEntity1, gem1, gemEntity2, gem2);
            }
        }
    }
}
