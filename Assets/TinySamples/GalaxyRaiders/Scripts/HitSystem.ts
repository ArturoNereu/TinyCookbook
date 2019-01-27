
namespace game {
    /**
     * Spawns an explosion entity with a specific position and scale
     */
    let explode = function (world: ut.World, position: ut.Math.Vector3, scale: ut.Math.Vector3) {
        let explosion = ut.EntityGroup.instantiate(world, 'game.Explosion')[0];
        world.usingComponentData(explosion, [ut.Core2D.TransformLocalPosition, ut.Core2D.TransformLocalScale], (transformLocalPosition, transformLocalScale)=>{
        transformLocalPosition.position = position as any;
        transformLocalScale.scale = scale as any;
        });
    };

    @ut.executeAfter(ut.Shared.UserCodeStart)
    @ut.executeAfter(game.DamageSystem)
    @ut.executeBefore(ut.Shared.UserCodeEnd)
    @ut.requiredComponents(game.Hit)
    export class HitSystem extends ut.ComponentSystem {
        /**
         * Applies hit component on colliding objects
         */
        OnUpdate():void {
            let context = this.world.getConfigData(game.GameContext);
            if(context.State != game.GameState.Play){
                return;
            }
            let isGameOver = false;
            this.world.forEach([ut.Entity, game.Hit, game.GameOverLine],
                (entity, hit, gameOverLine) => {
                    isGameOver = true;
                    this.world.removeComponent(entity, game.GameOverLine);
                });
            if(isGameOver)
            {
                game.GameService.gameOver(this.world, context);
                this.world.setConfigData(context);
                return;
            }

            this.world.forEach([ut.Entity, game.Hit, game.Spaceship],
                (entity, hit, spaceship) => { 
                  game.GameService.decreaseLife(this.world, context) 
                });
        
            this.world.forEach([ut.Entity, game.Hit, game.Raider],
                (entity, hit, raider) => {
                    explode(this.world, ut.Core2D.TransformService.computeWorldPosition(this.world, entity), new Vector3(1, 1, 0));
                    game.GameService.increaseScore(this.world, context, raider.points);
        
                    // Destroy the raider
                    this.world.destroyEntity(entity);
                });
    
            this.world.forEach([ut.Entity, game.Hit, game.DefensePoint],
                (entity, hit, defensePoint) => {
                    explode(this.world, ut.Core2D.TransformService.computeWorldPosition(this.world, entity), new Vector3(0.5, 0.5, 0));
        
                    // Destroy the defense point
                    this.world.destroyEntity(entity);
                });

            this.world.forEach([ut.Entity, game.Hit, game.Laser],
                (entity, hit, laser) => {
                    this.world.destroyEntity(laser.reference);
                });

            this.world.forEach([ut.Entity, game.Hit],
                (entity, hit) => {
                    this.world.removeComponent(entity, game.Hit);
                });
    
            this.world.setConfigData(context);

        }
    }
}
