
namespace game {

    /**
     * When the dinosaur moves, it doesn't. Instead, every active ScrollingObject do.
     */
    export class MoveScrollingObjectSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            if (!(GameService.getGameState(this.world).GameStateType == game.GameStateTypes.Game
                || GameService.getGameState(this.world).GameStateType == game.GameStateTypes.Paused
                || GameService.getGameState(this.world).GameStateType == game.GameStateTypes.GameOver)) {
                return;
            }

            let deltaTime = this.scheduler.deltaTime();
            let dinoMoveAmount = 0;

            // Check if dinosaur is moving.
            if (GameService.getGameState(this.world).GameStateType != game.GameStateTypes.GameOver || GameService.isObjectiveCompleted(this.world)) {
                this.world.forEach([ut.Entity, game.Dinosaur], (entity, dinosaur) => {
    
                    if (this.world.hasComponent(entity, game.DinosaurAttack)) {
                        return;
                    }
    
                    let walkTimer = dinosaur.WalkTimer;
                    walkTimer += deltaTime;
                    dinosaur.WalkTimer = walkTimer;
                    let animTime = walkTimer % 2;
    
                    if (animTime < 0.5 || (animTime > 1 && animTime < 1.5)) {
                        dinoMoveAmount = deltaTime * dinosaur.MoveSpeed;
                    }
                });
            }
            
            let minPositionX = -game.GameService.getGameState(this.world).EnvironmentSceneWidth / 2;
            
            // Move scrolling objects.
            this.world.forEach([ut.Entity, game.ScrollingObject, ut.Core2D.TransformLocalPosition], (entity, scrollingObject, transformLocalPosition) => {

                if (dinoMoveAmount == 0 && scrollingObject.AutonomousSpeed == 0) {
                    return;
                }

                let position = transformLocalPosition.position;
                position.x -= dinoMoveAmount * scrollingObject.Speed;
                position.x -= scrollingObject.AutonomousSpeed * deltaTime;
                transformLocalPosition.position = position;

                if (position.x < minPositionX) {
                    ut.Core2D.TransformService.destroyTree(this.world, entity, true);
                }
            });
        }
    }
}
