
namespace game {

    export class MovePeopleSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            let dinosaurEntity = DinosaurService.getDinosaurEntity(this.world);
            if (!this.world.exists(dinosaurEntity)) {
                return;
            }
            
            let dinosaurPosition = this.world.getComponentData(dinosaurEntity, ut.Core2D.TransformLocalPosition).position;
            let deltaTime = this.scheduler.deltaTime();

            this.world.forEach([game.Person, ut.Core2D.TransformLocalPosition],
                (person, transformPosition) => {

                    if (person.State == game.PersonState.Appear) {
                        let position = transformPosition.position;
                        position.y -= deltaTime * person.MoveSpeed;
                        if (position.y <= person.WalkPositionY) {
                            position.y = person.WalkPositionY;
                            person.State = game.PersonState.Fire;
                        }
                        transformPosition.position = position;
                    }
                    else if (person.State == game.PersonState.Fire) {
                        if (transformPosition.position.x < dinosaurPosition.x + person.MinimumDistanceFromDinosaur) {
                            person.State = game.PersonState.Walk;
                        }
                    }
                    else if (person.State == game.PersonState.Walk) {
                        let fearSpeedMultiplier = (transformPosition.position.x < dinosaurPosition.x + person.MinimumDistanceFromDinosaur) ? person.FearSpeedMultiplier : 1;
                        let position = transformPosition.position;
                        position.x += deltaTime * person.MoveSpeed * person.MoveSpeedMultiplier * fearSpeedMultiplier;
                        transformPosition.position = position;

                        if (transformPosition.position.x > dinosaurPosition.x + person.MaximumDistanceFromDinosaur) {
                            person.State = game.PersonState.Fire;
                        }
                    }
                    
                    GameService.setEntityEnabled(this.world, person.WalkAnimation, person.State == game.PersonState.Walk || person.State == game.PersonState.Appear);
                    GameService.setEntityEnabled(this.world, person.FireAnimation, person.State == game.PersonState.Fire);
                });
        }
    }
}
