
namespace game {

    export class GameService {

        static gameStateEntity: ut.Entity;

        static clear() {
            this.gameStateEntity = new ut.Entity();
        };

        static getGameState(world: ut.World): game.GameState {

            if (!world.exists(this.gameStateEntity)) {
                this.gameStateEntity = world.getEntityByName("Game");
                if (!world.exists(this.gameStateEntity)) {
                    this.gameStateEntity = null;
                    return null;
                }
            }

            return world.getComponentData(this.gameStateEntity, game.GameState);
        }

        static getCurrentLevelEntity(world: ut.World): ut.Entity {
            let currentLevelID = this.getGameState(world).CurrentLevelID;
            if (currentLevelID <= 0) {
                return null;
            }

            return this.getLevelEntity(world, currentLevelID);
        }

        static getCurrentLevel(world: ut.World): game.Level {
            let levelEntity = this.getCurrentLevelEntity(world);
            if (!world.exists(levelEntity)) {
                return null;
            }

            return world.getComponentData(levelEntity, game.Level);
        }

        static getLevelEntity(world: ut.World, levelID: number): ut.Entity {
            return world.getEntityByName("Level" + levelID);
        }

        static getLevel(world: ut.World, levelID: number): game.Level {
            let levelEntity = this.getLevelEntity(world, levelID);
            return world.getComponentData(levelEntity, game.Level);
        }

        static incrementMoveCounter(world: ut.World) {
            let gameState = this.getGameState(world);
            gameState.CurrentMoveCount++;
            world.setComponentData(this.gameStateEntity, gameState);
            this.updateRemainingMovesLabel(world);
        }

        static updateRemainingMovesLabel(world: ut.World) {
            let currentLevel = this.getCurrentLevel(world);
            let maxMoveCount = currentLevel == null ? 0 : currentLevel.MaxMoveCount;

            let gameUI = world.getComponentData(world.getEntityByName("GameUI"), game.GameUI);
            let labelRemainingMoves = world.getComponentData(gameUI.LabelRemainingMoves, ut.Text.Text2DRenderer);

            let strRemainingMoveCount = "";
            if (maxMoveCount > 0) {
                let remainingMoveCount = maxMoveCount - this.getGameState(world).CurrentMoveCount;
                strRemainingMoveCount = String(remainingMoveCount);
            }

            labelRemainingMoves.text = strRemainingMoveCount;
            world.setComponentData(gameUI.LabelRemainingMoves, labelRemainingMoves);
        }

        static hasRemainingMoves(world: ut.World): boolean {
            let maxMoveCount = this.getCurrentLevel(world).MaxMoveCount;
            if (maxMoveCount <= 0) {
                return true;
            }

            let remainingMoveCount = maxMoveCount - this.getGameState(world).CurrentMoveCount;
            return remainingMoveCount > 0;
        }

        static isNearDeath(world: ut.World): boolean {
            let maxMoveCount = this.getCurrentLevel(world).MaxMoveCount;
            if (maxMoveCount > 0) {
                let remainingMoveCount = maxMoveCount - this.getGameState(world).CurrentMoveCount;
                return remainingMoveCount <= 3;
            }
            else if (world.hasComponent(GameService.getCurrentLevelEntity(world), game.LevelSurvival)) {
                let levelSurvival = world.getComponentData(GameService.getCurrentLevelEntity(world), game.LevelSurvival);
                let survivalRatio = levelSurvival.SurvivalTimer / levelSurvival.MaxSurvivalTime;
                return survivalRatio < 0.2;
            }
        }

        static isObjectiveCompleted(world: ut.World): boolean {
            if (world.hasComponent(GameService.getCurrentLevelEntity(world), game.LevelPointObjective)) {
                let levelPointObjective = world.getComponentData(GameService.getCurrentLevelEntity(world), game.LevelPointObjective);
                return GameService.getGameState(world).CurrentScore >= levelPointObjective.ScoreObjective;;
            }
            else if (world.hasComponent(GameService.getCurrentLevelEntity(world), game.LevelEggObjective)) {
                let levelEggObjective = world.getComponentData(GameService.getCurrentLevelEntity(world), game.LevelEggObjective);
                return levelEggObjective.CollectedEggs >= levelEggObjective.EggsInGridAtStart + levelEggObjective.EggsToSpawnOnEggCollected;
            }
            else if (world.hasComponent(GameService.getCurrentLevelEntity(world), game.LevelSurvival)) {
                let levelSurvival = world.getComponentData(GameService.getCurrentLevelEntity(world), game.LevelSurvival);
                return GameService.getGameState(world).Time >= levelSurvival.TimeObjective;
            }
        }

        static formatTime(seconds: number): string {
            var result = "";
            var hours = Math.floor(seconds / 3600);
            var minutes = Math.floor((seconds % 3600) / 60);
            var seconds = Math.floor(seconds % 60);

            if (hours > 0) {
                result += "" + hours + ":" + (minutes < 10 ? "0" : "");
            }

            result += "" + minutes + ":" + (seconds < 10 ? "0" : "");
            result += "" + seconds;
            return result;
        }

        static formatNumber(value: number): string {
            return value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, " ");;
        }

        static unloadLevel(world: ut.World) {
            SoundService.playMusic(world);

            let grid = GridService.getGridConfiguration(world);
            game.GridService.clear(world, grid);

            world.forEach([ut.Entity, game.ScrollingObject],
                (entity, scrollingObject) => {
                    ut.Tweens.TweenService.removeAllTweens(world, entity);
                    ut.Core2D.TransformService.destroyTree(world, entity, true);
                });

            world.forEach([ut.Entity, game.Helicopter],
                (entity, helicopter) => {
                    ut.Tweens.TweenService.removeAllTweens(world, entity);
                    ut.Core2D.TransformService.destroyTree(world, entity, true);
                });

            ut.Tweens.TweenService.removeAllTweensInWorld(world);

            ut.EntityGroup.destroyAll(world, "game.Gem");
            ut.EntityGroup.destroyAll(world, "game.Cell");

            let gameUI = world.getEntityByName("GameUI");
            if (world.exists(gameUI)) {
                ut.Core2D.TransformService.destroyTree(world, gameUI);
                ut.EntityGroup.destroyAll(world, "game.GameUI");
            }

            let pauseMenu = world.getEntityByName("PauseMenu");
            if (world.exists(pauseMenu)) {
                ut.Core2D.TransformService.destroyTree(world, pauseMenu);
                ut.EntityGroup.destroyAll(world, "game.PauseMenu");
            }

            let gameOverMenu = world.getEntityByName("GameOverMenu");
            if (world.exists(gameOverMenu)) {
                ut.Core2D.TransformService.destroyTree(world, gameOverMenu);
                ut.EntityGroup.destroyAll(world, "game.GameOverMenu");
            }

            ut.EntityGroup.destroyAll(world, "game.GameScene");
            ut.EntityGroup.destroyAll(world, "game.Dinosaur");
            ut.EntityGroup.destroyAll(world, "game.SurvivalModeTimeline");
            ut.EntityGroup.destroyAll(world, "game.BackgroundNearDeathWarning");

            ut.EntityGroup.destroyAll(world, "game.TutorialHighlight");
            ut.EntityGroup.destroyAll(world, "game.TutorialMatchPointer");
            ut.EntityGroup.destroyAll(world, "game.TutorialEggPointer");
            ut.EntityGroup.destroyAll(world, "game.TutorialSurvivalPointer");
        }

        /**
         * Utility method to enable and disable entities.
         */
        static setEntityEnabled(world: ut.World, entity: ut.Entity, enabled: boolean) {
            let hasDisabledComponent = world.hasComponent(entity, ut.Disabled);
            if (enabled && hasDisabledComponent) {
                world.removeComponent(entity, ut.Disabled);
            }
            else if (!enabled && !hasDisabledComponent) {
                world.addComponent(entity, ut.Disabled);
            }
        }
    }
}
