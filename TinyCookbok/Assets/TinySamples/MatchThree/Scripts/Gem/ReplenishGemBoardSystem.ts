
namespace game {

    /**
     * Create new gems to refill the grid after the player matched and destroyed gems.
     */
    @ut.executeAfter(game.DeleteMatchedGemSystem)
    @ut.executeAfter(game.CreateNewGemBoardSystem)
    export class ReplenishGemBoardSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            
            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game || GridService.isGridFrozen(this.world)) {
                return;
            }

            
            let possibleGemTypes: number[] = [0, 1, 2, 3, 4, 5];
            let currentLevel = game.GameService.getCurrentLevel(this.world);
            currentLevel.MissingGems.forEach(missingGem => {
                let indexToRemove = possibleGemTypes.indexOf(missingGem);
                possibleGemTypes.splice(indexToRemove, 1);
            });
            
            let grid = game.GridService.getGridConfiguration(this.world);
            for (let i = 0; i < grid.Width; i++) {
                let fallOffset = 0;
                for (let j = 0; j < grid.Height; j++) {
                    let existingGemEntity = game.GemService.getGemEntityAtPosition(this.world, grid, i, j);

                    if (existingGemEntity == null) {
                        let newGemEntity = game.GemService.createGem(this.world, grid, GridService.getCellHashCode(grid, i, j), possibleGemTypes);
                        let newGem = this.world.getComponentData(newGemEntity, game.Gem);
                        let spawnYPosition = grid.Height + fallOffset;
                        let transformLocalPosition = this.world.getComponentData(newGemEntity, ut.Core2D.TransformLocalPosition);
                        let newPosition = game.GridService.getGridToWorldPosition(grid, i, spawnYPosition);
                        transformLocalPosition.position.x = newPosition.x;
                        transformLocalPosition.position.y = newPosition.y;
                        this.world.setComponentData(newGemEntity, transformLocalPosition);
                        game.GemService.animateGemFall(this.world, grid, newGemEntity, newGem, spawnYPosition - j);

                        fallOffset++;
                    }
                }
            }
        }
    }
}
