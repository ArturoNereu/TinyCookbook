namespace game {

    /**
     * Update the localized text associated to labels.
     */
    export class UpdateLocalizedText extends ut.ComponentSystem {
        
        OnUpdate():void {
            let languageID = LocalizationService.getLanguageID(this.world);
            this.world.forEach([game.LocalizedText, ut.Text.Text2DRenderer],
                (localizedText, textRenderer) => {
                    if (localizedText.TextID != localizedText.LastTextID || languageID != localizedText.LastLanguageID) {
                        localizedText.LastTextID = localizedText.TextID;
                        localizedText.LastLanguageID = languageID;

                        let text = LocalizationService.getText(this.world, localizedText.TextID, ...localizedText.TextParameters);
                        if (localizedText.ToUpper) {
                            text = text.toUpperCase();
                        }
						textRenderer.text = text;
                    }
                });
        }
    }
}
