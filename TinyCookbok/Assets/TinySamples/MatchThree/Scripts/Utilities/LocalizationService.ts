
namespace game {

    export class LocalizationService {

        static getLocalizationConfiguration(world: ut.World): game.LocalizationConfiguration {
            return world.getConfigData(game.LocalizationConfiguration);
        }

        static init(world: ut.World) {
            let localizationConfig = this.getLocalizationConfiguration(world);
            localizationConfig.LanguageID = game.UserDataService.getLanguageID();
            world.setConfigData(localizationConfig);
        }

        /**
         * Get a localized text.
         * @param world The world.
         * @param textID The ID of the text element to translate.
         * @param params The parameters to be inserted in the formated transalted text in the form of {0}, {1}, etc
         */
        static getText(world: ut.World, textID: string, ...params: string[]): string {

            if (textID.trim() == "") {
                return textID;
            }

            let localizationConfig = this.getLocalizationConfiguration(world);

            let result = "(" + textID + ")";
            localizationConfig.Texts.forEach(text => {
                if (textID == text.TextID) {
                    result = text[localizationConfig.LanguageID];
                }
            });

            for (let i = 0; params != null && i < params.length; i++) {
                result = result.replace("{" + i + "}", params[i]);
            }

            return result;
        }

        static getLanguageID(world: ut.World): string {
            return this.getLocalizationConfiguration(world).LanguageID;
        }

        static setLanguageID(world: ut.World, value: string): string {
            let localizationConfig = this.getLocalizationConfiguration(world);
            localizationConfig.LanguageID = value;
            world.setConfigData(localizationConfig);
            UserDataService.setLanguageID(value);
            return value;
        }
    }
}
