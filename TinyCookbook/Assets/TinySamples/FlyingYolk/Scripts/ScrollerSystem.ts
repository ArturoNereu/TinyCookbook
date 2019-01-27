namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(ut.Core2D.TransformLocalPosition, game.Scroller)
  export class ScrollerSystem extends ut.ComponentSystem {

    /**
     * Scrolls objects based on the global game speed
     */
    OnUpdate(): void {
      let gameConfig = this.world.getConfigData(game.GameConfig);
      this.world.forEach([ut.Core2D.TransformLocalPosition, game.Scroller],
        (position, scroller) => {
          let p = position.position;
          p.x -= gameConfig.currentScrollSpeed;
          position.position = p;
        });
    }
  }
}
