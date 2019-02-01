
namespace game {

    /** New System */
    export class CloseWindowButtonSystem extends ut.ComponentSystem {
        
        OnUpdate(): void
        {
            this.world.forEach([ut.UIControls.Button, ut.UIControls.MouseInteraction, game.CloseWindowButton], (button, mouseInteraction, alertButton) => {
                if (mouseInteraction.clicked)
                {
                    window.close();
                }
            });
        }
    }
}
