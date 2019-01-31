
namespace game {

    /** New System */
    export class NewWindowButtonSystem extends ut.ComponentSystem {
        
        OnUpdate(): void
        {
            this.world.forEach([ut.UIControls.Button, ut.UIControls.MouseInteraction, game.NewWindowButton], (button, mouseInteraction, alertButton) => {
                if (mouseInteraction.clicked)
                {
                    
                    //document.execCommand("openURLInNewWindow");
                    //alert("efdsf");
                }
            });
        }
    }
}
