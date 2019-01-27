
namespace game {

    export class UpdateSurvivalModeSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.Game || GridService.isGridFrozen(this.world)) {
                return;
            }

            let currentLevelEntity = GameService.getCurrentLevelEntity(this.world);
            if (this.world.hasComponent(currentLevelEntity, game.LevelSurvival)) {
                let levelSurvival = this.world.getComponentData(currentLevelEntity, game.LevelSurvival);
                let survivalRatio = -1;
                levelSurvival.SurvivalTimer -= levelSurvival.TimeDepleteRate * this.scheduler.deltaTime();
                levelSurvival.SurvivalTimer = Math.max(0, Math.min(levelSurvival.MaxSurvivalTime, levelSurvival.SurvivalTimer));
                survivalRatio = levelSurvival.SurvivalTimer / levelSurvival.MaxSurvivalTime;
                this.world.setComponentData(currentLevelEntity, levelSurvival);
    
                if (survivalRatio == -1) {
                    return;
                }
    
                let survivalTimeline = this.world.getComponentData(this.world.getEntityByName("SurvivalModeTimeline"), game.SurvivalModeTimeline);
    
                let transformPositionDinosaur = this.world.getComponentData(survivalTimeline.DinosaurCursor, ut.Core2D.TransformLocalPosition);
                transformPositionDinosaur.position.x = survivalRatio * survivalTimeline.Width - survivalTimeline.Width / 2;
                this.world.setComponentData(survivalTimeline.DinosaurCursor, transformPositionDinosaur);
    
                let transformScaleFilling = this.world.getComponentData(survivalTimeline.ContainerFilling, ut.Core2D.TransformLocalScale);
                transformScaleFilling.scale.x = survivalRatio;
                this.world.setComponentData(survivalTimeline.ContainerFilling, transformScaleFilling);
            }
        }
    }
}
