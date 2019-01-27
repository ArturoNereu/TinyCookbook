
namespace game {

    @ut.executeAfter(ut.Shared.UserCodeStart)
    @ut.executeAfter(game.DamageSystem)
    @ut.executeBefore(ut.Shared.UserCodeEnd)
    @ut.requiredComponents(game.LifeManager)
    export class LifeManagerSystem extends ut.ComponentSystem {
        
        /**
         * Updates the player's life text and sprites, checks for gameover condition
         */
        OnUpdate():void {
            let context = this.world.getConfigData(game.GameContext);

            this.world.forEach([ut.Entity, game.LifeManager], (entity, lifeManager) => {
                
                this.world.usingComponentData(lifeManager.LifeCount, [ut.Text.Text2DRenderer], (TextRenderer)=>{
                    if(TextRenderer.text != context.Life.toString() && context.Life >= 0){
                        this.world.destroyEntity(lifeManager.LifeSprites[context.Life]);
                        TextRenderer.text = context.Life.toString();
                    } 
                });
            });

            if(context.Life < 0){
                GameService.gameOver(this.world, context);
                context.Life = 0;
            }

            this.world.setConfigData(context);
        }
    }
}
