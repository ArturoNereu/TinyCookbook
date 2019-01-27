
namespace game {

export class RenderModeSelector extends ut.ComponentSystem {
      
      OnUpdate():void {
        var modeEntity = this.world.getEntityByName("RenderMode");
        if (modeEntity != null && this.world.exists(modeEntity)) {
          this.world.usingComponentData(modeEntity, [game.CurrentRenderMode], (currentRenderMode) => {
            if (currentRenderMode.apply) {
              currentRenderMode.apply = false;
              var di = this.world.getConfigData(ut.Core2D.DisplayInfo);
              if (di.renderMode != currentRenderMode.desiredMode) {
                di.renderMode = currentRenderMode.desiredMode;
                this.world.setConfigData(di);
              }
            }
          });
        }
      }
  }
}
