/// <reference path="SpawnerSystem.ts" />
namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.executeAfter(game.SpawnerSystem)
  @ut.requiredComponents(ut.Core2D.Sprite2DRenderer, ut.Core2D.Sprite2DSequence)
  export class SkinSystem extends ut.ComponentSystem {

    OnUpdate(): void {

      let gameConfig = this.world.getConfigData(game.GameConfig);
      let skinConfig = this.world.getConfigData(game.SkinConfig);
      let change: boolean = false;

      // change theme with user keyboard input
      if (ut.Runtime.Input.getKeyUp(ut.Core2D.KeyCode.S)) {
        change = true;
      }

      // change theme every four points
      if (gameConfig.currentScore != 0 && gameConfig.currentScore % 4 == 0) {
        change = true;
      }

      let themeIndex = gameConfig.currentScore % 8;

      // apply the theme change to the game config
      if (themeIndex >= 4) {
        skinConfig.theme = game.SkinType.Night;
      } else {
        skinConfig.theme = game.SkinType.Day;
      }

      this.world.setConfigData(skinConfig);

      // update the theme for reskinnable entities
      // get the skin theme (string value) from the GameManager entity
      let theme = skinConfig.theme;
      let themeName = Object.keys(game.SkinType).filter(value => game.SkinType[value] === theme);

      // Update Sprite2DRenderer sprites
      this.world.forEach([game.Reskinnable, ut.Core2D.Sprite2DRenderer],
        (reskinnable, sprite2drenderer) => {
          if (reskinnable.theme == theme) {
            return;
          }

          let spriteEntity = sprite2drenderer.sprite;
          let imgPath = this.world.getEntityName(spriteEntity);

          // variation of a sprite is contained in another Sprite Atlas
          // e.g. "assets/sprites/Day/bg" and "assets/sprites/Night/bg"
          let path = "assets/sprites/" + themeName + imgPath.substring(imgPath.lastIndexOf('/'));

          sprite2drenderer.sprite = this.world.getEntityByName(path);
        });

      // Update Sprite2DSequence sprites
      this.world.forEach([game.Reskinnable, ut.Core2D.Sprite2DSequence],
        (reskinnable, sprite2dsequence) => {
          if (reskinnable.theme == theme) {
            return;
          }

          let sprites = Array<ut.Entity>();

          sprite2dsequence.sprites.forEach(sprite => {
            let imgPath = this.world.getEntityName(sprite);

            // variation of a sprite is contained in another Sprite Atlas
            // e.g. "assets/sprites/Day/bg" and "assets/sprites/Night/bg"
            let path = "assets/sprites/" + themeName + imgPath.substring(imgPath.lastIndexOf('/'));

            sprites.push(this.world.getEntityByName(path));
          });

          sprite2dsequence.sprites = sprites;
        });
        
      this.world.forEach([game.Reskinnable],
        (reskinnable) => {
          reskinnable.theme = theme;
        });
    }
  }

}