
namespace game {

    export class ChangeSkinSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            let currentSkin = SkinService.getCurrentSkin(this.world);

            // Update sprite renderer sprites if the current skin changed.
            this.world.forEach([game.SpriteRendererSkin, ut.Core2D.Sprite2DRenderer],
                (spriteRendererSkin, spriteRenderer) => {
                    if (spriteRendererSkin.CurrentSkinIndex != currentSkin) {
                        // Set sprite.
                        if (spriteRendererSkin.SpritePath.length > 0) {
                            let spritePath = "assets/sprites/" + spriteRendererSkin.SpritePath;
                            while (spritePath.indexOf("[Skin]") > -1) {
                                spritePath = spritePath.replace("[Skin]", SkinService.getCurrentSkinName(this.world));
                            }
                            spriteRenderer.sprite = this.world.getEntityByName(spritePath);
                        }

                        // Set solor by skin.
                        spriteRendererSkin.SkinColorInfo.forEach(skinInfo => {
                            if (skinInfo.Skin == currentSkin) {
                                spriteRenderer.color = skinInfo.Color;
                            }
                        });

                        spriteRendererSkin.CurrentSkinIndex = currentSkin;
                    }
                });

            // Update animation sprite sequences if the current skin changed.
            this.world.forEach([game.SpriteSequenceSkin, ut.Core2D.Sprite2DSequence],
                (spriteSequenceSkin, spriteSequence) => {
                    if (spriteSequenceSkin.CurrentSkinIndex != currentSkin) {
                        // Set sprite.
                        let sprites: ut.Entity[] = new Array();
                        for (let i = 0; i < spriteSequenceSkin.SpritePaths.length; i++) {
                            let spritePath = "assets/sprites/" + spriteSequenceSkin.SpritePaths[i];
                            while (spritePath.indexOf("[Skin]") > -1) {
                                spritePath = spritePath.replace("[Skin]", SkinService.getCurrentSkinName(this.world));
                            }
                            let sprite = this.world.getEntityByName(spritePath);
                            sprites.push(sprite);
                        }

                        spriteSequence.sprites = sprites;
                        spriteSequenceSkin.CurrentSkinIndex = currentSkin;
                    }
                });
        }
    }
}
