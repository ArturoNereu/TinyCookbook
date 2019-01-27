
namespace game {

    export class SkinService {

        static getSkinConfiguration(world: ut.World): game.SkinConfiguration {
            return world.getConfigData(game.SkinConfiguration);
        }

        static getCurrentSkin(world: ut.World): game.SkinTypes {
            return this.getSkinConfiguration(world).CurrentSkin;
        }

        static setCurrentSkin(world: ut.World, skin: game.SkinTypes): void {
            let skinData = this.getSkinConfiguration(world);
            skinData.CurrentSkin = skin;
            world.setConfigData(skinData);
        }

        static getCurrentSkinName(world: ut.World): string {
            return this.getSkinName(this.getCurrentSkin(world));
        }

        static getSkinName(skin: game.SkinTypes): string {
            switch (skin) {
                case game.SkinTypes.Camp:
                    return "Camp";
                case game.SkinTypes.Farm:
                    return "Farm";
                case game.SkinTypes.City:
                default:
                    return "City";
            }  
        }
    }
}
