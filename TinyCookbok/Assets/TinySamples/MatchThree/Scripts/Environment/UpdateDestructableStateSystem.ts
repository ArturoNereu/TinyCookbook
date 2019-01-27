
namespace game {

    export class UpdateDestructableStateSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            this.world.forEach([game.Destructible, ut.Core2D.Sprite2DRenderer],
                (destructible, spriteRenderer) => {
                    if (destructible.CurrentState != destructible.LastState) {
                        let spritePath = "assets/sprites/" + destructible.SpriteStates[destructible.CurrentState];
                        while (spritePath.indexOf("[Skin]") > -1) {
                            spritePath = spritePath.replace("[Skin]", SkinService.getCurrentSkinName(this.world));
                        }
                        spriteRenderer.sprite = this.world.getEntityByName(spritePath);
                        destructible.LastState = destructible.CurrentState;
                    }
                });
        }
    }
}
