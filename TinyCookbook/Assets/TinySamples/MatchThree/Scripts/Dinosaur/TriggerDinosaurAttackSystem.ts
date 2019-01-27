/// <reference path="../Gem/ActivateGemPowerUpSystem.ts" />
/// <reference path="../Gem/SpawnComboPowerUpSystem.ts" />
namespace game {

    /**
     * When the player makes a match, the dinosaur performs an attack.
     */
    @ut.executeAfter(game.ActivateGemPowerUpSystem)
    @ut.executeBefore(game.SpawnComboPowerUpSystem)
    export class TriggerDinosaurAttackSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            let matchedGemType = -1;
            this.world.forEach([ut.Entity, game.Gem, game.Matched, game.GemSwap], (entity, gemToDestroy, matched, swaped) => {

                if ((matched.IsMatch && matchedGemType != GemTypes.ColorBomb) ||
                    (gemToDestroy.GemType == GemTypes.ColorBomb && matched.CreatedPowerUp == GemPowerUpTypes.None)) {
                    matchedGemType = gemToDestroy.GemType;
                }
            });

            if (matchedGemType == -1 || GridService.isGridFrozen(this.world)) {
                return;
            }

            this.world.forEach([ut.Entity, game.Dinosaur], (entity, dinosaur) => {
                if (this.world.hasComponent(entity, DinosaurAttack)) {
                    return;
                }

                let dinosaurAttack = new game.DinosaurAttack;
                dinosaurAttack.AttackType = matchedGemType == GemTypes.ColorBomb ? game.DinosaurAttackTypes.Laser : matchedGemType;
                this.world.addComponentData(entity, dinosaurAttack);
            });
        }
    }
}
