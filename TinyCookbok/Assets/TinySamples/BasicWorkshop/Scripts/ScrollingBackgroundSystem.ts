
namespace game 
{

    /** New System */
    export class ScrollingBackgroundSystem extends ut.ComponentSystem 
	{    
        OnUpdate():void 
		{
			let dt = ut.Time.deltaTime();

			this.world.forEach([ut.Core2D.TransformLocalPosition, game.ScrollingBackground], (position, scrolling) => 
			{
				let localPosition = position.position;
				
				localPosition.y -= scrolling.speed * dt;
				
				if (localPosition.y < scrolling.threshold) 
					localPosition.y += scrolling.distance;
				
				position.position = localPosition;
			});

        }
    }
}
