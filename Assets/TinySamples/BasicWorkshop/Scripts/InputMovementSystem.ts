
namespace game 
{
    /** New System */
    @ut.executeAfter(ut.Shared.InputFence)
    export class InputMovementSystem extends ut.ComponentSystem 
	{
        OnUpdate():void 
		{
			let dt = ut.Time.deltaTime();

			this.world.forEach([game.MoveSpeed, game.MoveWithInput, game.Boundaries, ut.Core2D.TransformLocalPosition], (speed, tag, bounds, position) => 
			{
				let localPosition = position.position;

				if(ut.Runtime.Input.getKey(ut.Core2D.KeyCode.W))
					localPosition.y += speed.speed * dt;
				if(ut.Runtime.Input.getKey(ut.Core2D.KeyCode.S))
					localPosition.y -= speed.speed * dt;
				if(ut.Runtime.Input.getKey(ut.Core2D.KeyCode.A))
					localPosition.x -= speed.speed * dt;
				if(ut.Runtime.Input.getKey(ut.Core2D.KeyCode.D))
					localPosition.x += speed.speed * dt;
				
				this.ProcessTouchInput(localPosition, speed.speed * dt);

				localPosition.x = Math.max(bounds.minX, Math.min(bounds.maxX, localPosition.x));
				localPosition.y = Math.max(bounds.minY, Math.min(bounds.maxY, localPosition.y));

				position.position = localPosition;
			});

		}
		
		ProcessTouchInput(position: Vector3, speed):void
		{
			if (ut.Core2D.Input.isTouchSupported()) {
				if (ut.Core2D.Input.touchCount() > 0) {
					let touch: ut.Core2D.Touch = ut.Core2D.Input.getTouch(0);
					let player = this.world.getEntityByName("Player");
					let playerWorldPos = ut.Core2D.TransformService.computeWorldPosition(this.world, player);
					let transPos = ut.Core2D.TransformService.worldToWindow(this.world, this.world.getEntityByName("Camera"), playerWorldPos, new Vector2(600,800));

					if (touch.x >= transPos.x){
						position.x += speed;
					}
					else if (touch.x < transPos.x){
						position.x -= speed;
					}
					if (touch.y >= transPos.y){
						position.y += speed;
					}
					else if (touch.y < transPos.y){
						position.y -= speed;
					}

					if (touch.phase == ut.Core2D.TouchState.Moved) {
						//console.log("moved: " + ut.Core2D.Input.touchCount());
					}
					else if (touch.phase == ut.Core2D.TouchState.Ended) {
						//console.log("ended: " + ut.Core2D.Input.touchCount());
					}
					else if (touch.phase == ut.Core2D.TouchState.Stationary) {
						//console.log("holding: " + ut.Core2D.Input.touchCount());
					}
					else if (touch.phase == ut.Core2D.TouchState.Canceled) {
						//console.log("cancelled: " + ut.Core2D.Input.touchCount());
					}
					else if (touch.phase == ut.Core2D.TouchState.Began) {
						//console.log("began: " + ut.Core2D.Input.touchCount());
					}
					else {
						console.log("NO TOUCH STATE!!!");
					}
				}
			}
			else {
				//console.log("TOUCH IS DISABLED!!!");
			}
		}
    }
}
