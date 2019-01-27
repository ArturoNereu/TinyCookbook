
namespace game {

    /**
     * Update the current game state.
     */
    @ut.executeAfter(ut.Shared.UserCodeEnd)
    export class GameStateUpdateService {
        
        static updateGameState(world: ut.World, gameState: GameState) {
            switch (gameState.GameStateType) {
                case game.GameStateTypes.Boot: {
                    this.updateBoot(world, gameState);
                    break;
                }
                case game.GameStateTypes.Loading: {
                    this.updateLoading(world, gameState);
                    break;
                }
                case game.GameStateTypes.MainMenu: {
                    this.updateMainMenu(world, gameState);
                    break;
                }
                case game.GameStateTypes.WorldMap: {
                    this.updateWorldMap(world, gameState);
                    break;
                }
                case game.GameStateTypes.Game: {
                    this.updateGame(world, gameState);
                    break;
                }
                case game.GameStateTypes.Settings: {
                    this.updateSettings(world, gameState);
                    break;
                }
                case game.GameStateTypes.Paused: {
                    this.updatePause(world, gameState);
                    break;
                }
                case game.GameStateTypes.Credits: {
                    this.updateCredits(world, gameState);
                    break;
                }
                case game.GameStateTypes.Languages: {
                    this.updateLanguages(world, gameState);
                    break;
                }
                case game.GameStateTypes.GameOver: {
                    this.updateGameOver(world, gameState);
                    break;
                }
            }
        }

        static updateBoot(world: ut.World, gameState: GameState) {
            GameStateLoadingService.setGameState(world, game.GameStateTypes.Loading);

            for (let levelID = 1; levelID <= GameService.getGameState(world).LevelCount; levelID++) {
                ut.EntityGroup.instantiate(world, "game.Level" + levelID);
            }
        }

        static updateLoading(world: ut.World, gameState: GameState) {
            ut.EntityGroup.destroyAll(world, "game.Loading");
          
            if (UserDataService.getHasSeenCutscene()) {
                GameStateLoadingService.setGameState(world, game.GameStateTypes.MainMenu);
            }
            else {
                UserDataService.setHasSeenCutscene(true);
                GameStateLoadingService.setGameState(world, game.GameStateTypes.Cutscene);
            }
        }

        static updateMainMenu(world: ut.World, gameState: GameState) {
            let mainMenu = world.getComponentData(world.getEntityByName("MainMenu"), game.MainMenu);
            let buttonPlay = world.getComponentData(mainMenu.ButtonPlay, game.CustomButton);
            let buttonWatchIntro = world.getComponentData(mainMenu.ButtonWatchIntro, game.CustomButton);
            let buttonSettings = world.getComponentData(mainMenu.ButtonSettings, game.CustomButton);

            if (buttonPlay.JustClicked) {
                GameStateLoadingService.transitionToGameStateWithScaledHole(world, game.GameStateTypes.WorldMap, new Vector2(0, 50));
                SoundService.play(world, "GenericClickSound");
            }
            else if (buttonWatchIntro.JustClicked) {
                GameStateLoadingService.transitionToGameState(world, game.GameStateTypes.Cutscene);
                SoundService.play(world, "GenericClickSound");
                SoundService.stopMusic(world);
            }
            else if (buttonSettings.JustClicked) {
                GameStateLoadingService.setGameState(world, game.GameStateTypes.Settings);
                SoundService.play(world, "GenericClickSound");
            }
        }

        static updateWorldMap(world: ut.World, gameState: GameState) {
            let worldMap = world.getComponentData(world.getEntityByName("WorldMap"), game.WorldMap);
            let buttonPlay = world.getComponentData(worldMap.ButtonPlay, game.CustomButton);
            let buttonSettings = world.getComponentData(worldMap.ButtonSettings, game.CustomButton);
            let buttonLeft = world.getComponentData(worldMap.ButtonLeft, game.CustomButton);
            let buttonRight = world.getComponentData(worldMap.ButtonRight, game.CustomButton);
            let buttonCutscene = world.getComponentData(worldMap.ButtonCutscene, game.CustomButton);

            if (buttonPlay.JustClicked) {
                let currentLevelIndex = worldMap.CurrentLevelIndex;
                UserDataService.setSelectedWorldMapIndex(currentLevelIndex);

                GameStateLoadingService.transitionToGameStateWithScaledHole(world, game.GameStateTypes.Game, new Vector2(0, 0));

                SoundService.play(world, "GameStartSound");
            }
            else if (buttonLeft.JustClicked || buttonRight.JustClicked) {
                let increment = buttonRight.JustClicked ? 1 : -1;
                let worldMapEntity = world.getEntityByName("WorldMap");
                let worldMap = world.getComponentData(worldMapEntity, game.WorldMap);
                let currentLevelIndex = worldMap.CurrentLevelIndex;
                currentLevelIndex += increment;
                currentLevelIndex = Math.min(2, Math.max(0, currentLevelIndex));
                worldMap.CurrentLevelIndex = currentLevelIndex;
                world.setComponentData(worldMapEntity, worldMap);

                SoundService.play(world, "GenericClickSound");
            }
            else if (buttonSettings.JustClicked) {
                GameStateLoadingService.setGameState(world, game.GameStateTypes.Settings);
                SoundService.play(world, "GenericClickSound");
            }
            else if (buttonCutscene.JustClicked) {
                UserDataService.setSelectedWorldMapIndex(worldMap.CurrentLevelIndex);
                GameStateLoadingService.transitionToGameState(world, game.GameStateTypes.CutsceneEnd);
                SoundService.play(world, "GenericClickSound");
                SoundService.stopMusic(world);
            }
        }

        static updateGame(world: ut.World, gameState: GameState) {
            gameState.Time += world.scheduler().deltaTime();

            let gameUIEntity = world.getEntityByName("GameUI");
            let gameUI = world.getComponentData(gameUIEntity, game.GameUI);
            let buttonPause = world.getComponentData(gameUI.ButtonPause, game.CustomButton);

            let labelTime = world.getComponentData(gameUI.LabelTime, ut.Text.Text2DRenderer);
            let labelObjective = world.getComponentData(gameUI.LabelObjective, ut.Text.Text2DRenderer);
            let levelEntity = GameService.getCurrentLevelEntity(world);
            let isPointObjective = world.hasComponent(levelEntity, game.LevelPointObjective);
            let isSurvivalObjective = world.hasComponent(levelEntity, game.LevelSurvival);
            let isEggObjective = world.hasComponent(levelEntity, game.LevelEggObjective);

            let heldEgg = 0;
            world.forEach([game.CollectedCurrency], (collectedCurrency) => { heldEgg++; });
            let isObjectiveComplete = heldEgg == 0 && game.GameService.isObjectiveCompleted(world);
            game.GameService.setEntityEnabled(world, gameUI.ImageObjectiveCompleteGlow, isObjectiveComplete);
            game.GameService.setEntityEnabled(world, gameUI.ImageObjectivePoint, isPointObjective && isObjectiveComplete);
            game.GameService.setEntityEnabled(world, gameUI.ImageObjectiveEgg, isEggObjective && isObjectiveComplete);
            game.GameService.setEntityEnabled(world, gameUI.ImageObjectiveSurvival, isSurvivalObjective && isObjectiveComplete);
            game.GameService.setEntityEnabled(world, gameUI.ImageObjectivePointIncomplete, isPointObjective && !isObjectiveComplete);
            game.GameService.setEntityEnabled(world, gameUI.ImageObjectiveEggIncomplete, isEggObjective && !isObjectiveComplete);
            game.GameService.setEntityEnabled(world, gameUI.ImageObjectiveSurvivalIncomplete, isSurvivalObjective && !isObjectiveComplete);

            if (isObjectiveComplete) {
                let objectiveGlowTransformRotation = world.getComponentData(gameUI.ImageObjectiveCompleteGlow, ut.Core2D.TransformLocalRotation);
                objectiveGlowTransformRotation.rotation.setFromAxisAngle(new Vector3(0, 0, 1), gameState.Time);
                world.setComponentData(gameUI.ImageObjectiveCompleteGlow, objectiveGlowTransformRotation);

                if (!gameUI.LastIsObjectiveComplete) {
                    gameUI.LastIsObjectiveComplete = true;
                    world.setComponentData(gameUIEntity, gameUI);
                    this.punchScale(world, gameUI.LabelObjective, 1.35);
                    this.punchScale(world, gameUI.ImageObjectivePoint, 1.1);
                    this.punchScale(world, gameUI.ImageObjectiveEgg, 1.1);
                    this.punchScale(world, gameUI.ImageObjectiveSurvival, 1.1);
                }
            }

            if (isSurvivalObjective) {
                labelTime.text = String(game.GameService.formatTime(gameState.Time));
                world.setComponentData(gameUI.LabelTime, labelTime);
            }
            else if (isEggObjective) {
                let eggObjective = world.getComponentData(levelEntity, game.LevelEggObjective);
                let totalToCollect = eggObjective.EggsInGridAtStart + eggObjective.EggsToSpawnOnEggCollected;

                let collectedEgg = eggObjective.CollectedEggs - heldEgg;
                if (collectedEgg > gameUI.LastCollectedEggCount) {
                    gameUI.LastCollectedEggCount = collectedEgg;
                    world.setComponentData(gameUIEntity, gameUI);
                    this.punchScale(world, gameUI.LabelObjective, 1.35);
                    this.punchScale(world, gameUI.ImageObjectiveEggIncomplete, 1.1);
                }

                let remainingEggCount = collectedEgg + "/" + totalToCollect;
                if (remainingEggCount != labelObjective.text) {
                    labelObjective.text = remainingEggCount;
                    world.setComponentData(gameUI.LabelObjective, labelObjective);
                }
            }

            if (buttonPause.JustClicked) {
                GameStateLoadingService.setGameState(world, game.GameStateTypes.Paused);
                SoundService.play(world, "GenericClickSound");
            }
        }

        static punchScale(world: ut.World, entity: ut.Entity, scale: number) {
            let transformScale = world.getComponentData(entity, ut.Core2D.TransformLocalScale);
            let startScale = transformScale.scale;
            let endScale = new Vector3(scale, scale, 1);

            let punchLevelTitleTween = new ut.Tweens.TweenDesc;
            punchLevelTitleTween.cid = ut.Core2D.TransformLocalScale.cid;
            punchLevelTitleTween.offset = 0;
            punchLevelTitleTween.duration = 0.1;
            punchLevelTitleTween.func = ut.Tweens.TweenFunc.OutCubic;
            punchLevelTitleTween.loop = ut.Core2D.LoopMode.PingPongOnce;
            punchLevelTitleTween.destroyWhenDone = true;
            punchLevelTitleTween.t = 0.0;

            ut.Tweens.TweenService.addTweenVector3(world, entity, startScale, endScale, punchLevelTitleTween);
        }

        static updateSettings(world: ut.World, gameState: GameState) {
            let settingsMenuEntity = world.getEntityByName("SettingsMenu");
            let settingsMenu = world.getComponentData(settingsMenuEntity, game.SettingsMenu);
            this.updateAudioSettings(world, settingsMenuEntity);

            let buttonResetProgress = world.getComponentData(settingsMenu.ButtonResetProgress, game.CustomButton);
            let buttonRenderMode = world.getComponentData(settingsMenu.ButtonRenderMode, game.CustomButton);
            let buttonCredits = world.getComponentData(settingsMenu.ButtonCredits, game.CustomButton);
            let buttonLanguages = world.getComponentData(settingsMenu.ButtonLanguage, game.CustomButton);
            let buttonOK = world.getComponentData(settingsMenu.ButtonOK, game.CustomButton);

            if (buttonResetProgress.JustClicked) {
                if (confirm(LocalizationService.getText(world, "Settings_ResetProgressConfirm"))) {
                    game.UserDataService.deleteAllCookies();
                    location.reload();
                }
            }
            else if (buttonRenderMode.JustClicked) {
                let displayInfo = world.getConfigData(ut.Core2D.DisplayInfo);
                displayInfo.renderMode = displayInfo.renderMode == ut.Core2D.RenderMode.WebGL ? ut.Core2D.RenderMode.Canvas : ut.Core2D.RenderMode.WebGL;
                world.setConfigData(displayInfo);

                let labelRenderMode = world.getComponentData(settingsMenu.LabelRenderMode, game.LocalizedText);
                labelRenderMode.TextID = "Settings_Rendering";
                labelRenderMode.TextParameters = [(displayInfo.renderMode == ut.Core2D.RenderMode.WebGL ? "WebGL" : "Canvas")];
                labelRenderMode.LastTextID = ""; // Set dirty to force refresh
                world.setComponentData(settingsMenu.LabelRenderMode, labelRenderMode);
            }
            else if (buttonCredits.JustClicked) {
                GameStateLoadingService.transitionToGameState(world, game.GameStateTypes.Credits);
            }
            else if (buttonLanguages.JustClicked) {
                GameStateLoadingService.setGameState(world, game.GameStateTypes.Languages);
            }
            else if (buttonOK.JustClicked) {
                ut.Tweens.TweenService.removeAllTweensInWorld(world);
                ut.Core2D.TransformService.destroyTree(world, settingsMenuEntity);
                ut.EntityGroup.destroyAll(world, "game.SettingsMenu");
                SoundService.play(world, "GenericClickSound");

                if (world.exists(world.getEntityByName("MainMenu"))) {
                    gameState.GameStateType = GameStateTypes.MainMenu;
                }
                else {
                    gameState.GameStateType = GameStateTypes.WorldMap;
                }
            }
        }

        static updateAudioSettings(world: ut.World, audioSettingsMenuEntity: ut.Entity) {
            let audioSettingsMenu = world.getComponentData(audioSettingsMenuEntity, game.SettingsMenuAudio);

            let buttonSound = world.getComponentData(audioSettingsMenu.ButtonSound, game.CustomButton);
            let buttonMusic = world.getComponentData(audioSettingsMenu.ButtonMusic, game.CustomButton);
            
            if (buttonSound.JustClicked) {
                SoundService.toggleSoundIsOn(world);
                SoundService.play(world, "GenericClickSound");

                let soundSpriteRenderer = world.getComponentData(audioSettingsMenu.ButtonSound, ut.Core2D.Sprite2DRenderer);
                soundSpriteRenderer.sprite = SoundService.getIsSoundOn(world) ? audioSettingsMenu.SpriteAudioOn : audioSettingsMenu.SpriteAudioOff;
                world.setComponentData(audioSettingsMenu.ButtonSound, soundSpriteRenderer);
            }
            else if (buttonMusic.JustClicked) {
                SoundService.toggleMusicIsOn(world);
                SoundService.play(world, "GenericClickSound");

                let musicSpriteRenderer = world.getComponentData(audioSettingsMenu.ButtonMusic, ut.Core2D.Sprite2DRenderer);
                musicSpriteRenderer.sprite = SoundService.getIsMusicOn(world) ? audioSettingsMenu.SpriteAudioOn : audioSettingsMenu.SpriteAudioOff;
                world.setComponentData(audioSettingsMenu.ButtonMusic, musicSpriteRenderer);
            }
        }

        static updatePause(world: ut.World, gameState: GameState) {
            let pauseMenuEntity = world.getEntityByName("PauseMenu");
            let pauseMenu = world.getComponentData(pauseMenuEntity, game.PauseMenu);
            let buttonResume = world.getComponentData(pauseMenu.ButtonResume, game.CustomButton);
            let buttonQuit = world.getComponentData(pauseMenu.ButtonQuit, game.CustomButton);

            this.updateAudioSettings(world, pauseMenuEntity);

            if (buttonResume.JustClicked) {
                gameState.GameStateType = GameStateTypes.Game;

                ut.Core2D.TransformService.destroyTree(world, pauseMenuEntity);
                ut.EntityGroup.destroyAll(world, "game.PauseMenu");

                SoundService.play(world, "GenericClickSound");
            }
            else if (buttonQuit.JustClicked) {
                GameStateLoadingService.transitionToGameState(world, game.GameStateTypes.WorldMap);

                SoundService.play(world, "GenericClickSound");
            }
        }

        static updateGameOver(world: ut.World, gameState: GameState) {
            let gameOverMenuEntity = world.getEntityByName("GameOverMenu");
            let gameOverMenu = world.getComponentData(gameOverMenuEntity, game.GameOverMenu);
            let popup = world.getComponentData(gameOverMenuEntity, game.Popup);
            let buttonQuit = world.getComponentData(gameOverMenu.ButtonQuit, game.CustomButton);

            gameOverMenu.Timer += world.scheduler().deltaTime();
            world.setComponentData(gameOverMenuEntity, gameOverMenu);

            if (!world.hasComponent(gameOverMenu.ImageNewBestScoreGlow, ut.Disabled)) {
                let newBestScoreGlowTransformRotation = world.getComponentData(gameOverMenu.ImageNewBestScoreGlow, ut.Core2D.TransformLocalRotation);
                newBestScoreGlowTransformRotation.rotation.setFromAxisAngle(new Vector3(0, 0, 1), gameOverMenu.Timer);
                world.setComponentData(gameOverMenu.ImageNewBestScoreGlow, newBestScoreGlowTransformRotation);
            }

            if (buttonQuit.JustClicked && gameOverMenu.Timer > popup.DelayIn + popup.ScaleInDuration / 2) {
                SoundService.play(world, "GenericClickSound");
                if (GameService.getCurrentLevel(world).LevelID == 3 && GameService.isObjectiveCompleted(world) && !UserDataService.getHasSeenEndCutscene()) {
                    UserDataService.setHasSeenEndCutscene(true);
                    GameStateLoadingService.transitionToGameState(world, game.GameStateTypes.CutsceneEnd);
                }
                else {
                    GameStateLoadingService.transitionToGameState(world, game.GameStateTypes.WorldMap);
                }
            }
        }

        static updateCredits(world: ut.World, gameState: GameState) {
            let creditsMenuEntity = world.getEntityByName("CreditsMenu");
            let creditsMenu = world.getComponentData(creditsMenuEntity, game.CreditsMenu);
            let buttonClose = world.getComponentData(creditsMenu.ButtonClose, game.CustomButton);

            if (buttonClose.JustClicked) {
                gameState.GameStateType = GameStateTypes.Settings;

                ut.Core2D.TransformService.destroyTree(world, creditsMenuEntity);
                ut.EntityGroup.destroyAll(world, "game.CreditsMenu");

                SoundService.play(world, "GenericClickSound");
            }
        }

        static updateLanguages(world: ut.World, gameState: GameState) {
            let languagesMenuEntity = world.getEntityByName("LanguagesMenu");
            let languagesMenu = world.getComponentData(languagesMenuEntity, game.LanguagesMenu);
            let buttonEnglish = world.getComponentData(languagesMenu.ButtonEnglish, game.CustomButton);
            let buttonFrench = world.getComponentData(languagesMenu.ButtonFrench, game.CustomButton);
            let buttonCancel = world.getComponentData(languagesMenu.ButtonCancel, game.CustomButton);

            if (buttonEnglish.JustClicked) {
                LocalizationService.setLanguageID(world, "en");
            }
            else if (buttonFrench.JustClicked) {
                LocalizationService.setLanguageID(world, "fr");
            }

            if (buttonEnglish.JustClicked || buttonFrench.JustClicked|| buttonCancel.JustClicked) {
                gameState.GameStateType = GameStateTypes.Settings;

                ut.Tweens.TweenService.removeAllTweensInWorld(world);
                ut.Core2D.TransformService.destroyTree(world, languagesMenuEntity);
                ut.EntityGroup.destroyAll(world, "game.LanguagesMenu");

                SoundService.play(world, "GenericClickSound");
            }
        }
    }
}
