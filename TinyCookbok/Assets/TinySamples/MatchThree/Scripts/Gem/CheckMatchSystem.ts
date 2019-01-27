
namespace game {

    /**
     * Find matches in the gem grid and mark matched gems as so.
     */
    export class CheckMatchSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            if (GridService.isGridFrozen(this.world)) {
                return;
            }

            let grid = game.GridService.getGridConfiguration(this.world);
            let successfulGemSwap = false;
            this.world.forEach([ut.Entity, game.Gem], (gemEntity, gem) => {
                if (this.isMatchable(gem)) {
                    let leftGemEntity = game.GemService.getNeighborGem(this.world, grid, gem, -1, 0);
                    let rightGemEntity = game.GemService.getNeighborGem(this.world, grid, gem, 1, 0);
                    let downGemEntity = game.GemService.getNeighborGem(this.world, grid, gem, 0, -1);
                    let upGemEntity = game.GemService.getNeighborGem(this.world, grid, gem, 0, 1);
                    let leftGem = game.GemService.getGemFromEntity(this.world, leftGemEntity);
                    let rightGem = game.GemService.getGemFromEntity(this.world, rightGemEntity);
                    let downGem = game.GemService.getGemFromEntity(this.world, downGemEntity);
                    let upGem = game.GemService.getGemFromEntity(this.world, upGemEntity);
    
                    if (this.isMatchable(leftGem) && this.isMatchable(rightGem) && gem.GemType == leftGem.GemType && gem.GemType == rightGem.GemType) {
                        GemService.addMatchedComponent(this.world, gemEntity, true);
                        GemService.addMatchedComponent(this.world, leftGemEntity, true);
                        GemService.addMatchedComponent(this.world, rightGemEntity, true);

                        if (this.world.hasComponent(gemEntity, game.GemSwap) || this.world.hasComponent(leftGemEntity, game.GemSwap) || this.world.hasComponent(rightGemEntity, game.GemSwap)) {
                            successfulGemSwap = true;
                        }
                    }
                    else if (this.isMatchable(downGem) && this.isMatchable(upGem) && gem.GemType == downGem.GemType && gem.GemType == upGem.GemType) {
                        GemService.addMatchedComponent(this.world, gemEntity, true);
                        GemService.addMatchedComponent(this.world, downGemEntity, true);
                        GemService.addMatchedComponent(this.world, upGemEntity, true);
                        
                        if (this.world.hasComponent(gemEntity, game.GemSwap) || this.world.hasComponent(downGemEntity, game.GemSwap) || this.world.hasComponent(upGemEntity, game.GemSwap)) {
                            successfulGemSwap = true;
                        }
                    }
                }
            });

            if (successfulGemSwap) {
                game.GameService.incrementMoveCounter(this.world);
            }
        }

        isMatchable(gem: game.Gem): boolean {
            return gem != null && !gem.IsFalling && !gem.IsSwapping && gem.GemType != GemTypes.Egg && gem.GemType != GemTypes.ColorBomb;
        }
    }
}
