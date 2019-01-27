
namespace game {

    /** 
     * Adjust screen layout to fit any aspect ratio.
     */
    export class FitScreenLayoutSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            let displayInfo = this.world.getConfigData(ut.Core2D.DisplayInfo);
            let aspectRatio = displayInfo.height / displayInfo.width;
            let referenceRatio = 16 / 9;
            let isTallDisplay = aspectRatio > referenceRatio + 0.01;
            let matchWidthOrHeight = isTallDisplay ? 0 : 1;

            // If resolution is taller than 9/16, make UI canvas match the width.
            this.world.forEach([game.CanvasResolutionFitter, ut.UILayout.UICanvas], (resolutionFitter, canvas) => {
                canvas.matchWidthOrHeight = matchWidthOrHeight;

                let referenceHalfSize = 200;
                let halfVerticalSize = isTallDisplay ? aspectRatio * referenceHalfSize / referenceRatio : referenceHalfSize;
                let camera = this.world.getComponentData(canvas.camera, ut.Core2D.Camera2D);
                camera.halfVerticalSize = halfVerticalSize;
            });

            // If resolution is taller than 9/16, zoom out the camera.
            this.world.forEach([game.CameraResolutionFitter, ut.Core2D.Camera2D], (resolutionFitter, camera) => {
                
                if (resolutionFitter.DefaultHalfVerticalSize == 0) {
                    resolutionFitter.DefaultHalfVerticalSize = camera.halfVerticalSize;
                }
                
                let referenceHalfSize = resolutionFitter.DefaultHalfVerticalSize;
                let halfVerticalSize = isTallDisplay ? aspectRatio * referenceHalfSize / referenceRatio : referenceHalfSize;
                camera.halfVerticalSize = halfVerticalSize;
            });
        }
    }
}
