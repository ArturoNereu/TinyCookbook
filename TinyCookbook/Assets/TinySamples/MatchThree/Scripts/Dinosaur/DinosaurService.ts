
namespace game {

    export class DinosaurService {
        
        static dinosaurEntity: ut.Entity;

        static getDinosaurEntity(world: ut.World): ut.Entity {

            if (!world.exists(this.dinosaurEntity)) {
                this.dinosaurEntity = world.getEntityByName("Dinosaur");
                if (!world.exists(this.dinosaurEntity)) {
                    this.dinosaurEntity = null;
                    return null;
                }
            }

            return this.dinosaurEntity;
        }

        static getDinosaur(world: ut.World): game.Dinosaur {
            let dinosaurEntity = this.getDinosaurEntity(world);
            if (dinosaurEntity == null) {
                return null;
            }
            else {
                return world.getComponentData(dinosaurEntity, game.Dinosaur);
            }
        }

        static startAnimation(world: ut.World, dinosaur: game.Dinosaur, dinosaurAttack: game.DinosaurAttack): void {

            GameService.setEntityEnabled(world, dinosaur.WalkAnimation, false);

            let animation: ut.Entity = null;
            switch (dinosaurAttack.AttackType) {
                case game.DinosaurAttackTypes.TailWhip: {
                    animation = dinosaur.TailWhipAnimation;
                    break;
                }
                case game.DinosaurAttackTypes.Stomp: {
                    animation = dinosaur.StompAnimation;
                    break;
                }
                case game.DinosaurAttackTypes.Bite: {
                    animation = dinosaur.BiteAnimation;
                    break;
                }
                case game.DinosaurAttackTypes.Crush: {
                    animation = dinosaur.CrushAnimation;
                    break;
                }
                case game.DinosaurAttackTypes.Launch: {
                    animation = dinosaur.LaunchAnimation;
                    break;
                }
                case game.DinosaurAttackTypes.Jump: {
                    animation = dinosaur.JumpAnimation;
                    break;
                }
                case game.DinosaurAttackTypes.Laser: {
                    animation = dinosaur.LaserAnimation;
                    break;
                }
                default: {
                    animation = null;
                    break;
                }
            }

            if (animation != null) {
                GameService.setEntityEnabled(world, animation, true);
                world.usingComponentData(animation, [ut.Core2D.Sprite2DSequencePlayer], (seqPlayer) => seqPlayer.time = 0);
            }
        }

        /**
         * Get the delay after which the dinosaur triggers its attack effects (damages) after starting the animation.
         */
        static getAnimationEffectTime(dinosaurAttack: game.DinosaurAttack): number {

            switch (dinosaurAttack.AttackType) {
                case game.DinosaurAttackTypes.TailWhip:
                    return 0.8;
                case game.DinosaurAttackTypes.Stomp:
                    return 0.7;
                case game.DinosaurAttackTypes.Bite:
                    return 0.6;
                case game.DinosaurAttackTypes.Crush:
                    return 0.9;
                case game.DinosaurAttackTypes.Launch:
                    return 0.6;
                case game.DinosaurAttackTypes.Jump:
                    return 1.3;
                case game.DinosaurAttackTypes.Laser:
                    return 0.4;
                default:
                    return 0;
            }
        }

        static getAnimationDuration(dinosaurAttack: game.DinosaurAttack): number {

            switch (dinosaurAttack.AttackType) {
                case game.DinosaurAttackTypes.TailWhip:
                    return 1;
                case game.DinosaurAttackTypes.Stomp:
                    return 1;
                case game.DinosaurAttackTypes.Bite:
                    return 0.9;
                case game.DinosaurAttackTypes.Crush:
                    return 1.4;
                case game.DinosaurAttackTypes.Launch:
                    return 1.0;
                case game.DinosaurAttackTypes.Jump:
                    return 1.5;
                case game.DinosaurAttackTypes.Laser:
                    return 1.5;
                default:
                    return 1;
            }
        }

        static triggerKidOnBike(world: ut.World, dinosaurEntity: ut.Entity): void {
            let dinosaurTransformPosition = world.getComponentData(dinosaurEntity, ut.Core2D.TransformLocalPosition);

            world.forEach([game.KidOnBike, ut.Core2D.TransformLocalPosition],
                (kid, transformLocalPosition) => {
                    if (Math.abs(transformLocalPosition.position.x - dinosaurTransformPosition.position.x) < 70) {
                        kid.IsInWheelyMode = true;
                    }
                });
        }

        static performAttack(world: ut.World, dinosaurEntity: ut.Entity, dinosaur: game.Dinosaur, dinosaurAttack: game.DinosaurAttack): void {

            dinosaurAttack.IsDone = true;
            world.setComponentData(dinosaurEntity, dinosaurAttack);

            switch (dinosaurAttack.AttackType) {
                case game.DinosaurAttackTypes.Jump:
                    this.performAttackOnObjectType(world, game.FireTruck, -45, 60, true, false);
                    break;
                case game.DinosaurAttackTypes.Stomp:
                    this.performAttackOnObjectType(world, game.Prop, -45, 40, true, false);
                    break;
                case game.DinosaurAttackTypes.Launch:
                    this.performLaunchAttack(world, -10, 30);
                    break;
                case game.DinosaurAttackTypes.Crush:
                    this.performAttackOnObjectType(world, game.Helicopter, -30, 50, false, true);
                    break;
                case game.DinosaurAttackTypes.Bite:
                    this.performAttackOnObjectType(world, game.Person, 0, 25, false, false);
                    break;
                case game.DinosaurAttackTypes.Laser: {
                    let centerPosition = 105;
                    let radius = 115;
                    this.performAttackOnObjectType(world, game.Building, centerPosition, radius, true, false);
                    this.performAttackOnObjectType(world, game.Person, centerPosition, radius, false, false);
                    this.performAttackOnObjectType(world, game.Prop, centerPosition, radius, false, false);
                    this.performAttackOnObjectType(world, game.FireTruck, centerPosition, radius, false, false);
                    this.performAttackOnObjectType(world, game.Car, centerPosition, radius, false, false);
                    SoundService.play(world, "Lazer");
                    break;
                }
                case game.DinosaurAttackTypes.TailWhip:
                default:
                    this.performAttackOnObjectType(world, game.Building, 0, 30, false, true);
                    break;
                    
            }
        }

        /**
         * Damage/destroy destructible objects within a range from the dinosaur.
         */
        static performAttackOnObjectType(world: ut.World, targetObjectType: any, hitCenter: number, hitRadius: number, shakeOnAttack: boolean, shakeOnHit: boolean): void {

            if (shakeOnAttack) {
                this.shakeCamera(world);
            }

            let entitiesToDestroy: ut.Entity[] = new Array();
            world.forEach([ut.Entity, targetObjectType, game.Destructible, ut.Core2D.TransformLocalPosition],
                (entity, type, destructible, transformPosition) => {
                    if (Math.abs(transformPosition.position.x - hitCenter) < hitRadius) {
                        if (destructible.SpriteStates.length > 1) {
                            this.damageDestructible(destructible);
                        }
                        else {
                            let entityToDestroy = new ut.Entity();
                            entityToDestroy.index = entity.index;
                            entityToDestroy.version = entity.version;
                            entitiesToDestroy.push(entityToDestroy);
                        }

                        this.spawnExplosions(world, destructible, transformPosition);

                        if (shakeOnHit) {
                            this.shakeCamera(world);
                        }
                    }
                });

            for (let i = 0; i < entitiesToDestroy.length; i++) {
                ut.Core2D.TransformService.destroyTree(world, entitiesToDestroy[i], true);
            }
        }

        static damageDestructible(destructible: game.Destructible) : void{
            let spriteState = destructible.CurrentState;
            spriteState++;
            if (spriteState >= destructible.SpriteStates.length) {
                spriteState = destructible.SpriteStates.length - 1;
            }

            destructible.CurrentState = spriteState;
        }

        static spawnExplosions(world: ut.World, destructible: game.Destructible, transformPosition: ut.Core2D.TransformLocalPosition): void {

            let explosionCount = destructible.ExplosionMinCount + Math.floor(Math.random() * (destructible.ExplosionMaxCount - destructible.ExplosionMinCount));

            for (let i = 0; i < explosionCount; i++) {

                let explosionPosition = new Vector2(transformPosition.position.x, transformPosition.position.y);
                explosionPosition.x += destructible.ExplosionOffsetX + Math.random() * destructible.ExplosionRangeX * 2 - destructible.ExplosionRangeX;
                explosionPosition.y += destructible.ExplosionOffsetY + Math.random() * destructible.ExplosionRangeY * 2 - destructible.ExplosionRangeY;
                
                let explosionID = 1 + i % 2;
                let explosionEntity = ut.EntityGroup.instantiate(world, "game.Explosion" + explosionID)[0];
                let explosionTransform = world.getComponentData(explosionEntity, ut.Core2D.TransformLocalPosition);
                explosionTransform.position = new Vector3(explosionPosition.x, explosionPosition.y, explosionTransform.position.z);
                world.setComponentData(explosionEntity, explosionTransform);

                // Spawn explosion particles
                let particleEntity = ut.EntityGroup.instantiate(world, "game.ParticleExplosion")[0];
                let explosionTransformPosition = world.getComponentData(particleEntity, ut.Core2D.TransformLocalPosition);
                explosionTransformPosition.position = new Vector3(explosionPosition.x, explosionPosition.y, 0);
                world.setComponentData(particleEntity, explosionTransformPosition);
            }

            if (destructible.ExplosionMinCount > 0) {
                let explosionSound = "Destruction" + (1 + Math.floor(Math.random() * 2));
                SoundService.play(world, explosionSound);
            }
        }

        /**
         * Throw a car into the sky, if there is one in front of the dinosaur.
         */
        static performLaunchAttack(world: ut.World, hitCenter: number, hitRadius: number): void {

            world.forEach([ut.Entity, game.Car, game.Destructible, game.ScrollingObject, ut.Core2D.TransformLocalPosition, ut.Core2D.TransformLocalRotation],
                (entity, car, destructible, scrollingObject, transformPosition, transformRotation) => {
                    if (Math.abs(transformPosition.position.x - hitCenter) < hitRadius) {
                        if (destructible.SpriteStates.length > 1) {
                            this.damageDestructible(destructible);
                        }

                        this.spawnExplosions(world, destructible, transformPosition);
                        
                        world.removeComponent(entity, game.ScrollingObject);
                        let destroyAfterDelay = new game.DestroyAfterDelay();
                        destroyAfterDelay.Delay = 2;
                        world.addComponentData(entity, destroyAfterDelay)

                        // Move tween
                        {
                            let startPosition = new Vector3(transformPosition.position.x, transformPosition.position.y, 0);
                            let endPosition = new Vector3(transformPosition.position.x, 300, 0);
                            transformPosition.position = startPosition;
    
                            let moveTween = new ut.Tweens.TweenDesc;
                            moveTween.cid = ut.Core2D.TransformLocalPosition.cid;
                            moveTween.offset = 0;
                            moveTween.duration = 0.4;
                            moveTween.func = ut.Tweens.TweenFunc.Linear;
                            moveTween.loop = ut.Core2D.LoopMode.Once;
                            moveTween.destroyWhenDone = true;
                            moveTween.t = 0.0;
    
                            ut.Tweens.TweenService.addTweenVector3(
                                world,
                                entity,
                                startPosition,
                                endPosition,
                                moveTween);
                        }
                        
                        // Rotate tween
                        {
                            let startRotation = new Quaternion().setFromAxisAngle(new Vector3(0, 0, 1), 0);
                            let endRotation = new Quaternion().setFromAxisAngle(new Vector3(0, 0, 1), Math.random() * 720 - 360);
                            transformRotation.rotation = startRotation;
    
                            let rotateTween = new ut.Tweens.TweenDesc;
                            rotateTween.cid = ut.Core2D.TransformLocalRotation.cid;
                            rotateTween.offset = 0;
                            rotateTween.duration = 0.45;
                            rotateTween.func = ut.Tweens.TweenFunc.Linear;
                            rotateTween.loop = ut.Core2D.LoopMode.Once;
                            rotateTween.destroyWhenDone = true;
                            rotateTween.t = 0.0;
    
                            ut.Tweens.TweenService.addTweenQuaternion(
                                world,
                                entity,
                                startRotation,
                                endRotation,
                                rotateTween);
                        }

                        this.spawnFallingCar(world);
                    }
                });
        }

        static spawnFallingCar(world: ut.World): void {

            let entity = ut.EntityGroup.instantiate(world, "game.CarSkyFall")[0];
            let transformPosition = world.getComponentData(entity, ut.Core2D.TransformLocalPosition);
            let transformRotation = world.getComponentData(entity, ut.Core2D.TransformLocalRotation);

            // Move tween
            {
                let xPosition = Math.random() * 100 - 50;
                let startPosition = new Vector3(xPosition, 300, 0);
                let endPosition = new Vector3(xPosition, 54, 0);
                transformPosition.position = startPosition;
                world.setComponentData(entity, transformPosition);

                let moveTween = new ut.Tweens.TweenDesc;
                moveTween.cid = ut.Core2D.TransformLocalPosition.cid;
                moveTween.offset = 0;
                moveTween.duration = 1.9;
                moveTween.func = ut.Tweens.TweenFunc.Linear;
                moveTween.loop = ut.Core2D.LoopMode.Once;
                moveTween.destroyWhenDone = true;
                moveTween.t = 0.0;

                ut.Tweens.TweenService.addTweenVector3(
                    world,
                    entity,
                    startPosition,
                    endPosition,
                    moveTween);
            }

            // Rotate tween
            {
                let startRotation = new Quaternion().setFromAxisAngle(new Vector3(0, 0, 1), 0);
                let endRotation = new Quaternion().setFromAxisAngle(new Vector3(0, 0, 1), Math.random() * 720 - 360);
                transformRotation.rotation = startRotation;
                world.setComponentData(entity, transformRotation);

                let rotateTween = new ut.Tweens.TweenDesc;
                rotateTween.cid = ut.Core2D.TransformLocalRotation.cid;
                rotateTween.offset = 0;
                rotateTween.duration = 1.95;
                rotateTween.func = ut.Tweens.TweenFunc.Linear;
                rotateTween.loop = ut.Core2D.LoopMode.Once;
                rotateTween.destroyWhenDone = true;
                rotateTween.t = 0.0;

                ut.Tweens.TweenService.addTweenQuaternion(
                    world,
                    entity,
                    startRotation,
                    endRotation,
                    rotateTween);
            }
        }

        static shakeCamera(world: ut.World, startDelay: number = 0): void {
            let cameraEntity = world.getEntityByName("Camera");
            if (world.exists(cameraEntity))
            {
                if (world.hasComponent(cameraEntity, game.ShakeAnimationPlayer)) {
                    let shakePlayer = world.getComponentData(cameraEntity, game.ShakeAnimationPlayer);
                    shakePlayer.Timer = 0;
                    world.setComponentData(cameraEntity, shakePlayer);
                }
                else {
                    let cameraShakePlayer = new game.ShakeAnimationPlayer();
                    cameraShakePlayer.Duration = 0.5;
                    cameraShakePlayer.StartDelay = startDelay;
                    cameraShakePlayer.ShakeRadiusX = 6;
                    cameraShakePlayer.ShakeRadiusY = 6;
                    world.addComponentData(cameraEntity, cameraShakePlayer);
                }
            }
        }

        /**
         * Trigger death animation if objective is incomplete.
         */
        static setEndGameAnimation(world: ut.World, isObjectiveComplete: boolean): void {

            let dinosaurEntity = world.getEntityByName("Dinosaur");
            let dinosaur = world.getComponentData(dinosaurEntity, game.Dinosaur);

            if (world.hasComponent(dinosaurEntity, game.DinosaurAttack)) {
                world.removeComponent(dinosaurEntity, game.DinosaurAttack);
            }

            GameService.setEntityEnabled(world, dinosaur.TailWhipAnimation, false);
            GameService.setEntityEnabled(world, dinosaur.StompAnimation, false);
            GameService.setEntityEnabled(world, dinosaur.BiteAnimation, false);
            GameService.setEntityEnabled(world, dinosaur.CrushAnimation, false);
            GameService.setEntityEnabled(world, dinosaur.LaunchAnimation, false);
            GameService.setEntityEnabled(world, dinosaur.JumpAnimation, false);
            GameService.setEntityEnabled(world, dinosaur.LaserAnimation, false);
            GameService.setEntityEnabled(world, dinosaur.LaserBeam1, false);
            GameService.setEntityEnabled(world, dinosaur.LaserBeam2, false);

            let isDead = !isObjectiveComplete;
            GameService.setEntityEnabled(world, dinosaur.WalkAnimation, !isDead);
            GameService.setEntityEnabled(world, dinosaur.DeathAnimation, isDead);

            if (isDead) {
                this.shakeCamera(world, 0.85);
            }
        }
    }
}
