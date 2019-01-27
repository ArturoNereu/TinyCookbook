
namespace game {

    export class InputService {
        
        static getPointerWorldPosition(world: ut.World, cameraEntity: ut.Entity): Vector3 {
            let displayInfo = world.getConfigData(ut.Core2D.DisplayInfo);
            let displaySize = new Vector2(displayInfo.width, displayInfo.height);
            let inputPosition = ut.Runtime.Input.getInputPosition();
            return ut.Core2D.TransformService.windowToWorld(world, cameraEntity, inputPosition, displaySize);
        }
    }
}
