/// <reference path="RepeatingBackgroundSystem.ts" />
namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.executeAfter(game.RepeatingBackgroundSystem)
  @ut.requiredComponents(game.Spawner)
  export class SpawnerSystem extends ut.ComponentSystem {

    OnUpdate(): void {
      var pipesToSpawn = [];

      this.world.forEach([game.Spawner],
        (spawner) => {
          if (spawner.paused) {
            return;
          }

          let time = spawner.time;
          let delay = spawner.delay;
          time += this.scheduler.deltaTime();

          if (time > delay) {
            time -= delay;

            // this is to get a non-ref copy of the pipespawner component.  This is not the right way to do this,
            // we need an API for this if we think this is going to be a common operation.
            //@ts-ignore
            pipesToSpawn.push(game.Spawner._fromPtr(spawner._ptr));
          }

          spawner.time = time;
        });

      for (var i = 0; i < pipesToSpawn.length; ++i) {
        var spawner = pipesToSpawn[i];
        let instance = ut.EntityGroup.instantiate(this.world, 'game.Pipes')[0];
        let pipe = ut.Core2D.TransformService.getChild(this.world, instance, 0);
        let transform = new ut.Core2D.TransformLocalPosition(
          new Vector3(spawner.distance,
            (Math.random() * spawner.maxHeight) + spawner.minHeight,
            0));
        if (this.world.hasComponent(pipe, ut.Core2D.TransformLocalPosition))
          this.world.setComponentData(pipe, transform);
        else
          this.world.addComponentData(pipe, transform);
      }
    }
  }

}
