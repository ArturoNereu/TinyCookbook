
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
                    sprite2dSequencePlayer.loop = ut.Core2D.LoopMode.Once;
                    
                }
            });
        }
    }
}
