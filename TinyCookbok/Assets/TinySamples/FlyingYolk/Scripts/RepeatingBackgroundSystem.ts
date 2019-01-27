/// <reference path="ScrollerSystem.ts" />
namespace game {

  @ut.executeAfter(game.ScrollerSystem)
  @ut.requiredComponents(ut.Core2D.TransformLocalPosition, game.RepeatingBackground)
  export class RepeatingBackgroundSystem extends ut.ComponentSystem {

    OnUpdate(): void {
      this.world.forEach([ut.Core2D.TransformLocalPosition, game.RepeatingBackground],
        function (transformlocalposition, repeatingbackground) {
          let position = transformlocalposition.position;

          // when this entity reaches a certain threshold jump forward a specific distance
          if (position.x < repeatingbackground.threshold) {
            position.x += repeatingbackground.distance;
          }

          transformlocalposition.position = position;
        });
    }
  }

}
