
namespace game {

    export class PerformDinosaurAttackSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            let gameState = GameService.getGameState(this.world).GameStateType;
            if (!(gameState == game.GameStateTypes.Game || gameState == game.GameStateTypes.Paused)) {
                return;
            }

            let dinosaurEntity = DinosaurService.getDinosaurEntity(this.world);
            if (!this.world.hasComponent(dinosaurEntity, game.DinosaurAttack)) {
                return;
            }

            let dinosaur = DinosaurService.getDinosaur(this.world);
            let dinosaurAttack = this.world.getComponentData(dinosaurEntity, game.DinosaurAttack);
            let attackTime = dinosaurAttack.Timer;
            let isAttacking = attackTime < DinosaurService.getAnimationDuration(dinosaurAttack);

            if (attackTime == 0) {
                DinosaurService.startAnimation(this.world, dinosaur, dinosaurAttack);
                dinosaur.DefaultPositionY = this.world.getComponentData(dinosaur.JumpAnimation, ut.Core2D.TransformLocalPosition).position.y;
            }
            
            // Perform the dinosaur's attack effect (destruction).
            if (!dinosaurAttack.IsDone && attackTime >= DinosaurService.getAnimationEffectTime(dinosaurAttack)) {
                DinosaurService.performAttack(this.world, dinosaurEntity, dinosaur, dinosaurAttack);
                DinosaurService.triggerKidOnBike(this.world, dinosaurEntity);
            }

            // Update the dinosaur laser animation if it's playing.
            if (isAttacking && dinosaurAttack.AttackType == DinosaurAttackTypes.Laser) {

                DinosaurService.shakeCamera(this.world);

                if (attackTime == 0) {
                    ut.EntityGroup.instantiate(this.world, "game.DinosaurLaserAttackBlackCover");
                }

                let blackCoverEntity = this.world.getEntityByName("DinosaurLaserAttackBlackCover");
                let alphaCurve = this.world.getComponentData(blackCoverEntity, game.DinosaurLaserAttackBlackCover).AlphaCurve;
                let progress = attackTime / DinosaurService.getAnimationDuration(dinosaurAttack);
                let alpha = ut.Interpolation.InterpolationService.evaluateCurveFloat(this.world, progress, alphaCurve);
                let childCount = ut.Core2D.TransformService.countChildren(this.world, blackCoverEntity);
                for (let i = 0; i < childCount; i++) {
                    let spriteEntity = ut.Core2D.TransformService.getChild(this.world, blackCoverEntity, i);
                    let spriteRenderer = this.world.getComponentData(spriteEntity, ut.Core2D.Sprite2DRenderer);
                    spriteRenderer.color = new ut.Core2D.Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha * 0.65);
                    this.world.setComponentData(spriteEntity, spriteRenderer);
                }

                let animationSequence = this.world.getComponentData(dinosaur.LaserAnimation, ut.Core2D.Sprite2DSequencePlayer);
                let showLaser = animationSequence.time > DinosaurService.getAnimationEffectTime(dinosaurAttack);
                GameService.setEntityEnabled(this.world, dinosaur.LaserBeam1, showLaser);
                GameService.setEntityEnabled(this.world, dinosaur.LaserBeam2, showLaser);
            }

            attackTime += this.scheduler.deltaTime();
            dinosaurAttack.Timer = attackTime;
            this.world.setComponentData(dinosaurEntity, dinosaurAttack);

            // Update the dinosaur jump position when the jumping animation is playing.
            if (isAttacking && dinosaurAttack.AttackType == game.DinosaurAttackTypes.Jump) {
                let animationProgress = attackTime / DinosaurService.getAnimationDuration(dinosaurAttack);
                let heightRatio = ut.Interpolation.InterpolationService.evaluateCurveFloat(this.world, animationProgress, dinosaur.JumpAnimationHeightCurve);
                
                this.world.usingComponentData(dinosaur.JumpAnimation, [ut.Core2D.TransformLocalPosition], (jumpAnimationTransform) => {
                    let currentHeight = dinosaur.JumpHeight * heightRatio;
                    let position = jumpAnimationTransform.position;
                    position.y = dinosaur.DefaultPositionY;
                    position.y += currentHeight;
                    jumpAnimationTransform.position = position;
                });

                let shadowScale = 1 - heightRatio;
                this.world.usingComponentData(dinosaur.Shadow, [ut.Core2D.TransformLocalScale], (scale) => {
                    scale.scale = new Vector3(shadowScale, shadowScale, 1);
                });
            }

            // Stop all animations at the end of the attack and start the walking animation.
            if (!isAttacking) {
                this.world.removeComponent(dinosaurEntity, game.DinosaurAttack);

                GameService.setEntityEnabled(this.world, dinosaur.TailWhipAnimation, false);
                GameService.setEntityEnabled(this.world, dinosaur.StompAnimation, false);
                GameService.setEntityEnabled(this.world, dinosaur.BiteAnimation, false);
                GameService.setEntityEnabled(this.world, dinosaur.CrushAnimation, false);
                GameService.setEntityEnabled(this.world, dinosaur.LaunchAnimation, false);
                GameService.setEntityEnabled(this.world, dinosaur.JumpAnimation, false);
                GameService.setEntityEnabled(this.world, dinosaur.LaserAnimation, false);
                GameService.setEntityEnabled(this.world, dinosaur.LaserBeam1, false);
                GameService.setEntityEnabled(this.world, dinosaur.LaserBeam2, false);

                dinosaur.WalkTimer = 0;
                GameService.setEntityEnabled(this.world, dinosaur.WalkAnimation, true)
                this.world.usingComponentData(dinosaur.WalkAnimation, [ut.Core2D.Sprite2DSequencePlayer], (seqPlayer) => seqPlayer.time = 0);
            }

            this.world.setComponentData(dinosaurEntity, dinosaur);
        }
    }
}
