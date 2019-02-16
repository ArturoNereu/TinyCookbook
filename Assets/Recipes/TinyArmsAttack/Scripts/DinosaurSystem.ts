
namespace game 
{
    @ut.executeAfter(ut.Shared.InputFence)
    export class DinosaurSystem extends ut.ComponentSystem 
    {    
        OnUpdate(): void
        {
            this.world.forEach([ut.Core2D.Sprite2DSequencePlayer, game.Movement, game.Dinosaur], 
                (sprite2dSequencePlayer, movement, dinosaur) => {

                if (ut.Runtime.Input.getMouseButtonDown(0))
                {
                    this.ChangeState(dinosaur, movement, DinosaurStates.attacking);

                    sprite2dSequencePlayer.sequence = this.world.getEntityByName("DinosaurAttacking");
                    sprite2dSequencePlayer.time = 0;
                    
                }

                if (dinosaur.dinosaurState == DinosaurStates.attacking)
                {
                    dinosaur.timeSinceAttack += this.scheduler.deltaTime();

                    if (dinosaur.timeSinceAttack >= dinosaur.attackTime) {

                        this.ChangeState(dinosaur, movement, DinosaurStates.walking);

                        sprite2dSequencePlayer.sequence = this.world.getEntityByName("DinosaurWalking");
                        sprite2dSequencePlayer.time = 0;
                    }
                }
            });
        }

        ChangeState(dinosaur: game.Dinosaur, movement: game.Movement, dinosaurState: DinosaurStates): void
        {
            if(dinosaurState == DinosaurStates.attacking)
            {
                dinosaur.dinosaurState = DinosaurStates.attacking;
                dinosaur.timeSinceAttack = 0;
                movement.isMoving = false;
            }
            else if(dinosaurState == DinosaurStates.walking)
            {
                dinosaur.dinosaurState = DinosaurStates.walking;
                movement.isMoving = true;
            }
        }
    }
}
