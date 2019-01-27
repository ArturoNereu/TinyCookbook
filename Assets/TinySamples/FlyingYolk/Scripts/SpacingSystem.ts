namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(game.Spacing)
  @ut.optionalComponents(ut.Core2D.TransformLocalPosition)
  export class PipeSpacingSystem extends ut.ComponentSystem {

    OnUpdate(): void {
      this.world.forEach([game.Spacing],
         (spacing) => {
          // system responsible for setting the spacing between the pipes
          // this can be manipulated when spawing the pipe, or by another system in real time to create moving pipes
          let topPosition = this.world.getComponentData(spacing.top, ut.Core2D.TransformLocalPosition);
          let botPosition = this.world.getComponentData(spacing.bottom, ut.Core2D.TransformLocalPosition);

          topPosition.position = new Vector3(0, spacing.spacing * 0.5, 0);
          botPosition.position = new Vector3(0, -spacing.spacing * 0.5, 0);

          this.world.setComponentData(spacing.top, topPosition);
          this.world.setComponentData(spacing.bottom, botPosition);
        });
    }
  }

}