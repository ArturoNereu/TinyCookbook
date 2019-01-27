
namespace game 
{
    /** New System */
    export class SpawnSystem extends ut.ComponentSystem 
	{
        OnUpdate():void 
		{
			this.world.forEach([game.Spawner], (spawner) => 
			{
				if (spawner.isPaused) 
					return;

				let time = spawner.time;
				let delay = spawner.delay;
				
				time -= ut.Time.deltaTime();

				if (time <= 0) 
				{
					time += delay;
					
					ut.EntityGroup.instantiate(this.world, spawner.spawnedGroup);
				}

				spawner.time = time;
			});
        }
    }
}
