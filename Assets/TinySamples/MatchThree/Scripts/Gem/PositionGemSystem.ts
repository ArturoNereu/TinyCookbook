
namespace game {

    @ut.executeAfter(ut.Shared.UserCodeEnd)
    @ut.executeBefore(ut.Shared.RenderingFence)
    export class PositionGemSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            let grid = game.GridService.getGridConfiguration(this.world);
            this.world.forEach([game.Gem, ut.Core2D.TransformLocalPosition], (gem, transformLocalPosition) => {
                if (!gem.IsFalling && !gem.IsSwapping) {
                    transformLocalPosition.position = game.GemService.getGemWorldPosition(grid, gem);
                }
            });
        }
    }
}
