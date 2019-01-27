/// <reference path="CheckMatchSystem.ts" />
/// <reference path="RestoreGemSwapSystem.ts" />

namespace game {

    @ut.executeAfter(game.CheckMatchSystem)
    @ut.executeBefore(game.RestoreGemSwapSystem)
    export class ActivateGemPowerUpSystem extends ut.ComponentSystem {
        
        static sameColorBombTriggeredThisFrame: boolean = false;

        OnUpdate():void {

            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game) {
                return;
            }

            let grid = game.GridService.getGridConfiguration(this.world);
            ActivateGemPowerUpSystem.sameColorBombTriggeredThisFrame = false;

            let deltaTime = this.scheduler.deltaTime();
            if (grid.FrozenGridTimer > 0) {
                grid.FrozenGridTimer -= deltaTime;
                this.world.setConfigData(grid);
            }
            else {
                this.activeSameColorBombAfterSwap(grid);
                this.activateMatchedBombs(grid);
            }
        }

        activeSameColorBombAfterSwap(grid: game.GridConfiguration) {
            let swapedGemEntities: ut.Entity[] = new Array();
            
            this.world.forEach([ut.Entity, game.Gem, game.GemSwap], (entity, gem, swaped) => {
                if (!gem.IsSwapping) {
                    let gridGemEntity = game.GemService.getGemEntity(this.world, grid, gem.CellHashKey);
                    let gridGem = game.GemService.getGem(this.world, grid, gem.CellHashKey);
                    if (gridGem != null && !gridGem.IsSwapping) {
                        swapedGemEntities.push(gridGemEntity);
                    }
                }
            });

            if (swapedGemEntities.length != 2) {
                return;
            }

            let gem1 = game.GemService.getGemFromEntity(this.world, swapedGemEntities[0]);
            let gem2 = game.GemService.getGemFromEntity(this.world, swapedGemEntities[1]);
            if (gem1.PowerUp == GemPowerUpTypes.SameColor || gem2.PowerUp == GemPowerUpTypes.SameColor) {
                let bombGemEntity = gem1.PowerUp == GemPowerUpTypes.SameColor ? swapedGemEntities[0] : swapedGemEntities[1];
                let otherGemEntity = gem1.PowerUp == GemPowerUpTypes.SameColor ? swapedGemEntities[1] : swapedGemEntities[0];
                let bombGem = gem1.PowerUp == GemPowerUpTypes.SameColor ? gem1 : gem2;
                let otherGem = gem1.PowerUp == GemPowerUpTypes.SameColor ? gem2 : gem1;
                let bombGemPosition = game.GridService.getPositionFromCellHashCode(grid, bombGem.CellHashKey);

                if (otherGem.PowerUp != GemPowerUpTypes.SameColor && otherGem.GemType != GemTypes.Egg) {
                    
                    if (!this.world.hasComponent(otherGemEntity, game.Matched)) {
                        game.GameService.incrementMoveCounter(this.world);
                    }

                    ActivateGemPowerUpSystem.sameColorBombTriggeredThisFrame = true;
                    this.addMatchedComponent(bombGemEntity);

                    let bombGemSpriteLayerSorting = this.world.getComponentData(bombGemEntity, ut.Core2D.LayerSorting);
                    bombGemSpriteLayerSorting.order = 1000;
                    this.world.setComponentData(bombGemEntity, bombGemSpriteLayerSorting);
                    let bombGemPowerUpSpriteLayerSorting = this.world.getComponentData(bombGem.SameColorPowerUpVisual, ut.Core2D.LayerSorting);
                    bombGemPowerUpSpriteLayerSorting.order = 1001;
                    this.world.setComponentData(bombGem.SameColorPowerUpVisual, bombGemPowerUpSpriteLayerSorting);

                    SoundService.play(this.world, "Slash");

                    game.GridService.getGridConfiguration(this.world).GemEntities.forEach(gemEntity => {
                        if (this.world.exists(gemEntity)) {
                            let gem = this.world.getComponentData(gemEntity, game.Gem);
                            let gemPosition = game.GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
                            if (gem.GemType == otherGem.GemType) {
                                this.destroyGem(grid, gemEntity, false);

                                let startPosition = GridService.getGridToWorldPosition(grid, bombGemPosition.x, bombGemPosition.y);
                                let endPosition = GridService.getGridToWorldPosition(grid, gemPosition.x, gemPosition.y);
                                this.spawnDestroyLaserAnimation(startPosition.x, startPosition.y, endPosition.x, endPosition.y);
                            }
                        }
                    });
                }
            }
            else if (gem1.PowerUp != GemPowerUpTypes.None && gem2.PowerUp != GemPowerUpTypes.None) {
                // If two power ups are swaped together, they are both triggered.
                this.addMatchedComponent(swapedGemEntities[0]);
                this.addMatchedComponent(swapedGemEntities[1]);
            }
        }

        activateMatchedBombs(grid: game.GridConfiguration) {
            let bombGems: game.Gem[] = new Array();
            this.world.forEach([ut.Entity, game.Gem, game.Matched], (entity, gem, matched) => {
                if (gem.PowerUp != GemPowerUpTypes.None && gem.PowerUp != GemPowerUpTypes.SameColor) {
                    bombGems.push(game.GemService.getGem(this.world, grid, gem.CellHashKey));
                }
            });

            let bombTriggered = false;
            bombGems.forEach(gem => {
                bombTriggered = this.activateBomb(grid, gem) || bombTriggered;
            });

            if (bombTriggered && grid.FrozenGridTimer < 0.3 && !ActivateGemPowerUpSystem.sameColorBombTriggeredThisFrame) {
                grid.FrozenGridTimer = 0.35;
                this.world.setConfigData(grid);
            }
        }

        activateBomb(grid: game.GridConfiguration, gem: game.Gem): boolean {
            let gemPosition = game.GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
            switch (gem.PowerUp) {
                case GemPowerUpTypes.Row: {
                    for (let i = 0; i < grid.Width; i++) {
                        this.destroyGem(grid, game.GemService.getGemEntityAtPosition(this.world, grid, i, gemPosition.y), true);
                    }
                    let startPosition = GridService.getGridToWorldPosition(grid, 0, gemPosition.y);
                    let endPosition = GridService.getGridToWorldPosition(grid, grid.Width - 1, gemPosition.y);
                    this.spawnDestroyLineAnimation(startPosition.x, startPosition.y, endPosition.x, endPosition.y);
                    SoundService.play(this.world, "Slash");
                    return true;
                }
                case GemPowerUpTypes.Column: {
                    for (let j = 0; j < grid.Height; j++) {
                        this.destroyGem(grid, game.GemService.getGemEntityAtPosition(this.world, grid, gemPosition.x, j), true);
                    }
                    let startPosition = GridService.getGridToWorldPosition(grid, gemPosition.x, 0);
                    let endPosition = GridService.getGridToWorldPosition(grid, gemPosition.x, grid.Height - 1);
                    this.spawnDestroyLineAnimation(startPosition.x, startPosition.y, endPosition.x, endPosition.y);
                    SoundService.play(this.world, "Slash");
                    return true;
                }
                case GemPowerUpTypes.Square: {
                    this.destroyGem(grid, game.GemService.getNeighborGem(this.world, grid, gem, -1, 1), true);
                    this.destroyGem(grid, game.GemService.getNeighborGem(this.world, grid, gem, -1, 0), true);
                    this.destroyGem(grid, game.GemService.getNeighborGem(this.world, grid, gem, -1, -1), true);
                    this.destroyGem(grid, game.GemService.getNeighborGem(this.world, grid, gem, 0, 1), true);
                    this.destroyGem(grid, game.GemService.getNeighborGem(this.world, grid, gem, 0, -1), true);
                    this.destroyGem(grid, game.GemService.getNeighborGem(this.world, grid, gem, 1, 1), true);
                    this.destroyGem(grid, game.GemService.getNeighborGem(this.world, grid, gem, 1, 0), true);
                    this.destroyGem(grid, game.GemService.getNeighborGem(this.world, grid, gem, 1, -1), true);
                    let startPositionLeft = GridService.getGridToWorldPosition(grid, gemPosition.x - 1, gemPosition.y + 1);
                    let endPositionLeft = GridService.getGridToWorldPosition(grid, gemPosition.x - 1, gemPosition.y - 1);
                    let startPositionRight = GridService.getGridToWorldPosition(grid, gemPosition.x + 1, gemPosition.y + 1);
                    let endPositionRight = GridService.getGridToWorldPosition(grid, gemPosition.x + 1, gemPosition.y - 1);
                    this.spawnDestroyLineAnimation(startPositionLeft.x, startPositionLeft.y + 10, endPositionLeft.x + 5, endPositionLeft.y - 10);
                    this.spawnDestroyLineAnimation(startPositionRight.x, startPositionRight.y + 10, endPositionRight.x - 5, endPositionRight.y - 10);
                    SoundService.play(this.world, "Slash");
                    return true;
                }
                case GemPowerUpTypes.DiagonalCross: {
                    for (let i = 0; i < grid.Width; i++) {
                        this.destroyGem(grid, game.GemService.getGemEntityAtPosition(this.world, grid, i, gemPosition.y + gemPosition.x - i), true);
                    }
                    for (let i = 0; i < grid.Width; i++) {
                        this.destroyGem(grid, game.GemService.getGemEntityAtPosition(this.world, grid, i, gemPosition.y + i - gemPosition.x), true);
                    }
                    let startPosition1 = this.findDiagonalEnd(grid, new Vector2(gemPosition.x, gemPosition.y), new Vector2(-1, -1));
                    let endPosition1 = this.findDiagonalEnd(grid, new Vector2(gemPosition.x, gemPosition.y), new Vector2(1, 1));
                    let startPosition2 = this.findDiagonalEnd(grid, new Vector2(gemPosition.x, gemPosition.y), new Vector2(-1, 1));
                    let endPosition2 = this.findDiagonalEnd(grid, new Vector2(gemPosition.x, gemPosition.y), new Vector2(1, -1));
                    this.spawnDestroyLineAnimation(startPosition1.x, startPosition1.y, endPosition1.x, endPosition1.y);
                    this.spawnDestroyLineAnimation(startPosition2.x, startPosition2.y, endPosition2.x, endPosition2.y);
                    SoundService.play(this.world, "Slash");
                    return true;
                }
            }

            return false;
        }

        findDiagonalEnd(grid: GridConfiguration, current: Vector2, direction: Vector2): Vector2 {
            if (current.x == 0 || current.x == grid.Width - 1 || current.y == 0 || current.y == grid.Height - 1) {
                let worldPositon = GridService.getGridToWorldPosition(grid, current.x, current.y);
                return new Vector2(worldPositon.x, worldPositon.y);
            }
            else {
                return this.findDiagonalEnd(grid, new Vector2(current.x + direction.x, current.y + direction.y), direction);
            }
        }

        destroyGem(grid: game.GridConfiguration, gemEntity: ut.Entity, triggerBomb: boolean) {
            let gem = game.GemService.getGemFromEntity(this.world, gemEntity);
            if (gem != null && gem.GemType != GemTypes.Egg && gem.GemType != GemTypes.ColorBomb) {

                let hadComponent = this.world.hasComponent(gemEntity, game.Matched);
                this.addMatchedComponent(gemEntity);

                if (triggerBomb && !hadComponent) {
                    this.activateBomb(grid, gem);
                }
            }
        }

        addMatchedComponent(gemEntity: ut.Entity) {
            if (!this.world.hasComponent(gemEntity, game.Matched)) {
                let matched = new game.Matched();
                matched.CreatedPowerUp = game.GemPowerUpTypes.None;
                this.world.addComponentData(gemEntity, matched);
            }
        }

        spawnDestroyLineAnimation(startPositionX: number, startPositionY: number, endPositionX: number, endPositionY: number) {
            let entity = ut.EntityGroup.instantiate(this.world, "game.DestroyLineAnimation")[0];
            let destroyLineAnimation = this.world.getComponentData(entity, game.DestroyLineAnimation);
            destroyLineAnimation.StartPositionX = startPositionX;
            destroyLineAnimation.StartPositionY = startPositionY;
            destroyLineAnimation.EndPositionX = endPositionX;
            destroyLineAnimation.EndPositionY = endPositionY;
            this.world.setComponentData(entity, destroyLineAnimation);
        }

        spawnDestroyLaserAnimation(startPositionX: number, startPositionY: number, endPositionX: number, endPositionY: number) {
            let entity = ut.EntityGroup.instantiate(this.world, "game.DestroyLaserAnimation")[0];
            let destroyLaserAnimation = this.world.getComponentData(entity, game.DestroyLaserAnimation);
            destroyLaserAnimation.StartPositionX = startPositionX;
            destroyLaserAnimation.StartPositionY = startPositionY;
            destroyLaserAnimation.EndPositionX = endPositionX;
            destroyLaserAnimation.EndPositionY = endPositionY;
            this.world.setComponentData(entity, destroyLaserAnimation);
        }
    }
}
