namespace game 
{
    export class GameService 
    {    
        private static mainGroup: string = 'game.MainGroup';
        private static enemyGroup: string = 'game.EnemyGroup';
        private static explosionGroup: string = 'game.ExplosionGroup';
        
        static restart(world: ut.World) 
        {
            //return; //Uncomment this line if you don't want the game to restart

            setTimeout(() => 
            { 
                this.newGame(world);
            }, 3000);
        };

        static newGame(world: ut.World) 
        {
            ut.Time.reset();

            ut.EntityGroup.destroyAll(world, this.mainGroup);
            ut.EntityGroup.destroyAll(world, this.enemyGroup);
            ut.EntityGroup.destroyAll(world, this.explosionGroup);

            ut.EntityGroup.instantiate(world, this.mainGroup);
        };
    }
}