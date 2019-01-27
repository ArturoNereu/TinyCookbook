
namespace game {

    export class SoundService {
        
        static getSoundConfiguration(world: ut.World): game.SoundConfiguration {
            return world.getConfigData(game.SoundConfiguration);
        }

        static init(world: ut.World) {
            let soundConfig = this.getSoundConfiguration(world);
            soundConfig.IsSoundOn = game.UserDataService.getIsSoundOn();
            soundConfig.IsMusicOn = game.UserDataService.getIsMusicOn();
            world.setConfigData(soundConfig);
        }

        static play(world: ut.World, entityName: string) {
            if (this.getIsSoundOn(world)) {
                let soundEntity = world.getEntityByName(entityName);
                if (world.exists(soundEntity) && !world.hasComponent(soundEntity, ut.Audio.AudioSourceStart)) {
                    world.addComponent(soundEntity, ut.Audio.AudioSourceStart);
                }
            }
        }

        static stop(world: ut.World, entityName: string) {
            world.addComponent(world.getEntityByName(entityName), ut.Audio.AudioSourceStop);
        }

        static playMusic(world: ut.World) {
            this.stopMusic(world);

            if (!this.getIsMusicOn(world)) {
                return;
            }

            let gameState = game.GameService.getGameState(world);
            let musicName = "";
            switch (gameState.GameStateType) {
                case game.GameStateTypes.Paused:
                case game.GameStateTypes.Game: {
                    musicName = "Music" + game.SkinService.getCurrentSkinName(world);
                    break;
                }
                default: {
                    musicName = "MusicMenu";
                    break;
                }
            }

            let soundConfiguration = this.getSoundConfiguration(world);
            soundConfiguration.CurrentMusic = musicName;
            world.setConfigData(soundConfiguration);

            let musicEntity = world.getEntityByName(musicName);
            if (world.exists(musicEntity)) {
                world.addComponent(musicEntity, ut.Audio.AudioSourceStart);
            }
            else {
                console.log("[" + musicName + "] entity does not exists.");
            }
        }

        static stopMusic(world: ut.World) {
            if (this.getSoundConfiguration(world).CurrentMusic != "") {
                world.addComponent(world.getEntityByName(this.getSoundConfiguration(world).CurrentMusic), ut.Audio.AudioSourceStop);
            }
        }

        static toggleSoundIsOn(world: ut.World): boolean {
            return this.setIsSoundOn(world, !this.getIsSoundOn(world));
        }

        static getIsSoundOn(world: ut.World): boolean {
            return this.getSoundConfiguration(world).IsSoundOn;
        }

        static setIsSoundOn(world: ut.World, value: boolean): boolean {
            let soundConfig = this.getSoundConfiguration(world);
            soundConfig.IsSoundOn = value;
            world.setConfigData(soundConfig);
            UserDataService.setIsSoundOn(value);
            return value;
        }

        static toggleMusicIsOn(world: ut.World): boolean {
            return this.setIsMusicOn(world, !this.getIsMusicOn(world));
        }

        static getIsMusicOn(world: ut.World): boolean {
            return this.getSoundConfiguration(world).IsMusicOn;
        }

        static setIsMusicOn(world: ut.World, value: boolean): boolean {
            let soundConfig = this.getSoundConfiguration(world);
            soundConfig.IsMusicOn = value;
            world.setConfigData(soundConfig);
            UserDataService.setIsMusicOn(value);

            this.playMusic(world);
            return value;
        }
    }
}
