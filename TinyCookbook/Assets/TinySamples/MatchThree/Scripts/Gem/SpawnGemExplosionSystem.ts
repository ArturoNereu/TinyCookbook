
namespace game {

    /**
     * Spawn particle explosions for each gem to destroy on the grid.
     */
    @ut.executeAfter(game.UpdateScoreSystem)
    @ut.executeBefore(game.DeleteMatchedGemSystem)
    export class SpawnGemExplosionSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            if (GridService.isGridFrozen(this.world)) {
                return;
            }

            this.world.forEach([game.Gem, game.Matched, ut.Core2D.TransformLocalPosition],
                (gemToDestroy, matched, gemTransformLocalPosition) => {

                    let particleEntity1 = ut.EntityGroup.instantiate(this.world, "game.ExplodingGem1")[0];
                    this.initParticleSystem(particleEntity1, gemToDestroy, gemTransformLocalPosition);

                    let particleEntity2 = ut.EntityGroup.instantiate(this.world, "game.ExplodingGem2")[0];
                    this.initParticleSystem(particleEntity2, gemToDestroy, gemTransformLocalPosition);
                });
        }

        initParticleSystem(explosionEntity: ut.Entity, gemToDestroy: game.Gem, gemTransformLocalPosition: ut.Core2D.TransformLocalPosition) {
            let emitter = this.world.getComponentData(explosionEntity, ut.Particles.ParticleEmitter);

            let particleSpriteRenderer = this.world.getComponentData(emitter.particle, ut.Core2D.Sprite2DRenderer);
            particleSpriteRenderer.color = game.GemService.getGemParticleColor(this.world, gemToDestroy);
            this.world.setComponentData(emitter.particle, particleSpriteRenderer);

            let gemPosition = gemTransformLocalPosition.position;
            let explosionTransformPosition = this.world.getComponentData(explosionEntity, ut.Core2D.TransformLocalPosition);
            let position = explosionTransformPosition.position;
            position.x = gemPosition.x;
            position.y = gemPosition.y;
            explosionTransformPosition.position = position;
            this.world.setComponentData(explosionEntity, explosionTransformPosition);
        }
    }
}
