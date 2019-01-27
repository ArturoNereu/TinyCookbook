namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(ut.Core2D.TransformNode)
  @ut.requiredComponents(ut.Core2D.TransformLocalPosition)
  @ut.requiredComponents(game.AutoDestroy)
  export class AutoDestroySystem extends ut.ComponentSystem {

    OnUpdate(): void {
      this.world.forEach([ut.Core2D.TransformNode, ut.Core2D.TransformLocalPosition, game.AutoDestroy],
        (transformnode, transformlocalposition, autodestroy) => {
          let position = transformlocalposition.position;

          if (position.x < autodestroy.threshold) {
            // ut.Core2D.TransformService.destroyTreeDeferred(this.world, ecb, transformnode.parent, true);
          }
        });
    }
  }
}
