/// <reference path="ReplenishGemBoardSystem.ts" />
namespace game {

    /**
     * Find possible gem swap to create a match and mark these gems as a possible match.
     * Used by the match hint mechanism.
     */
    @ut.executeAfter(game.ReplenishGemBoardSystem)
    export class FindPossibleMatchSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            let matchableGemCount = 0;
            this.world.forEach([ut.Entity, game.Gem, game.MatchPossibility], (entity, gem) => {
                matchableGemCount++;
            });

            if (matchableGemCount > 0) {
                return;
            }

            let grid = GridService.getGridConfiguration(this.world);
            if (grid == null) {
                return;
            }

            let foundMatch = false;
            this.world.forEach([game.Gem], (gem) => {

                if (!foundMatch && this.isMatchable(gem)) {
                    let gemPosition = game.GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
                    if ((gemPosition.x + gemPosition.y) % 2 == 0) {
                        let gemEntity = game.GemService.getGemEntity(this.world, grid, gem.CellHashKey);
                        let leftGemEntity = game.GemService.getNeighborGem(this.world, grid, gem, -1, 0);
                        let rightGemEntity = game.GemService.getNeighborGem(this.world, grid, gem, 1, 0);
                        let downGemEntity = game.GemService.getNeighborGem(this.world, grid, gem, 0, -1);
                        let upGemEntity = game.GemService.getNeighborGem(this.world, grid, gem, 0, 1);
    
                        if (this.trySwapAndMatch(grid, gemEntity, leftGemEntity)) {
                            foundMatch = true;
                        }
                        else if (this.trySwapAndMatch(grid, gemEntity, rightGemEntity)) {
                            foundMatch = true;
                        }
                        else if (this.trySwapAndMatch(grid, gemEntity, downGemEntity)) {
                            foundMatch = true;
                        }
                        else if (this.trySwapAndMatch(grid, gemEntity, upGemEntity)) {
                            foundMatch = true;
                        }
                    }
                }
            });

            // TODO: if (foundMatch == false), trigger blocked!
        }

        trySwapAndMatch(grid: game.GridConfiguration, gemEntity1: ut.Entity, gemEntity2: ut.Entity): boolean
        {
            let gem1 = game.GemService.getGemFromEntity(this.world, gemEntity1);
            let gem2 = game.GemService.getGemFromEntity(this.world, gemEntity2);

            if (!this.isMatchable(gem1) || !this.isMatchable(gem2)) {
                return false;
            }

            game.GemService.swapGems(this.world, grid, gemEntity1, gem1, gemEntity2, gem2);
            let matched = this.checkMatchPossibility(grid, gem1, gem2);  
            game.GemService.swapGems(this.world, grid, gemEntity1, gem1, gemEntity2, gem2);
            return matched;
        }

        checkMatchPossibility(grid: game.GridConfiguration, swapedGem1: game.Gem, swapedGem2: game.Gem): boolean {

            let matchPossibilityCount = 0;

            for (let i = 0; i < grid.GemEntities.length; i++) {
                let gemEntity = grid.GemEntities[i];
                if (!this.world.exists(gemEntity)) {
                    continue;
                }
                let gem = this.world.getComponentData(gemEntity, game.Gem);

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
                        matchPossibilityCount++;
                        this.addMatchPossibility(gemEntity, gem, swapedGem1, swapedGem2);
                        this.addMatchPossibility(leftGemEntity, leftGem, swapedGem1, swapedGem2);
                        this.addMatchPossibility(rightGemEntity, rightGem, swapedGem1, swapedGem2);
                    }
                    else if (this.isMatchable(downGem) && this.isMatchable(upGem) && gem.GemType == downGem.GemType && gem.GemType == upGem.GemType) {
                        matchPossibilityCount++;
                        this.addMatchPossibility(gemEntity, gem, swapedGem1, swapedGem2);
                        this.addMatchPossibility(downGemEntity, downGem, swapedGem1, swapedGem2);
                        this.addMatchPossibility(upGemEntity, upGem, swapedGem1, swapedGem2);
                    }
                }
            }

            return matchPossibilityCount > 0;
        }

        addMatchPossibility(gemEntity: ut.Entity, gem: game.Gem, swapedGem1: game.Gem, swapedGem2: game.Gem) {
            if (!this.world.hasComponent(gemEntity, game.MatchPossibility)) {
                let matchPossibility = new game.MatchPossibility()
                matchPossibility.NeedsSwap = (gem.CellHashKey == swapedGem1.CellHashKey || gem.CellHashKey == swapedGem2.CellHashKey);
                matchPossibility.SwapGem1HashKey = swapedGem1.CellHashKey;
                matchPossibility.SwapGem2HashKey = swapedGem2.CellHashKey;
                this.world.addComponentData(gemEntity, matchPossibility);
            }
        }

        isMatchable(gem: game.Gem): boolean {
            return gem != null && gem.GemType != GemTypes.Egg && gem.GemType != GemTypes.ColorBomb;
        }
    }
}
