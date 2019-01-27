namespace game {

    /**
     * Create the grid cells and the gems at the start of a game.
     */
    export class CreateNewGemBoardSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game) {
                return;
            }

            let grid = game.GridService.getGridConfiguration(this.world);
            if (grid == null) {
                return;
            }
                    
            if (!grid.IsGridCreated) {

                if (this.world.hasComponent(game.GameService.getCurrentLevelEntity(this.world), game.LevelSurvival)) {
                    grid.GridOffsetPositionY = -7;
                }
                else {
                    grid.GridOffsetPositionY = 0;
                }

                game.GridService.createGridCells(this.world, grid);

                let gemEntities = this.createBoard(grid);
                this.placeStartEggs(gemEntities, grid);
                this.placeStartPowerUps(gemEntities);

                grid.IsGridCreated = true;
                this.world.setConfigData(grid);
            }
        }

        createBoard(grid: GridConfiguration):ut.Entity[] {

            let createdGems: ut.Entity[] = new Array();
            let currentLevel = game.GameService.getCurrentLevel(this.world);

            for (let i = 0; i < grid.Width; i++) {
                for (let j = 0; j < grid.Height; j++) {

                    let possibleGemTypes: number[] = [0, 1, 2, 3, 4, 5];
                    currentLevel.MissingGems.forEach(missingGem => {
                        let indexToRemove = possibleGemTypes.indexOf(missingGem);
                        possibleGemTypes.splice(indexToRemove, 1);
                    });

                    let leftGem = game.GemService.getGemAtPosition(this.world, grid, i - 1, j);
                    let secondLeftGem = game.GemService.getGemAtPosition(this.world, grid, i - 2, j);
                    if (leftGem != null && secondLeftGem != null && leftGem.GemType == secondLeftGem.GemType) {
                        this.removePossibleGemType(possibleGemTypes, leftGem.GemType);
                    }

                    let downGem = game.GemService.getGemAtPosition(this.world, grid, i, j - 1);
                    let secondDownGem = game.GemService.getGemAtPosition(this.world, grid, i, j - 2);
                    if (downGem != null && secondDownGem != null && downGem.GemType == secondDownGem.GemType) {
                        this.removePossibleGemType(possibleGemTypes, downGem.GemType);
                    }

                    let gemType = possibleGemTypes[Math.floor(Math.random() * possibleGemTypes.length)];
                    let gemEntity = game.GemService.createGemOfType(this.world, grid, GridService.getCellHashCode(grid, i, j), gemType);
                    createdGems.push(gemEntity);
                }
            }

            return createdGems;
        }

        removePossibleGemType(possibleGemTypes: number[], gemType: number): void {
            for (let i = 0; i < possibleGemTypes.length; i++)
            {
                if (possibleGemTypes[i] == gemType)
                {
                    possibleGemTypes.splice(i, 1);
                    i--;
                }
            }
        }

        placeStartEggs(gemEntities: ut.Entity[], grid: game.GridConfiguration): void {

            let gemXPositions: number[] = [1, 2, 3, 4, 5, 6, 7];

            let levelEntity = game.GameService.getCurrentLevelEntity(this.world);
            if (this.world.hasComponent(levelEntity, game.LevelEggObjective)) {
                let levelEggObjective = this.world.getComponentData(levelEntity, game.LevelEggObjective)
                let eggCountAtStart = levelEggObjective.EggsInGridAtStart;
                for (let i = 0; i < eggCountAtStart; i++) {
                    let randomIndex = Math.floor(Math.random() * gemXPositions.length);
                    let randomGemXPosition = gemXPositions[randomIndex];
                    gemXPositions.splice(randomIndex, 1);

                    let gemEntity = new ut.Entity();
                    for (let j = 0; j < gemEntities.length; j++) {
                        let currentGemEntity = gemEntities[j];
                        let currentGem = this.world.getComponentData(currentGemEntity, game.Gem);
                        let gemPosition = GridService.getPositionFromCellHashCode(grid, currentGem.CellHashKey);
                        if (gemPosition.y == grid.Height - 1 && gemPosition.x == randomGemXPosition) {
                            gemEntity = gemEntities[j];
                            game.GemService.setSpecialGemType(this.world, currentGemEntity, currentGem, game.GemTypes.Egg);
                            this.world.setComponentData(gemEntity, currentGem);

                            gemEntities.splice(j, 1);
                            break;
                        }
                    }
                }
            }
        }

        placeStartPowerUps(gemEntities: ut.Entity[]): void {

            let level = game.GameService.getCurrentLevel(this.world);
            let powerUpsToPlace = level.StartPowerUps;
            for (let i = 0; i < powerUpsToPlace.length; i++) {

                let randomGemIndex = Math.floor(Math.random() * gemEntities.length);
                let gemEntity = gemEntities[randomGemIndex];
                let gem = this.world.getComponentData(gemEntity, game.Gem);
                game.GemService.setGemPowerUp(this.world, gemEntity, gem, powerUpsToPlace[i]);
                this.world.setComponentData(gemEntity, gem);

                gemEntities.splice(randomGemIndex, 1);
            }
        }
    }
}
