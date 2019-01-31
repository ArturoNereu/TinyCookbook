
namespace game {

    /** New System */
    export class AlertButtonSystem extends ut.ComponentSystem
    {    
        OnUpdate(): void
        {
            this.world.forEach([ut.UIControls.Button, ut.UIControls.MouseInteraction, game.AlertButton], (button, mouseInteraction, alertButton) =>
            {
                if (mouseInteraction.clicked)
                {
                    alert(alertButton.MessageToAlert);
                }
            });
        }
    }
}
