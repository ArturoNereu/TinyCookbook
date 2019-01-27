/// <reference path="ActivateGemPowerUpSystem.ts" />
namespace game {

    /**
     * Update the player's score after a gem match.
     */
    @ut.executeAfter(game.ActivateGemPowerUpSystem)
    export class UpdateScoreSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game || GridService.isGridFrozen(this.world)) {
                return;
            }

            let matchedGemCount = 0;
            let gemPositionSum = new Vector2();
            this.world.forEach([ut.Entity, game.Gem, game.Matched, ut.Core2D.TransformLocalPosition], (entity, gemToDestroy, matched, transformPosition) => {
                matchedGemCount++;
                gemPositionSum.x += transformPosition.position.x;
                gemPositionSum.y += transformPosition.position.y;
            });

            if (matchedGemCount <= 0) {
                return;
            }

            // Update current score.
            let gameState = game.GameService.getGameState(this.world);
            let scoreUnitCount = Math.max(1, matchedGemCount - 2);
            let scoreGain = scoreUnitCount * 25
            gameState.CurrentScore += scoreGain;
            this.world.setComponentData(game.GameService.gameStateEntity, gameState);

            // Spawn floating score gain label.
            gemPositionSum.x = gemPositionSum.x / matchedGemCount;
            gemPositionSum.y = gemPositionSum.y / matchedGemCount;
            this.spawnScoreGainLabel(gemPositionSum, scoreUnitCount, scoreGain);

            // Update survival mode timer.
            let levelEntity = game.GameService.getCurrentLevelEntity(this.world);
            if (this.world.hasComponent(levelEntity, game.LevelSurvival)) {
                let levelSurvival = this.world.getComponentData(levelEntity, game.LevelSurvival);

                let difficultyRatio = Math.min(1, gameState.Time / levelSurvival.DifficulyRampUpTime);
                let timeGain = levelSurvival.EndTimeGainByMatch + (1 - difficultyRatio) * (levelSurvival.StartTimeGainByMatch - levelSurvival.EndTimeGainByMatch);
                levelSurvival.SurvivalTimer += scoreUnitCount * timeGain;
                levelSurvival.SurvivalTimer = Math.max(0, Math.min(levelSurvival.MaxSurvivalTime, levelSurvival.SurvivalTimer));
                this.world.setComponentData(levelEntity, levelSurvival);
            }
        }

        spawnScoreGainLabel(position: Vector2, scoreUnitCount: number, scoreGain: number) {
            let scoreGainEntity = ut.EntityGroup.instantiate(this.world, "game.ScoreGainLabel")[0];
            let scoreGainTransform = this.world.getComponentData(scoreGainEntity, ut.Core2D.TransformLocalPosition);
            scoreGainTransform.position.x = position.x;
            scoreGainTransform.position.y = position.y + 26;
            this.world.setComponentData(scoreGainEntity, scoreGainTransform);

            let label = this.world.getComponentData(scoreGainEntity, ut.Text.Text2DRenderer);
            label.text = String(scoreGain);
            this.world.setComponentData(scoreGainEntity, label);

            let transformScale = this.world.getComponentData(scoreGainEntity, ut.Core2D.TransformLocalScale);
            let scale = Math.min(4, 1.8 + (scoreUnitCount - 1) * 0.12);
            transformScale.scale = new Vector3(scale, scale, 1);
            this.world.setComponentData(scoreGainEntity, transformScale);
        }
    }
}
