
namespace game {

    export class TutorialService {
        
        static getTutorialConfiguration(world: ut.World): game.TutorialConfiguration {
            return world.getConfigData(game.TutorialConfiguration);
        }

        static init(world: ut.World) {
            let tutorialConfig = this.getTutorialConfiguration(world);
            tutorialConfig.IsMatchTutorialDone = game.UserDataService.getIsMatchTutorialDone();
            tutorialConfig.IsEggTutorialDone = game.UserDataService.getIsEggTutorialDone();
            tutorialConfig.IsSurvivalTutorialDone = game.UserDataService.getIsSurvivalTutorialDone();
            world.setConfigData(tutorialConfig);
        }

        static getIsMatchTutorialDone(world: ut.World): boolean {
            return this.getTutorialConfiguration(world).IsMatchTutorialDone;
        }

        static setIsMatchTutorialDone(world: ut.World, value: boolean) {
            let tutorialConfig = this.getTutorialConfiguration(world);
            tutorialConfig.IsMatchTutorialDone = value;
            world.setConfigData(tutorialConfig);
            UserDataService.setIsMatchTutorialDone(value);
        }

        static getIsEggTutorialDone(world: ut.World): boolean {
            return this.getTutorialConfiguration(world).IsEggTutorialDone;
        }

        static setIsEggTutorialDone(world: ut.World, value: boolean) {
            let tutorialConfig = this.getTutorialConfiguration(world);
            tutorialConfig.IsEggTutorialDone = value;
            world.setConfigData(tutorialConfig);
            UserDataService.setIsEggTutorialDone(value);
        }

        static getIsSurvivalTutorialDone(world: ut.World): boolean {
            return this.getTutorialConfiguration(world).IsSurvivalTutorialDone;
        }

        static setIsSurvivalTutorialDone(world: ut.World, value: boolean) {
            let tutorialConfig = this.getTutorialConfiguration(world);
            tutorialConfig.IsSurvivalTutorialDone = value;
            world.setConfigData(tutorialConfig);
            UserDataService.setIsSurvivalTutorialDone(value);
        }

        static spawnTutorialHighlightOnGems(world: ut.World, gemTransformPositions: Vector2[], labelPositionY: number = -1): ut.Entity {

            let minX = 1000;
            let maxX = -1000;
            let minY = 1000;
            let maxY = -1000;

            gemTransformPositions.forEach(transformPosition => {
                if (transformPosition.x < minX) {
                    minX = transformPosition.x;
                }
                if (transformPosition.x > maxX) {
                    maxX = transformPosition.x;
                }
                if (transformPosition.y < minY) {
                    minY = transformPosition.y;
                }
                if (transformPosition.y > maxY) {
                    maxY = transformPosition.y;
                }
            });

            let padding = 19;
            minX -= padding;
            maxX += padding;
            minY -= padding;
            maxY += padding;

            let position = new Vector2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);
            let size = new Vector2(maxX - minX, maxY - minY)
            return this.spawnTutorialHighlight(world, position, size, labelPositionY);
        }

        // Spawn an highlight on the screen, darkening everything except a rectangle to draw attention to its content.
        static spawnTutorialHighlight(world: ut.World, position: Vector2, size: Vector2, labelPositionY: number = -1): ut.Entity {
            let tutorialHighlightEntity = ut.EntityGroup.instantiate(world, "game.TutorialHighlight")[0];
            let tutorialHighlight = world.getComponentData(tutorialHighlightEntity, game.TutorialHighlight);
            let highlightRectTransform = world.getComponentData(tutorialHighlight.HighlightRect, ut.UILayout.RectTransform);
            highlightRectTransform.anchoredPosition = position;
            highlightRectTransform.sizeDelta = size;
            world.setComponentData(tutorialHighlight.HighlightRect, highlightRectTransform);

            let instructionsDefaultPositionY = labelPositionY == -1 ? position.y + size.y / 2 + 10 : labelPositionY;
            tutorialHighlight.LabelDefaultPositionY = instructionsDefaultPositionY;
            world.setComponentData(tutorialHighlightEntity, tutorialHighlight);

            let labelInstructionsRectTransform = world.getComponentData(tutorialHighlight.LabelInstructions, ut.UILayout.RectTransform);
            labelInstructionsRectTransform.anchoredPosition.x = 0;
            labelInstructionsRectTransform.anchoredPosition.y = instructionsDefaultPositionY;
            world.setComponentData(tutorialHighlight.LabelInstructions, labelInstructionsRectTransform);

            return tutorialHighlightEntity;
        }
    }
}
