namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeAfter(ut.HitBox2D.HitBox2DSystem)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  @ut.requiredComponents(ut.HitBox2D.HitBoxOverlapResults)
  export class DamageSystem extends ut.ComponentSystem {
    
    /**
     * Applies hit component on colliding objects
     */
    OnUpdate(): void {
      let isGameOver = false;

      let context = this.world.getConfigData(game.GameContext);
      if(context.State != game.GameState.Play){
        return;
      }

      this.world.forEach([ut.Entity, ut.HitBox2D.HitBoxOverlapResults, game.Laser],
        (entity, hitboxoverlapresults, laser) => {
          if (hitboxoverlapresults.overlaps.length == 0 || this.world.hasComponent(entity, game.Hit)){
            return;
          }
          for (let i = 0; i < hitboxoverlapresults.overlaps.length; i++) {
            let otherEntity = hitboxoverlapresults.overlaps[i].otherEntity;
            if(!this.world.exists(otherEntity) || this.world.hasComponent(otherEntity, game.Hit)){
              continue;
            }

            if(this.world.hasComponent(otherEntity, game.Laser) || 
               this.world.hasComponent(otherEntity, game.DefensePoint) ||
               (laser.tag == game.LaserTag.Spaceship && this.world.hasComponent(otherEntity, game.Raider)) ||
               (laser.tag == game.LaserTag.Raider && this.world.hasComponent(otherEntity, game.Spaceship))){
              this.world.addComponent(entity, game.Hit);
              this.world.addComponent(otherEntity, game.Hit);
              break;
            }
            else if(this.world.hasComponent(otherEntity, game.Boundary)){
              this.world.addComponent(entity, game.Hit);
              break;
            }
          }
        });

      this.world.forEach([ut.Entity, ut.HitBox2D.HitBoxOverlapResults, game.Raider],
        (entity, hitboxoverlapresults, raider) => {
          if (hitboxoverlapresults.overlaps.length == 0) {
            return;
          }
          for (let i = 0; i < hitboxoverlapresults.overlaps.length; i++) {
            let otherEntity = hitboxoverlapresults.overlaps[i].otherEntity;
            if(!this.world.exists(otherEntity) || this.world.hasComponent(otherEntity, game.Hit)){
              continue;
            }

            if(this.world.hasComponent(otherEntity, game.DefensePoint) ||
               this.world.hasComponent(otherEntity, game.GameOverLine)){
              this.world.addComponent(otherEntity, game.Hit);
              break;
            }
          }
        });
    }

  }

}
