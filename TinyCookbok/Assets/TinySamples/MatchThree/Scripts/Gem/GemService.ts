
namespace game {

    export class GemService {

        static getGemEntity(world: ut.World, grid: game.GridConfiguration, cellHashCode: number) : ut.Entity {
            let gemEntity = grid.GemEntities[cellHashCode];
            if (world.exists(gemEntity)) {
                return gemEntity;
            }
            else {
                return null;
            }
        }

        static getGem(world: ut.World, grid: game.GridConfiguration, cellHashCode: number) : game.Gem {
            let gemEntity = grid.GemEntities[cellHashCode];
            if (world.exists(gemEntity) && world.hasComponent(gemEntity, game.Gem)) {
                return world.getComponentData(gemEntity, game.Gem);
            }
            else {
                return null;
            }
        }

        static getGemFromEntity(world: ut.World, gemEntity: ut.Entity) : game.Gem {
            if (world.exists(gemEntity) && world.hasComponent(gemEntity, game.Gem)) {
                return world.getComponentData(gemEntity, game.Gem);
            }
            else {
                return null;
            }
        }

        static setGem(world: ut.World, grid: game.GridConfiguration, cellHashCode: number, gemEntity: ut.Entity) : void {

            if (gemEntity == null) {
                grid.GemEntities[cellHashCode] = new ut.Entity();
            }
            else {
                grid.GemEntities[cellHashCode] = gemEntity;
            }

            world.setConfigData(grid);
        }

        static createGem(world: ut.World, grid: game.GridConfiguration, cellHashCode: number, possibleGemTypes: number[]): ut.Entity {
            let gemType = possibleGemTypes[Math.floor(Math.random() * possibleGemTypes.length)];
            return this.createGemOfType(world, grid, cellHashCode, gemType);
        }

        static createGemOfType(world: ut.World, grid: game.GridConfiguration, cellHashCode: number, gemType: number): ut.Entity {

            let entity = ut.EntityGroup.instantiate(world, "game.Gem")[0];
            
            let gem = world.getComponentData(entity, game.Gem);
            
            gem.GemType = gemType;
            gem.CellHashKey = cellHashCode;
            world.setComponentData(entity, gem);

            this.setGem(world, grid, cellHashCode, entity)

            let gemName = "";
            if (gemType == GemTypes.Blue) {
                gemName = "Blue";
            }
            else if (gemType == GemTypes.Green) {
                gemName = "Green";
            }
            else if (gemType == GemTypes.Purple) {
                gemName = "Purple";
            }
            else if (gemType == GemTypes.Red) {
                gemName = "Red";
            }
            else if (gemType == GemTypes.Silver) {
                gemName = "Silver";
            }
            else if (gemType == GemTypes.Yellow) {
                gemName = "Yellow";
            }

            let pathGemSprite = "assets/sprites/Gems/" + "Gem_" + gemName;
            let spriteRenderer = world.getComponentData(entity, ut.Core2D.Sprite2DRenderer)
            spriteRenderer.sprite = world.getEntityByName(pathGemSprite + "_Plain");
            world.setComponentData(entity, spriteRenderer);

            let highlightedSpriteRenderer = world.getComponentData(gem.SpriteRendererHighlightGem, ut.Core2D.Sprite2DRenderer);
            highlightedSpriteRenderer.sprite = (world.getEntityByName(pathGemSprite + "_Highlighted"));
            world.setComponentData(gem.SpriteRendererHighlightGem, highlightedSpriteRenderer);

            return entity;
        };

        static deleteGem(world: ut.World, grid: game.GridConfiguration, gemEntity: ut.Entity, gem: game.Gem) {
            let gemHashKey = gem.CellHashKey;

            if (world.hasComponent(gemEntity, game.MatchPossibility)) {
                world.removeComponent(gemEntity, game.MatchPossibility);
            }

            ut.Tweens.TweenService.removeAllTweens(world, gemEntity);
            ut.Core2D.TransformService.destroyTree(world, gemEntity, true);

            this.setGem(world, grid, gemHashKey, null)
        }

        static setGemPowerUp(world: ut.World, gemEntity: ut.Entity, gem: game.Gem, powerUp: game.GemPowerUpTypes) {
            gem.PowerUp = powerUp;
            if (powerUp == game.GemPowerUpTypes.SameColor) {
                this.setSpecialGemType(world, gemEntity, gem, game.GemTypes.ColorBomb);
            }
        }

        static setSpecialGemType(world: ut.World, gemEntity: ut.Entity, gem: game.Gem, gemType: game.GemTypes) {
            let pathGemSprite = "assets/sprites/Gems/";
            if (gemType == game.GemTypes.ColorBomb) {
                gem.GemType = gemType;
                let spriteRenderer = world.getComponentData(gemEntity, ut.Core2D.Sprite2DRenderer)
                spriteRenderer.sprite = world.getEntityByName(pathGemSprite + "Gem_Colorless_Plain_Glow");
                world.setComponentData(gemEntity, spriteRenderer);
                let layerSorting = world.getComponentData(gemEntity, ut.Core2D.LayerSorting)
                layerSorting.order = 9;
                world.setComponentData(gemEntity, layerSorting);
            }
            else if (gemType == game.GemTypes.Egg) {
                gem.GemType = gemType;
                let spriteRenderer = world.getComponentData(gemEntity, ut.Core2D.Sprite2DRenderer)
                spriteRenderer.sprite = world.getEntityByName(pathGemSprite + "Gem_Egg_Plain");
                world.setComponentData(gemEntity, spriteRenderer);
                let spriteRendererHighlight = world.getComponentData(gem.SpriteRendererHighlightGem, ut.Core2D.Sprite2DRenderer)
                spriteRendererHighlight.sprite = world.getEntityByName(pathGemSprite + "Gem_Egg_Highlighted");
                world.setComponentData(gem.SpriteRendererHighlightGem, spriteRendererHighlight); // 
            }
        }

        static addMatchedComponent(world: ut.World, gemEntity: ut.Entity, isMatch: boolean) {
            if (!world.hasComponent(gemEntity, game.Matched)) {
                let matched = new game.Matched();
                matched.CreatedPowerUp = game.GemPowerUpTypes.None;
                matched.IsMatch = isMatch;
                world.addComponentData(gemEntity, matched);
            }
        }

        static getGemEntityAtPosition(world: ut.World, grid: game.GridConfiguration, x: number, y: number): ut.Entity {
            return this.getGemEntity(world, grid, GridService.getCellHashCode(grid, x, y));
        }

        static getGemAtPosition(world: ut.World, grid: game.GridConfiguration, x: number, y: number): game.Gem {
            return this.getGem(world, grid, GridService.getCellHashCode(grid, x, y));
        }

        static getNeighborGem(world: ut.World, grid: game.GridConfiguration, gem: game.Gem, xOffset: number, yOffset: number): ut.Entity {

            let gemPosition = game.GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
            let neighborGemHashKey = game.GridService.getCellHashCode(grid, gemPosition.x + xOffset, gemPosition.y + yOffset);

            return this.getGemEntity(world, grid, neighborGemHashKey);
        }

        static areGemsNeighbor(grid: GridConfiguration, gem1: game.Gem, gem2: game.Gem): boolean {

            let gem1Position = game.GridService.getPositionFromCellHashCode(grid, gem1.CellHashKey);
            let gem2Position = game.GridService.getPositionFromCellHashCode(grid, gem2.CellHashKey);
            return Math.abs(gem1Position.x - gem2Position.x) + Math.abs(gem1Position.y - gem2Position.y) == 1;
        }

        static animateGemFall(world: ut.World, grid: GridConfiguration, gemEntity: ut.Entity, gem: game.Gem, fallHeight: number) {
            if (fallHeight == 0) {
                return;
            }

            let transformPosition = world.getComponentData(gemEntity, ut.Core2D.TransformLocalPosition);
            let targetPosition = game.GemService.getGemWorldPosition(grid, gem);

            let fallDuration = 0;
            for (let i = 1; i <= fallHeight; i++) {
                fallDuration += 0.4 / i;
            }

            let gemTween = new ut.Tweens.TweenDesc;
            gemTween.cid = ut.Core2D.TransformLocalPosition.cid;
            gemTween.offset = 0;
            gemTween.duration = fallDuration;
            gemTween.func = ut.Tweens.TweenFunc.OutBounce;
            gemTween.loop = ut.Core2D.LoopMode.Once;
            gemTween.destroyWhenDone = false;
            gemTween.t = 0.0;

            let tweenEntity = ut.Tweens.TweenService.addTweenVector3(world, gemEntity, transformPosition.position, targetPosition, gemTween);
            
            let gemCallback = new game.GemFallTweenEndCallback;
            gemCallback.GemEntity = gemEntity;
            world.addComponentData(tweenEntity, gemCallback);

            gem.IsFalling = true;
            world.setComponentData(gemEntity, gem);
        }

        static swapGems(world: ut.World, grid: game.GridConfiguration, gemEntity1: ut.Entity, gem1: game.Gem, gemEntity2: ut.Entity, gem2: game.Gem) {
            let gem2HashKey = gem2.CellHashKey;

            this.setGem(world, grid, gem1.CellHashKey, gemEntity2);
            gem2.CellHashKey = gem1.CellHashKey;
            world.setComponentData(gemEntity2, gem2);

            this.setGem(world, grid, gem2HashKey, gemEntity1);
            gem1.CellHashKey = gem2HashKey;
            world.setComponentData(gemEntity1, gem1);
        }

        static animateGemsSwap(world: ut.World, grid: GridConfiguration, gemEntity1: ut.Entity, gem1: game.Gem, gemEntity2: ut.Entity, gem2: game.Gem) {
            this.animateGemSwap(world, grid, gemEntity1, gem1);
            this.animateGemSwap(world, grid, gemEntity2, gem2);
        }

        private static animateGemSwap(world: ut.World, grid: GridConfiguration, gemEntity: ut.Entity, gem: game.Gem) {
            let transformPosition = world.getComponentData(gemEntity, ut.Core2D.TransformLocalPosition);
            let targetPosition = game.GemService.getGemWorldPosition(grid, gem);

            let gemTween = new ut.Tweens.TweenDesc;
            gemTween.cid = ut.Core2D.TransformLocalPosition.cid;
            gemTween.offset = 0;
            gemTween.duration = 0.12;
            gemTween.func = ut.Tweens.TweenFunc.Linear;
            gemTween.loop = ut.Core2D.LoopMode.Once;
            gemTween.destroyWhenDone = false;
            gemTween.t = 0.0;

            let tweenEntity = ut.Tweens.TweenService.addTweenVector3(world, gemEntity, transformPosition.position, targetPosition, gemTween);
            
            let gemCallback = new game.GemSwapTweenEndCallback;
            gemCallback.GemEntity = gemEntity
            world.addComponentData(tweenEntity, gemCallback);

            gem.IsSwapping = true;
            world.setComponentData(gemEntity, gem);
        }

        static getGemWorldPosition(grid: GridConfiguration, gem: game.Gem): Vector3 {
            let gemPosition = game.GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
            let gemWorldPosition = game.GridService.getGridToWorldPosition(grid, gemPosition.x, gemPosition.y);
            return new Vector3(gemWorldPosition.x - 0.5, gemWorldPosition.y + 0.5, 0);
        }

        static getGemParticleColor(world: ut.World, gem: game.Gem) : ut.Core2D.Color {
            switch (gem.GemType) {
                case GemTypes.Blue: {
                    return new ut.Core2D.Color(0/255, 102/255, 255/255, 1);
                }
                case GemTypes.Red: {
                    return new ut.Core2D.Color(236/255, 23/255, 40/255, 1);
                }
                case GemTypes.Green: {
                    return new ut.Core2D.Color(0, 1, 0, 1);
                }
                case GemTypes.Yellow: {
                    return new ut.Core2D.Color(255/255, 198/255, 0/255, 1);
                }
                case GemTypes.Silver: {
                    return new ut.Core2D.Color(199/255, 255/255, 255/255, 1);
                }
                case GemTypes.Purple: {
                    return new ut.Core2D.Color(205/255, 39/255, 255/255, 1);
                }
                default: {
                    return new ut.Core2D.Color(1, 1, 1, 1);
                }
            }
        }
    }
}
