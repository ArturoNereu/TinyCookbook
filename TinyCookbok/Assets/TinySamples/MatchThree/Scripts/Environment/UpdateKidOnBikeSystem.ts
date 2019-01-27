
namespace game {

    /**
     * The kid on his bike makes a wheely animation when the dinosaur makes an attack near him.
     */
    export class UpdateKidOnBikeSystem extends ut.ComponentSystem {
        
        OnUpdate():void {
            this.world.forEach([game.KidOnBike],
                (kid) => {
                    game.GameService.setEntityEnabled(this.world, kid.AnimationKidBike, !kid.IsInWheelyMode);
                    game.GameService.setEntityEnabled(this.world, kid.AnimationKidWheely, kid.IsInWheelyMode);
                });
        }
    }
}
