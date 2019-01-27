
namespace game {

    /**
     * Swap the clicked or dragged gems with another one.
     */
    @ut.executeBefore(game.CheckMatchSystem)
    export class SwapGemSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game || !GameService.hasRemainingMoves(this.world)) {
                return;
            }

            let grid = GridService.getGridConfiguration(this.world);
            let previouslySelectedGemHashKey = -1;
            let previouslySelectedGemPosition: Vector2;
            let hoveredGemHashKey = -1;
            let isGemAnimating = false;
            
            let pointerWorldPosition = InputService.getPointerWorldPosition(this.world, this.world.getEntityByName("GridCamera"));
            let pointerDown = ut.Runtime.Input.getMouseButtonDown(0) || (ut.Runtime.Input.touchCount() == 1 && ut.Runtime.Input.getTouch(0).phase == ut.Core2D.TouchState.Began);
            let pointerPressed = ut.Runtime.Input.getMouseButton(0) || (ut.Runtime.Input.touchCount() == 1 &&
                (ut.Runtime.Input.getTouch(0).phase == ut.Core2D.TouchState.Stationary || ut.Runtime.Input.getTouch(0).phase == ut.Core2D.TouchState.Moved));

            // Find the gem currently under the input pointer.
            this.world.forEach([game.Gem, ut.Core2D.TransformLocalPosition],
                (gem, gemTransform) => {


                    if (gem.IsFalling || gem.IsSwapping) {
                        isGemAnimating = true;
                        return;
                    }

                    if (gem.IsSelected) {
                        previouslySelectedGemHashKey = gem.CellHashKey;
                        previouslySelectedGemPosition = new Vector2(gemTransform.position.x, gemTransform.position.y);

                        if (pointerDown) {
                            gem.IsSelected = false;
                        }
                    }
                    
                    let gemPosition = new Vector2(gemTransform.position.x, gemTransform.position.y);
                    if (Math.abs(gemPosition.x - pointerWorldPosition.x) <= grid.CellDimension / 2 && Math.abs(gemPosition.y - pointerWorldPosition.y) <= grid.CellDimension / 2) {
                        hoveredGemHashKey = gem.CellHashKey;
                    }
                });

            if (isGemAnimating) {
                return;
            }

            if (pointerDown && hoveredGemHashKey != -1) {
                // Handle gem selection and two-step click match.
                let clickedGemEntity = game.GemService.getGemEntity(this.world, grid, hoveredGemHashKey);
                let clickedGem = game.GemService.getGemFromEntity(this.world, clickedGemEntity);
                let previouslySelectedGemEntity = game.GemService.getGemEntity(this.world, grid, previouslySelectedGemHashKey);
                let previouslySelectedGem = game.GemService.getGemFromEntity(this.world, previouslySelectedGemEntity);
                if (previouslySelectedGemHashKey != -1 && clickedGem.CellHashKey == previouslySelectedGem.CellHashKey) {
                    // Deselect gem if previously clicked gem is already selected.
                    previouslySelectedGem.IsSelected = false;
                    this.world.setComponentData(previouslySelectedGemEntity, previouslySelectedGem);
                }
                else if (previouslySelectedGemHashKey == -1 || !game.GemService.areGemsNeighbor(grid, clickedGem, previouslySelectedGem)) {
                    clickedGem.IsSelected = true;
                    this.world.setComponentData(clickedGemEntity, clickedGem);
                }
                else {
                    this.swapGems(grid, previouslySelectedGemEntity, previouslySelectedGem, clickedGemEntity, clickedGem);
                }
            }
            else if (pointerPressed && hoveredGemHashKey != previouslySelectedGemHashKey && previouslySelectedGemHashKey != -1)
            {
                // Trigger gem swap when the selected gem is dragged over another gem.
                let gemToSwapWithPosition: Vector2;
                let gemPosition = game.GridService.getPositionFromCellHashCode(grid, previouslySelectedGemHashKey);
                let xDiff = pointerWorldPosition.x - previouslySelectedGemPosition.x;
                let yDiff = pointerWorldPosition.y - previouslySelectedGemPosition.y;
                let isHorizontalMatch = Math.abs(xDiff) > Math.abs(yDiff);
                if (isHorizontalMatch && xDiff > 0) {
                    gemToSwapWithPosition = new Vector2(gemPosition.x + 1, gemPosition.y);
                }
                else if (isHorizontalMatch && xDiff < 0) {
                    gemToSwapWithPosition = new Vector2(gemPosition.x - 1, gemPosition.y);
                }
                else if (!isHorizontalMatch && yDiff > 0) {
                    gemToSwapWithPosition = new Vector2(gemPosition.x, gemPosition.y + 1);
                }
                else if (!isHorizontalMatch && yDiff < 0) {
                    gemToSwapWithPosition = new Vector2(gemPosition.x, gemPosition.y - 1);
                }
                
                let previouslySelectedGemEntity = game.GemService.getGemEntity(this.world, grid, previouslySelectedGemHashKey);
                let previouslySelectedGem = game.GemService.getGemFromEntity(this.world, previouslySelectedGemEntity);
                let gemToSwapWithEntity = game.GemService.getGemEntityAtPosition(this.world, grid, gemToSwapWithPosition.x, gemToSwapWithPosition.y);
                let gemToSwapWith = game.GemService.getGemFromEntity(this.world, gemToSwapWithEntity);
                if (gemToSwapWith != null){
                    this.swapGems(grid, previouslySelectedGemEntity, previouslySelectedGem, gemToSwapWithEntity, gemToSwapWith);
                }

                previouslySelectedGem.IsSelected = false;
                this.world.setComponentData(previouslySelectedGemEntity, previouslySelectedGem);
            }
        }

        swapGems(grid: game.GridConfiguration, gemEntity1: ut.Entity, gem1: game.Gem, gemEntity2: ut.Entity, gem2: game.Gem):void {
            if (!this.world.hasComponent(gemEntity1, game.GemSwap)) {
                this.world.addComponent(gemEntity1, game.GemSwap);
            }
            if (!this.world.hasComponent(gemEntity2, game.GemSwap)) {
                this.world.addComponent(gemEntity2, game.GemSwap);
            }

            game.GemService.swapGems(this.world, grid, gemEntity1, gem1, gemEntity2, gem2);
            game.GemService.animateGemsSwap(this.world, grid, gemEntity1, gem1, gemEntity2, gem2);

            SoundService.play(this.world, "GemSwapSound");
        }
    }
}
