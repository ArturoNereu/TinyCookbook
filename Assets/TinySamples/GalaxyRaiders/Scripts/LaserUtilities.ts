
namespace game {

    /** New System */
    export class LaserUtilities{
        /**
         * @desc Checks if there's a laser
         */
        static laserExists(world: ut.World, laserTag: game.LaserTag): boolean{
            let laserAlreadyExists = false;
            world.forEach([ut.Entity, game.Laser], (entity, laser) => {
              if(laser.tag == laserTag){
                laserAlreadyExists = true;
              }
            });
            return laserAlreadyExists;
        }

        /**
         * @desc Spawns the laser
         */
        static spawnLaser(world: ut.World, spawner: ut.Entity, entityGroup: string, offset: number){
            let laserEntity = ut.EntityGroup.instantiate(world, entityGroup)[0];
            
            world.usingComponentData(laserEntity, [ut.Core2D.TransformLocalPosition, game.Laser], (transformLocalPosition, laser)=>{
              transformLocalPosition.position = ut.Core2D.TransformService.computeWorldPosition(world, spawner);
              transformLocalPosition.position.y += offset;

              laser.reference = laserEntity;
            });
        }
    }
}
