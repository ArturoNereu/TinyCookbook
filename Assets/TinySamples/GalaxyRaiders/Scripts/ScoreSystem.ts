
namespace game {

    @ut.executeAfter(ut.Shared.UserCodeStart)
    @ut.executeAfter(game.DamageSystem)
    @ut.executeBefore(ut.Shared.UserCodeEnd)
    @ut.requiredComponents(game.Score)
    export class ScoreSystem extends ut.ComponentSystem {
        /**
         * Updates the scoring text
         */
        OnUpdate():void {
            let context = this.world.getConfigData(game.GameContext);

            this.world.forEach([ut.Entity, ut.Text.Text2DRenderer, game.Score], (entity, textRenderer, score) => {
                textRenderer.text = context.Score.toString();
            });
        }
    }
}
