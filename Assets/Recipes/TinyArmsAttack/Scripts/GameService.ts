
namespace game {

    /** New System */
    export class GameService
    {
        static restart(world: ut.World)
        {
            setTimeout(() =>
            {
                this.newGame(world);
            }, 1000);
        };

        static newGame(world: ut.World)
        {
            ut.EntityGroup.destroyAll(world, 'game.Gameplay');

            ut.EntityGroup.instantiate(world, 'game.Gameplay');
        }
    }
}
