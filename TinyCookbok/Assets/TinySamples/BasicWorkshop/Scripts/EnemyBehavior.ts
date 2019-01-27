
namespace game 
{
    /** New Filter */
    export class EnemyBehaviorFilter extends ut.EntityFilter 
	{
        entity: ut.Entity;
        position: ut.Core2D.TransformLocalPosition;
		tag: game.EnemyTag;
		speed: game.MoveSpeed;
		speedChange: game.ChangeOverTime;
		bounds: game.Boundaries;
    }

    /** New Behaviour */
    export class EnemyBehavior extends ut.ComponentBehaviour 
	{

        data: EnemyBehaviorFilter;

        OnEntityEnable():void 
		{
            let totalTime = ut.Time.time();
			let newSpeed = this.data.speed.speed + (this.data.speedChange.changePerSecond * totalTime);
			
			this.data.speed.speed = newSpeed;
			
			let randomX = getRandom(this.data.bounds.minX, this.data.bounds.maxX);
			let newPos = new Vector3(randomX, this.data.bounds.maxY, 0);
			
			this.data.position.position = newPos;

			console.log("enemy initialized. Speed: " + newSpeed);
        }
        
        OnEntityUpdate():void 
		{
            let localPosition = this.data.position.position;
			localPosition.y -= this.data.speed.speed * ut.Time.deltaTime();

			this.data.position.position = localPosition;

			if(localPosition.y <= this.data.bounds.minY)	
				//this.world.addComponent(this.entity, ut.Disabled);
				this.world.destroyEntity(this.data.entity);
        }
    }

	function getRandom(min, max) 
	{
		return Math.random() * (max - min + 1) + min;
	}
}
