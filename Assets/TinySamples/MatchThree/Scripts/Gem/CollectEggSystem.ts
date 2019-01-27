
namespace game {

    @ut.executeAfter(game.ActivateGemPowerUpSystem)
    export class CollectEggSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game) {
                return;
            }

            let levelEntity = GameService.getCurrentLevelEntity(this.world);
            if (!this.world.hasComponent(levelEntity, LevelEggObjective)) {
                return;
            }

            let levelEggObjective = this.world.getComponentData(levelEntity, LevelEggObjective)

            let matchedCount = 0;
            this.world.forEach([ut.Entity, game.Gem, game.Matched], (entity, gem, matched) => {
                matchedCount++;
            });

            let gemSwapCount = 0;
            this.world.forEach([ut.Entity, game.Gem, game.GemSwap], (entity, gem, gemSwap) => {
                gemSwapCount++;
            });

            let grid = game.GridService.getGridConfiguration(this.world);
            let collectedEggCount = 0;
            if (matchedCount == 0 && gemSwapCount == 0) {
                this.world.forEach([ut.Entity, game.Gem, ut.Core2D.TransformLocalPosition], (entity, gem, transformPosition) => {
                    let gemPosition = GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
                    if (gem.GemType == GemTypes.Egg && gemPosition.y == 0 && !gem.IsFalling && !gem.IsSwapping) {

                        GemService.addMatchedComponent(this.world, entity, false);

                        levelEggObjective.CollectedEggs++;
                        this.world.setComponentData(levelEntity, levelEggObjective);

                        // Spawn gain egg currency particle
                        {
                            let gameUI = this.world.getComponentData(this.world.getEntityByName("GameUI"), game.GameUI);
                            let destinationPosition = ut.Core2D.TransformService.computeWorldPosition(this.world, gameUI.ImageObjectiveEgg);

                            let collectedEggEntity = ut.EntityGroup.instantiate(this.world, "game.CollectedEgg")[0];

                            let collectedEggCurrency = this.world.getComponentData(collectedEggEntity, game.CollectedCurrency);
                            collectedEggCurrency.StartPosition = transformPosition.position;
                            collectedEggCurrency.MidPosition = new Vector3(-100, 0, 0);
                            collectedEggCurrency.EndPosition = destinationPosition;
                            collectedEggCurrency.StartDelay = collectedEggCount * 0.2;
                            this.world.setComponentData(collectedEggEntity, collectedEggCurrency);
    
                            let collectedEggTransformPosition = this.world.getComponentData(collectedEggEntity, ut.Core2D.TransformLocalPosition);
                            collectedEggTransformPosition.position = transformPosition.position;
                            this.world.setComponentData(collectedEggEntity, collectedEggTransformPosition);
                        }

                        collectedEggCount++;
                    }
                });
            }
        }
    }
}
