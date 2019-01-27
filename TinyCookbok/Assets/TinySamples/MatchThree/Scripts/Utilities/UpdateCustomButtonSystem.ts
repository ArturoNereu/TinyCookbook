
namespace game {

    /**
     * Update the sprite and other visual elements of buttons depending on their current state.
     */
    export class UpdateCustomButtonSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            let deltaTime = this.scheduler.deltaTime();
            this.world.forEach([game.CustomButton, ut.Core2D.Sprite2DRenderer, ut.UIControls.MouseInteraction],
                (button, spriteRenderer, mouseInteraction) => {

                    button.IsPressed = mouseInteraction.down && button.IsInteractable;
                    button.JustDown = !button.LastDown && mouseInteraction.down && button.IsInteractable;
                    button.JustUp = button.LastDown && !mouseInteraction.down && button.IsInteractable;
                    button.JustClicked = mouseInteraction.clicked && button.IsInteractable;

                    if (button.IsPressed) {
                        button.TimePressed += deltaTime;
                    }

                    if (button.JustDown || button.JustUp || mouseInteraction.over != button.IsPointerOver || button.LastIsInteractable != button.IsInteractable) {
                        button.LastIsInteractable = button.IsInteractable;

                        let sprite = button.DefaultSprite;
                        if (!button.IsInteractable) {
                            sprite = button.DisabledSprite;
                        }
                        else if (mouseInteraction.over && button.IsPressed) {
                            sprite = button.PressedSprite;
                        }
                        else if (mouseInteraction.over) {
                            sprite = button.HoveredSprite;
                        }
                        
                        if (this.world.exists(sprite)) {
                            spriteRenderer.sprite = sprite;
                        }

                        if (this.world.exists(button.ContentToOffsetOnPress) && button.IsInteractable) {
                            let contentRectTransform = this.world.getComponentData(button.ContentToOffsetOnPress, ut.UILayout.RectTransform);
                            if (!button.ContentDefautPositionIsSet) {
                                button.ContentDefautPositionIsSet = true;
                                button.ContentDefautPositionY = contentRectTransform.anchoredPosition.y;
                            }
                            let contentPosition = contentRectTransform.anchoredPosition;
                            contentPosition.y = button.ContentDefautPositionY + (mouseInteraction.over && button.IsPressed ? button.PressedOffsetY : 0);
                            contentRectTransform.anchoredPosition = contentPosition;
                            this.world.setComponentData(button.ContentToOffsetOnPress, contentRectTransform);
                        }
                    }

                    button.IsPointerOver = mouseInteraction.over;
                    button.LastDown = mouseInteraction.down;
                    button.LastOver = mouseInteraction.over;
                });
        }
    }
}
