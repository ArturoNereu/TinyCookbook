
namespace game {

    /** New System */
    export class DinosaurSystem extends ut.ComponentSystem {
        
        OnUpdate(): void
        {
            this.world.forEach([ut.Core2D.Sprite2DSequencePlayer, game.Movement, game.Dinosaur], (sprite2dSequencePlayer, movement, dinosaur) => {

                

                if (ut.Runtime.Input.getMouseButtonDown(0))
                {
                    movement.isMoving = false;

                    dinosaur.dinosaurState = DinosaurStates.attacking;

                    sprite2dSequencePlayer.sequence = this.world.getEntityByName("DinosaurAttacking");
                    sprite2dSequencePlayer.time = 0;
                    
                }

                if (dinosaur.dinosaurState == DinosaurStates.attacking)
                {
                    dinosaur.timeSinceAttack += this.scheduler.deltaTime();

                    if (dinosaur.timeSinceAttack >= dinosaur.attackTime) {
                        movement.isMoving = true;

                        dinosaur.dinosaurState = DinosaurStates.walking;

                        sprite2dSequencePlayer.sequence = this.world.getEntityByName("DinosaurWalking");
                        sprite2dSequencePlayer.time = 0;

                        dinosaur.timeSinceAttack = 0;
                    }
                }
            });
        }
    }
}
