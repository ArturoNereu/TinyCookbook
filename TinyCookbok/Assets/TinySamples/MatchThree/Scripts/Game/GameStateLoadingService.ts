
namespace game {

    /**
     * Load a specific game state and initialize it.
     */
    export class GameStateLoadingService {
        
        static transitionToGameState(world: ut.World, gameState: game.GameStateTypes): ut.Entity {
            this.setGameState(world, game.GameStateTypes.Transition);
            let screenTransitionEntity = ut.EntityGroup.instantiate(world, "game.ScreenTransition")[0];
            let screenTransition = world.getComponentData(screenTransitionEntity, game.ScreenTransition);
            screenTransition.TransitionToState = gameState;
            world.setComponentData(screenTransitionEntity, screenTransition);
            return screenTransitionEntity;
        }

        static transitionToGameStateWithDuration(world: ut.World, gameState: game.GameStateTypes, outDuration: number, inDuration: number) {
            let screenTransitionEntity = this.transitionToGameState(world, gameState);
            let screenTransition = world.getComponentData(screenTransitionEntity, game.ScreenTransition);
            screenTransition.OutDuration = outDuration;
            screenTransition.InDuration = inDuration;
            world.setComponentData(screenTransitionEntity, screenTransition);
        }

        static transitionToGameStateWithScaledHole(world: ut.World, gameState: game.GameStateTypes, positionOffset: Vector2) {
            let screenTransitionEntity = this.transitionToGameState(world, gameState);
            let screenTransition = world.getComponentData(screenTransitionEntity, game.ScreenTransition);
            screenTransition.IsScaleHoleTransition = true;

            let scaleHoleRectTransform = world.getComponentData(screenTransition.ScaleHole, ut.UILayout.RectTransform);
            scaleHoleRectTransform.anchoredPosition = positionOffset;
            world.setComponentData(screenTransition.ScaleHole, scaleHoleRectTransform);

            world.setComponentData(screenTransitionEntity, screenTransition);
        }

        static setGameState(world: ut.World, gameStateType: game.GameStateTypes) {
            let gameState = GameService.getGameState(world);
            gameState.GameStateType = gameStateType;
            world.setComponentData(GameService.gameStateEntity, gameState);

            switch (gameStateType) {
                case game.GameStateTypes.Loading: {
                    this.loadLoadingScreen(world);
                    break;
                }
                case game.GameStateTypes.Cutscene: {
                    this.loadCutscene(world);
                    break;
                }
                case game.GameStateTypes.CutsceneEnd: {
                    this.loadEndCutscene(world);
                    break;
                }
                case game.GameStateTypes.MainMenu: {
                    this.loadMainMenu(world);
                    break;
                }
                case game.GameStateTypes.WorldMap: {
                    this.loadWorldMap(world);
                    break;
                }
                case game.GameStateTypes.Game: {
                    this.loadGame(world);
                    break;
                }
                case game.GameStateTypes.Settings: {
                    this.loadSettings(world);
                    break;
                }
                case game.GameStateTypes.Paused: {
                    this.loadPause(world);
                    break;
                }
                case game.GameStateTypes.Credits: {
                    this.loadCredits(world);
                    break;
                }
                case game.GameStateTypes.Languages: {
                    this.loadLanguages(world);
                    break;
                }
                case game.GameStateTypes.GameOver: {
                    this.loadGameOver(world);
                    break;
                }
            }
        }

        private static loadLoadingScreen (world: ut.World) {
            game.SoundService.init(world);
            game.LocalizationService.init(world);
            game.TutorialService.init(world);
            ut.EntityGroup.instantiate(world, "game.Loading");
        }
        
        private static loadCutscene(world: ut.World) {

            let mainMenu = world.getEntityByName("MainMenu");
            if (world.exists(mainMenu)) {
                ut.Core2D.TransformService.destroyTree(world, mainMenu);
                ut.EntityGroup.destroyAll(world, "game.MainMenu");
                ut.Tweens.TweenService.removeAllTweensInWorld(world);
            }

            ut.EntityGroup.instantiate(world, "game.CutsceneCamera");
            let partAEntity = ut.EntityGroup.instantiate(world, "game.CutscenePartA")[0];
            let partBEntity = ut.EntityGroup.instantiate(world, "game.CutscenePartB")[0];
            let partCEntity = ut.EntityGroup.instantiate(world, "game.CutscenePartC")[0];

            let displayInfo = world.getConfigData(ut.Core2D.DisplayInfo);
            let aspectRatio = displayInfo.width / displayInfo.height;
            let isLandscape = aspectRatio >= 16 / 9 - 0.01;

            let transformPositionPartA = world.getComponentData(partAEntity, ut.Core2D.TransformLocalPosition);
            transformPositionPartA.position.x = isLandscape ? -112 : 0;
            world.setComponentData(partAEntity, transformPositionPartA);
            let transformPositionPartC = world.getComponentData(partCEntity, ut.Core2D.TransformLocalPosition);
            transformPositionPartC.position.x = isLandscape ? 112 : 0;
            world.setComponentData(partCEntity, transformPositionPartC);
        }

        private static loadEndCutscene(world: ut.World) {

            if (world.exists(DinosaurService.getDinosaurEntity(world))) {
                game.GameService.unloadLevel(world);
            }

            let worldMap = world.getEntityByName("WorldMap");
            if (world.exists(worldMap)) {
                ut.Core2D.TransformService.destroyTree(world, worldMap);
                ut.EntityGroup.destroyAll(world, "game.WorldMap");
                ut.Tweens.TweenService.removeAllTweensInWorld(world);
            }

            ut.EntityGroup.instantiate(world, "game.CutsceneCamera");
            let partAEntity = ut.EntityGroup.instantiate(world, "game.EndCutscenePartA")[0];
            let partBEntity = ut.EntityGroup.instantiate(world, "game.EndCutscenePartB")[0];

            let displayInfo = world.getConfigData(ut.Core2D.DisplayInfo);
            let aspectRatio = displayInfo.width / displayInfo.height;
            let isLandscape = aspectRatio >= 16 / 9 - 0.01;

            let transformPositionPartA = world.getComponentData(partAEntity, ut.Core2D.TransformLocalPosition);
            transformPositionPartA.position.x = isLandscape ? -55 : 0;
            world.setComponentData(partAEntity, transformPositionPartA);
            let transformPositionPartB = world.getComponentData(partBEntity, ut.Core2D.TransformLocalPosition);
            transformPositionPartB.position.x = isLandscape ? 55 : 0;
            world.setComponentData(partBEntity, transformPositionPartB);
        }

        private static loadMainMenu(world: ut.World) {

            if (world.exists(world.getEntityByName("CutsceneCamera"))) {
                ut.EntityGroup.destroyAll(world, "game.CutsceneCamera");
                ut.EntityGroup.destroyAll(world, "game.CutscenePartA");
                ut.EntityGroup.destroyAll(world, "game.CutscenePartB");
                ut.EntityGroup.destroyAll(world, "game.CutscenePartC");
            }

            ut.EntityGroup.instantiate(world, "game.MainMenu");
            SoundService.playMusic(world);
        }

        private static loadWorldMap(world: ut.World) {

            if (world.exists(DinosaurService.getDinosaurEntity(world))) {
                game.GameService.unloadLevel(world);
            }

            let mainMenu = world.getEntityByName("MainMenu");
            if (world.exists(mainMenu)) {
                ut.Core2D.TransformService.destroyTree(world, mainMenu);
                ut.EntityGroup.destroyAll(world, "game.MainMenu");
                ut.Tweens.TweenService.removeAllTweensInWorld(world);
            }

            if (world.exists(world.getEntityByName("CutsceneCamera"))) {
                ut.EntityGroup.destroyAll(world, "game.CutsceneCamera");
                ut.EntityGroup.destroyAll(world, "game.EndCutscenePartA");
                ut.EntityGroup.destroyAll(world, "game.EndCutscenePartB");
            }

            // Load world map.
            let worldMapEntity = ut.EntityGroup.instantiate(world, "game.WorldMap")[0];
            let worldMap = world.getComponentData(worldMapEntity, WorldMap);
            worldMap.CurrentLevelIndex = UserDataService.getSelectedWorldMapIndex();
            worldMap.CurrentScrollIndexPosition = worldMap.CurrentLevelIndex;
            worldMap.LastBeatenLevelID = UserDataService.getLastBeatenLevelID();
            world.setComponentData(worldMapEntity, worldMap);

            // Load level items.
            for (let i = 0; i < GameService.getGameState(world).LevelCount; i++) {
                let worldMapItemEntity = ut.EntityGroup.instantiate(world, "game.WorldMapItem")[0];

                let worldMapItem = world.getComponentData(worldMapItemEntity, WorldMapItem);
                worldMapItem.Index = i;
                world.setComponentData(worldMapItemEntity, worldMapItem);

                let worldMapTransformNode = world.getComponentData(worldMapItemEntity, ut.Core2D.TransformNode);
                worldMapTransformNode.parent = worldMapEntity;
                world.setComponentData(worldMapItemEntity, worldMapTransformNode);

                let level = GameService.getLevel(world, i + 1);
                let skinName = SkinService.getSkinName(level.Skin);
                let spritePath = "assets/sprites/WorldMap/" + skinName + "_WorldMap_Icon";
                let spriteRendererLevelPreview = world.getComponentData(worldMapItem.ImageLevelPreview, ut.Core2D.Sprite2DRenderer)
                spriteRendererLevelPreview.sprite = world.getEntityByName(spritePath);
                world.setComponentData(worldMapItem.ImageLevelPreview, spriteRendererLevelPreview);

                let sprite = world.getComponentData(spriteRendererLevelPreview.sprite, ut.Core2D.Sprite2D);
                let image = world.getComponentData(sprite.image, ut.Core2D.Image2D);
                let imageWidth = sprite.imageRegion.width * image.imagePixelSize.x;
                let imageHeight = sprite.imageRegion.height * image.imagePixelSize.y;
                let rectTransformLevelPreview = world.getComponentData(worldMapItem.ImageLevelPreview, ut.UILayout.RectTransform)
                rectTransformLevelPreview.sizeDelta = new Vector2(imageWidth, imageHeight);
                world.setComponentData(worldMapItem.ImageLevelPreview, rectTransformLevelPreview);
            }
        }

        private static loadGame(world: ut.World) {

            let worldMap = world.getEntityByName("WorldMap");
            if (world.exists(worldMap)) {
                ut.Core2D.TransformService.destroyTree(world, worldMap);
                ut.EntityGroup.destroyAll(world, "game.WorldMap");
                ut.Tweens.TweenService.removeAllTweensInWorld(world);
            }

            let levelID = UserDataService.getSelectedWorldMapIndex() + 1;
            let levelEntity = world.getEntityByName("Level" + levelID);
            let level = world.getComponentData(levelEntity, game.Level);

            SkinService.setCurrentSkin(world, level.Skin);

            ut.EntityGroup.instantiate(world, "game.GameScene");
            ut.EntityGroup.instantiate(world, "game.Dinosaur");
            ut.EntityGroup.instantiate(world, "game.BackgroundNearDeathWarning");

            let gameUIEntity = ut.EntityGroup.instantiate(world, "game.GameUI")[0];
            let gameUI = world.getComponentData(gameUIEntity, game.GameUI);
            let labelObjective = world.getComponentData(gameUI.LabelObjective, ut.Text.Text2DRenderer);
            let isPointObjective = world.hasComponent(levelEntity, game.LevelPointObjective);
            let isEggObjective = world.hasComponent(levelEntity, game.LevelEggObjective);
            let isSurvivalObjective = world.hasComponent(levelEntity, game.LevelSurvival);
            game.GameService.setEntityEnabled(world, gameUI.ImageObjectivePoint, isPointObjective);
            game.GameService.setEntityEnabled(world, gameUI.ImageObjectiveEgg, isEggObjective);
            game.GameService.setEntityEnabled(world, gameUI.ImageObjectiveSurvival, isSurvivalObjective);
            game.GameService.setEntityEnabled(world, gameUI.ImageMoves, !isSurvivalObjective);
            game.GameService.setEntityEnabled(world, gameUI.ImageNoMovesWarning, !isSurvivalObjective);
            if (isPointObjective) {
                let pointObjective = world.getComponentData(levelEntity, game.LevelPointObjective);
                labelObjective.text = game.GameService.formatNumber(pointObjective.ScoreObjective);
                world.setComponentData(gameUI.LabelObjective, labelObjective);
            }
            else if (isSurvivalObjective) {
                let survivalObjective = world.getComponentData(levelEntity, game.LevelSurvival);
                labelObjective.text = game.GameService.formatTime(survivalObjective.TimeObjective);
                world.setComponentData(gameUI.LabelObjective, labelObjective);

                ut.EntityGroup.instantiate(world, "game.SurvivalModeTimeline");
            }

            let gameState = GameService.getGameState(world);
            gameState.CurrentLevelID = levelID;
            gameState.CurrentMoveCount = 0;
            gameState.CurrentScore = 0;
            gameState.Time = 0;
            world.setComponentData(GameService.gameStateEntity, gameState);

            if (world.hasComponent(levelEntity, game.LevelSurvival)) {
                let levelSurvival = world.getComponentData(levelEntity, game.LevelSurvival);
                levelSurvival.SurvivalTimer = levelSurvival.MaxSurvivalTime;
                world.setComponentData(levelEntity, levelSurvival);
            }
            
            GameService.updateRemainingMovesLabel(world);

            SoundService.playMusic(world);
        }

        private static loadSettings(world: ut.World) {
            let settingsMenuEntity = ut.EntityGroup.instantiate(world, "game.SettingsMenu")[0];
            this.loadAudioSettings(world, settingsMenuEntity);

            let settingsMenu = world.getComponentData(settingsMenuEntity, game.SettingsMenu);
            let displayInfo = world.getConfigData(ut.Core2D.DisplayInfo);
            let labelRenderMode = world.getComponentData(settingsMenu.LabelRenderMode, game.LocalizedText);
            labelRenderMode.TextID = "Settings_Rendering";
            labelRenderMode.TextParameters = [(displayInfo.renderMode == ut.Core2D.RenderMode.WebGL ? "WebGL" : "Canvas")];
            labelRenderMode.LastTextID = ""; // Set dirty to force refresh
            world.setComponentData(settingsMenu.LabelRenderMode, labelRenderMode);
        }

        private static loadAudioSettings(world: ut.World, entity: ut.Entity) {
            let settingsMenuAudio = world.getComponentData(entity, game.SettingsMenuAudio);

            let soundSpriteRenderer = world.getComponentData(settingsMenuAudio.ButtonSound, ut.Core2D.Sprite2DRenderer);
            soundSpriteRenderer.sprite = SoundService.getIsSoundOn(world) ? settingsMenuAudio.SpriteAudioOn : settingsMenuAudio.SpriteAudioOff;
            world.setComponentData(settingsMenuAudio.ButtonSound, soundSpriteRenderer);
            let musicSpriteRenderer = world.getComponentData(settingsMenuAudio.ButtonMusic, ut.Core2D.Sprite2DRenderer);
            musicSpriteRenderer.sprite = SoundService.getIsMusicOn(world) ? settingsMenuAudio.SpriteAudioOn : settingsMenuAudio.SpriteAudioOff;
            world.setComponentData(settingsMenuAudio.ButtonMusic, musicSpriteRenderer);
        }

        private static loadPause(world: ut.World) {
            let pauseMenuEntity = ut.EntityGroup.instantiate(world, "game.PauseMenu")[0];
            this.loadAudioSettings(world, pauseMenuEntity);
        }

        private static loadCredits(world: ut.World) {
            ut.EntityGroup.instantiate(world, "game.CreditsMenu");
        }

        private static loadLanguages(world: ut.World) {
            ut.EntityGroup.instantiate(world, "game.LanguagesMenu");
        }

        private static loadGameOver(world: ut.World) {
            let gameState = GameService.getGameState(world);
            let gameOverEntity = ut.EntityGroup.instantiate(world, "game.GameOverMenu")[0];
            let gameOverMenu = world.getComponentData(gameOverEntity, game.GameOverMenu);

            let isObjectiveComplete = game.GameService.isObjectiveCompleted(world);
            let isSurvivalMode = world.hasComponent(game.GameService.getCurrentLevelEntity(world), game.LevelSurvival);
            let gameOverTextID = isObjectiveComplete ? "GameOver_Success" : "GameOver_Failure";
            let labelGameOver = world.getComponentData(gameOverMenu.LabelGameOver, game.LocalizedText);
            labelGameOver.TextID = gameOverTextID;
            world.setComponentData(gameOverMenu.LabelGameOver, labelGameOver);
            let labelGameOverShadow = world.getComponentData(gameOverMenu.LabelGameOverShadow, game.LocalizedText);
            labelGameOverShadow.TextID = gameOverTextID;
            world.setComponentData(gameOverMenu.LabelGameOverShadow, labelGameOverShadow);

            gameOverMenu.SuccessArms.forEach(armSuccess => {
                GameService.setEntityEnabled(world, armSuccess, isObjectiveComplete);
                this.animateGameOverArm(world, armSuccess);
            });
            gameOverMenu.FailArms.forEach(armFail => {
                GameService.setEntityEnabled(world, armFail, !isObjectiveComplete);
                this.animateGameOverArm(world, armFail);
            });

            let labelScore = world.getComponentData(gameOverMenu.LabelScoreValue, ut.Text.Text2DRenderer);
            labelScore.text = game.GameService.formatNumber(gameState.CurrentScore) + " pts";
            world.setComponentData(gameOverMenu.LabelScoreValue, labelScore);

            let labelTime = world.getComponentData(gameOverMenu.LabelTime, game.LocalizedText);
            labelTime.TextID = isSurvivalMode ? "GameOver_Time" : " ";
            world.setComponentData(gameOverMenu.LabelTime, labelTime);

            let labelTimeValue = world.getComponentData(gameOverMenu.LabelTimeValue, ut.Text.Text2DRenderer);
            labelTimeValue.text = isSurvivalMode ? GameService.formatTime(gameState.Time) : "";
            world.setComponentData(gameOverMenu.LabelTimeValue, labelTimeValue);

            let oldBestScore = game.UserDataService.getBestScore(gameState.CurrentLevelID);
			let isBestScore = gameState.CurrentScore > oldBestScore && isObjectiveComplete;
            let isNewBestScore = isBestScore && oldBestScore > 0;
            let isFirstTimeBeaten = isBestScore && oldBestScore == 0;
            GameService.setEntityEnabled(world, gameOverMenu.ImageNewBestScore, isNewBestScore);
            GameService.setEntityEnabled(world, gameOverMenu.ImageNewBestScoreGlow, isNewBestScore);
            let labelNewBest = world.getComponentData(gameOverMenu.LabelNewBestScore, LocalizedText);
            labelNewBest.TextID = isNewBestScore ? "GameOver_NewBest" : " ";
            world.setComponentData(gameOverMenu.LabelNewBestScore, labelNewBest);

            if (isBestScore) {
                game.UserDataService.setBestScore(gameState.CurrentLevelID, gameState.CurrentScore);
            }

            if (isObjectiveComplete && gameState.CurrentLevelID > game.UserDataService.getLastBeatenLevelID()) {
                game.UserDataService.setLastBeatenLevelID(gameState.CurrentLevelID);
            }

            // Set new unlocked level as selected level on the world map.
            if (isFirstTimeBeaten) {
                let currentLevelIndex = Math.min(2, Math.max(0, gameState.CurrentLevelID));
                UserDataService.setSelectedWorldMapIndex(currentLevelIndex);
            }

            DinosaurService.setEndGameAnimation(world, isObjectiveComplete);
        }

        static animateGameOverArm(world: ut.World, armEntity: ut.Entity) {
            if (world.hasComponent(armEntity, ut.Disabled)) {
                return;
            }

            let transformRotation = world.getComponentData(armEntity, ut.Core2D.TransformLocalRotation);
            let transformScale = world.getComponentData(armEntity, ut.Core2D.TransformLocalScale);
            let startRotation = new Quaternion().setFromAxisAngle(new Vector3(0, 0, 1), 0);
            let endRotation = new Quaternion().setFromAxisAngle(new Vector3(0, 0, 1), (transformScale.scale.x < 0 ? -15 : 15) * Math.PI / 180);

            let rotateArmTween = new ut.Tweens.TweenDesc;
            rotateArmTween.cid = ut.Core2D.TransformLocalRotation.cid;
            rotateArmTween.offset = 0;
            rotateArmTween.duration = 0.5;
            rotateArmTween.func = ut.Tweens.TweenFunc.Cosine;
            rotateArmTween.loop = ut.Core2D.LoopMode.PingPong;
            rotateArmTween.destroyWhenDone = true;
            rotateArmTween.t = 0;

            ut.Tweens.TweenService.addTweenQuaternion(world, armEntity, startRotation, endRotation, rotateArmTween);

            transformRotation.rotation = startRotation;
            world.setComponentData(armEntity, transformRotation);
        }
    }
}
