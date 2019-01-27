/// <reference path="NumberTextRenderingSystem.ts" />
namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.executeBefore(game.NumberTextRenderingSystem)
  @ut.requiredComponents(game.NumberTextRenderer, game.GameConfigTextValue)
  export class GameNumberTextValueSystem extends ut.ComponentSystem {

    /**
     * Sets the value of the `NumberTextRenderer` based on a property of the `GameConfig`
     * 
     * @note this relies some TypeScript/JavaScript magic to work
     */
    OnUpdate(): void {
      let gameConfig = this.world.getConfigData(game.GameConfig);
      this.world.forEach([ut.Entity, game.NumberTextRenderer, game.GameConfigTextValue],
        (entity, renderer, value) => {
          // assign the value based on the `GameConfig` property by name
          renderer.value = gameConfig[value.key];
        });
    }
  }

}
