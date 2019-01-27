
namespace game {

    /**
     * Update popup animation on show.
     */
    @ut.executeAfter(ut.Shared.UserCodeEnd)
    @ut.executeBefore(ut.Shared.RenderingFence)
    export class AnimatePopupSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            this.world.forEach([game.Popup],
                (popup) => {

                    if (!popup.HasPlayedInTransition) {
                        popup.HasPlayedInTransition = true;
                        this.updatePopupShowAnimation(popup);
                    }

                    if (this.world.exists(popup.ImageBackground)) {
                        let targetAlpha = popup.TargetFadeAlpha * Math.min(1, Math.max(0, (popup.Timer - popup.DelayIn) / popup.FadeInDuration));
                        let backgroundSpriteRenderer = this.world.getComponentData(popup.ImageBackground, ut.Core2D.Sprite2DRenderer);
                        if (backgroundSpriteRenderer.color.a != targetAlpha) {
                            backgroundSpriteRenderer.color.a = targetAlpha;
                            this.world.setComponentData(popup.ImageBackground, backgroundSpriteRenderer);
                        }
                    }
                    
                    popup.Timer += this.scheduler.deltaTime();
                });
        }

        updatePopupShowAnimation(popup: game.Popup) {
            let transformScale = this.world.getComponentData(popup.PanelContent, ut.Core2D.TransformLocalScale);
            let startScale = new Vector3(0, 0, 1);
            let endScale = new Vector3(1, 1, 1);
            transformScale.scale = startScale;
            this.world.setComponentData(popup.PanelContent, transformScale);

            let scaleContentTween = new ut.Tweens.TweenDesc;
            scaleContentTween.cid = ut.Core2D.TransformLocalScale.cid;
            scaleContentTween.offset = 0;
            scaleContentTween.duration = popup.ScaleInDuration;
            scaleContentTween.func = ut.Tweens.TweenFunc.OutBack;
            scaleContentTween.loop = ut.Core2D.LoopMode.Once;
            scaleContentTween.destroyWhenDone = true;
            scaleContentTween.t = -popup.DelayIn;

            ut.Tweens.TweenService.addTweenVector3(this.world, popup.PanelContent, startScale, endScale, scaleContentTween);

            if (this.world.exists(popup.ImageBackground)) {
                let backgroundSpriteRenderer = this.world.getComponentData(popup.ImageBackground, ut.Core2D.Sprite2DRenderer);
                popup.TargetFadeAlpha = backgroundSpriteRenderer.color.a;
            }
        }
    }
}
