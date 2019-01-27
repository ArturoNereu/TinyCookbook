
namespace game {

    export class UpdateCurrentWorldMapItemSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            let deltaTime = this.scheduler.deltaTime();

            let currentLevelIndex = 0;
            let currentIndexPosition = 0;
            this.world.forEach([game.WorldMap], (worldMap) => {

                currentLevelIndex = worldMap.CurrentLevelIndex;

                // Animate level scroll animation.
                {
                    currentIndexPosition = worldMap.CurrentScrollIndexPosition;
                    let distanceToDestination = Math.abs(currentLevelIndex - currentIndexPosition);
                    let moveDirection = (currentLevelIndex - currentIndexPosition) > 0 ? 1 : -1;
    
                    let speedRatio = 1;
                    let scrollSpeed = worldMap.ScrollSpeed;
                    if (distanceToDestination < 0.4) {
                        speedRatio = distanceToDestination / 0.25;
                    }
                    let speed = scrollSpeed * Math.max(0.2, speedRatio);
                    currentIndexPosition += moveDirection * deltaTime * speed;
    
                    if (distanceToDestination < 0.05) {
                        currentIndexPosition = currentLevelIndex;
                    }
    
                    worldMap.CurrentScrollIndexPosition = currentIndexPosition;
                }

                // Update current level title and info.
                if (worldMap.CurrentLevelIndex != worldMap.LastLevelIndex || LocalizationService.getLanguageID(this.world) != worldMap.LastLanguageID) {

                    let levelEntity = GameService.getLevelEntity(this.world, currentLevelIndex + 1);
                    let level = GameService.getLevel(this.world, currentLevelIndex + 1);

                    // Update button visibility.
                    let buttonLeft = this.world.getComponentData(worldMap.ButtonLeft, CustomButton);
                    buttonLeft.IsInteractable = currentLevelIndex > 0;
                    this.world.setComponentData(worldMap.ButtonLeft, buttonLeft);

                    let buttonRight = this.world.getComponentData(worldMap.ButtonRight, CustomButton);
                    buttonRight.IsInteractable = currentLevelIndex < 2;
                    this.world.setComponentData(worldMap.ButtonRight, buttonRight);

                    let showCutsceneButton = currentLevelIndex == 2 && worldMap.LastBeatenLevelID >= 3;
                    let buttonCutscene = this.world.getComponentData(worldMap.ButtonCutscene, game.CustomButton);
                    buttonCutscene.IsInteractable = showCutsceneButton;
                    this.world.setComponentData(worldMap.ButtonCutscene, buttonCutscene);
                    let buttonCutsceneRenderer = this.world.getComponentData(worldMap.ButtonCutscene, ut.Core2D.Sprite2DRenderer);
                    buttonCutsceneRenderer.color = showCutsceneButton ? new ut.Core2D.Color(1, 1, 1, 1) : new ut.Core2D.Color(1, 1, 1, 0);
                    this.world.setComponentData(worldMap.ButtonCutscene, buttonCutsceneRenderer);
                    let labelCutsceneButton = this.world.getComponentData(worldMap.LabelButtonCutscene, ut.Text.Text2DStyle);
                    labelCutsceneButton.color = new ut.Core2D.Color(labelCutsceneButton.color.r, labelCutsceneButton.color.g, labelCutsceneButton.color.b, showCutsceneButton ? 1 : 0);
                    this.world.setComponentData(worldMap.LabelButtonCutscene, labelCutsceneButton);

                    // Update baked title sprite.
                    let spriteName = game.SkinService.getSkinName(level.Skin);
                    let spritePath = "assets/sprites/WorldMap/" + spriteName;
                    let titleSpriteRenderer = this.world.getComponentData(worldMap.LabelLevelTitle, ut.Core2D.Sprite2DRenderer);
                    titleSpriteRenderer.sprite = this.world.getEntityByName(spritePath + "_Title");
                    this.world.setComponentData(worldMap.LabelLevelTitle, titleSpriteRenderer);
                    let titleSpriteRectTransform = this.world.getComponentData(worldMap.LabelLevelTitle, ut.UILayout.RectTransform);
                    let titleSprite = this.world.getComponentData(titleSpriteRenderer.sprite, ut.Core2D.Sprite2D);
                    let titleImage = this.world.getComponentData(titleSprite.image, ut.Core2D.Image2D);
                    let width = titleSprite.imageRegion.width * titleImage.imagePixelSize.x;
                    let height = titleSprite.imageRegion.height * titleImage.imagePixelSize.y;
                    titleSpriteRectTransform.sizeDelta.x = width;
                    titleSpriteRectTransform.sizeDelta.y = height;
                    this.world.setComponentData(worldMap.LabelLevelTitle, titleSpriteRectTransform);

                    let isPointObjective = this.world.hasComponent(levelEntity, game.LevelPointObjective);
                    let isEggObjective = this.world.hasComponent(levelEntity, game.LevelEggObjective);
                    let isSurvivalObjective = this.world.hasComponent(levelEntity, game.LevelSurvival);
    
                    // Refresh level objective description.
                    {
                        let labelLevelInfoLine1 = this.world.getComponentData(worldMap.LabelLevelInfoLine1, ut.Text.Text2DRenderer);
                        let labelLevelInfoLine2 = this.world.getComponentData(worldMap.LabelLevelInfoLine2, ut.Text.Text2DRenderer);
                        labelLevelInfoLine1.text = "";
                        labelLevelInfoLine2.text = "";

                        let strObjective = "";
                        if (isPointObjective) {
                            let pointObjective = this.world.getComponentData(levelEntity, game.LevelPointObjective);
                            strObjective = LocalizationService.getText(this.world, "Objective_Points", String(pointObjective.ScoreObjective), String(level.MaxMoveCount));
                        }
                        else if (isEggObjective) {
                            let eggObjective = this.world.getComponentData(levelEntity, game.LevelEggObjective);
                            let eggCount = eggObjective.EggsInGridAtStart + eggObjective.EggsToSpawnOnEggCollected;
                            strObjective = LocalizationService.getText(this.world, "Objective_Eggs", String(eggCount), String(level.MaxMoveCount));
                        }
                        else if (isSurvivalObjective) {
                            let survivalObjective = this.world.getComponentData(levelEntity, game.LevelSurvival);
                            strObjective = LocalizationService.getText(this.world, "Objective_Survival", GameService.formatTime(survivalObjective.TimeObjective));
                        }

                        let words = strObjective.split(" ");
                        words.forEach(word => {
                            if (labelLevelInfoLine1.text.length + word.length <= 25) {
                                labelLevelInfoLine1.text += word + " ";
                            }
                            else {
                                labelLevelInfoLine2.text += word + " ";
                            }
                        });

                        this.world.setComponentData(worldMap.LabelLevelInfoLine1, labelLevelInfoLine1);
                        this.world.setComponentData(worldMap.LabelLevelInfoLine2, labelLevelInfoLine2);
                    }

                    game.GameService.setEntityEnabled(this.world, worldMap.ImagePointObjective, isPointObjective);
                    game.GameService.setEntityEnabled(this.world, worldMap.ImageEggObjective, isEggObjective);
                    game.GameService.setEntityEnabled(this.world, worldMap.ImageSurvivalObjective, isSurvivalObjective);

                    let isLevelUnlocked = currentLevelIndex <= worldMap.LastBeatenLevelID;

                    let buttonPlay = this.world.getComponentData(worldMap.ButtonPlay, game.CustomButton);
                    buttonPlay.IsInteractable = isLevelUnlocked;
                    this.world.setComponentData(worldMap.ButtonPlay, buttonPlay);

                    let labelPlayButton = this.world.getComponentData(worldMap.LabelButtonPlay, game.CustomLabel);
					let labelColor = this.world.getComponentData(worldMap.LabelButtonPlay, ut.Text.Text2DStyle);
                    let playButtonTextColor = isLevelUnlocked ? worldMap.PlayButtonColor : worldMap.PlayButtonDisabledColor;
                    if (labelColor.color != playButtonTextColor) {
                        labelColor.color = playButtonTextColor;
                        labelPlayButton.LastText = ""; // Set dirty
                        this.world.setComponentData(worldMap.LabelButtonPlay, labelPlayButton);
						this.world.setComponentData(worldMap.LabelButtonPlay, labelColor);
                    }

                    worldMap.ImageLock.forEach(imageEntity => {
                        game.GameService.setEntityEnabled(this.world, imageEntity, !isLevelUnlocked);
                    });

                    // Punch animate level title.
                    if (worldMap.LastLevelIndex != -1) {
                        let transformScale = this.world.getComponentData(worldMap.LabelLevelTitle, ut.Core2D.TransformLocalScale);
                        let startScale = transformScale.scale;
                        let endScale = new Vector3(1.2, 1.2, 1);
    
                        let punchLevelTitleTween = new ut.Tweens.TweenDesc;
                        punchLevelTitleTween.cid = ut.Core2D.TransformLocalScale.cid;
                        punchLevelTitleTween.offset = 0;
                        punchLevelTitleTween.duration = 0.1;
                        punchLevelTitleTween.func = ut.Tweens.TweenFunc.OutCubic;
                        punchLevelTitleTween.loop = ut.Core2D.LoopMode.PingPongOnce;
                        punchLevelTitleTween.destroyWhenDone = true;
                        punchLevelTitleTween.t = 0.0;
    
                        ut.Tweens.TweenService.addTweenVector3(
                            this.world,
                            worldMap.LabelLevelTitle,
                            startScale,
                            endScale,
                            punchLevelTitleTween);
                    }

                    // Update best score.
                    let bestScore = game.UserDataService.getBestScore(currentLevelIndex + 1);
                    let labelBestScore = this.world.getComponentData(worldMap.LabelBestScore, ut.Text.Text2DRenderer);
                    if (bestScore == 0) {
                        labelBestScore.text = "---";
                    }
                    else {
                        labelBestScore.text = game.GameService.formatNumber(bestScore) + " pts";
                    }
                    this.world.setComponentData(worldMap.LabelBestScore, labelBestScore);
                    
                    worldMap.LastLevelIndex = worldMap.CurrentLevelIndex;
                    worldMap.LastLanguageID = LocalizationService.getLanguageID(this.world);
                }

                // Update sky and ground sprite and transition sprite.
                if (worldMap.CurrentScrollIndexPosition != worldMap.LastScrollIndexPosition) {
                    let environmentSpritePath = "assets/sprites/WorldMap/";
                    let currentLevelSkinName = SkinService.getSkinName(GameService.getLevel(this.world, Math.floor(currentIndexPosition) + 1).Skin);
                    let transitionLevelSkinName = SkinService.getSkinName(GameService.getLevel(this.world, Math.ceil(currentIndexPosition) + 1).Skin);

                    // Sky
                    let skySpriteRenderer = this.world.getComponentData(worldMap.Sky, ut.Core2D.Sprite2DRenderer);
                    skySpriteRenderer.sprite = this.world.getEntityByName(environmentSpritePath + currentLevelSkinName + "_Sky");
                    this.world.setComponentData(worldMap.Sky, skySpriteRenderer);
                    let skyTransitionSpriteRenderer = this.world.getComponentData(worldMap.SkyTransition, ut.Core2D.Sprite2DRenderer);
                    skyTransitionSpriteRenderer.sprite = this.world.getEntityByName(environmentSpritePath + transitionLevelSkinName + "_Sky");
                    skyTransitionSpriteRenderer.color.a = currentIndexPosition % 1;
                    this.world.setComponentData(worldMap.SkyTransition, skyTransitionSpriteRenderer);
                    
                    // Ground
                    let groundSpriteRenderer = this.world.getComponentData(worldMap.Ground, ut.Core2D.Sprite2DRenderer);
                    groundSpriteRenderer.sprite = this.world.getEntityByName(environmentSpritePath + currentLevelSkinName + "_Ground");
                    this.world.setComponentData(worldMap.Ground, groundSpriteRenderer);
                    let groundTransitionSpriteRenderer = this.world.getComponentData(worldMap.GroundTransition, ut.Core2D.Sprite2DRenderer);
                    groundTransitionSpriteRenderer.sprite = this.world.getEntityByName(environmentSpritePath + transitionLevelSkinName + "_Ground");
                    groundTransitionSpriteRenderer.color.a = currentIndexPosition % 1;
                    this.world.setComponentData(worldMap.GroundTransition, groundTransitionSpriteRenderer);

                    worldMap.LastScrollIndexPosition = worldMap.CurrentScrollIndexPosition;
                }
            });

            // Update position and scale of all world map level items.
            this.world.forEach([game.WorldMapItem, ut.UILayout.RectTransform, ut.Core2D.TransformLocalScale], (worldMapItem, rectTransform, transformLocalScale) => {
                let position = rectTransform.anchoredPosition;
                position.x = (worldMapItem.Index - currentIndexPosition) * worldMapItem.DistanceBetweenLevelItems;
                rectTransform.anchoredPosition = position;

                let scale = Math.min(Math.abs(position.x), worldMapItem.DistanceBetweenLevelItems) / worldMapItem.DistanceBetweenLevelItems;
                scale = worldMapItem.UnfocusedScale + (worldMapItem.FocusedScale - worldMapItem.UnfocusedScale) * (1 - scale);
                transformLocalScale.scale = new Vector3(scale, scale, 1);

                let levelPreviewEntity = worldMapItem.ImageLevelPreview;
                let levelPreviewSprite = this.world.getComponentData(levelPreviewEntity, ut.Core2D.Sprite2DRenderer);
                levelPreviewSprite.color.a = scale;
                this.world.setComponentData(levelPreviewEntity, levelPreviewSprite);
            });
        }
    }
}
