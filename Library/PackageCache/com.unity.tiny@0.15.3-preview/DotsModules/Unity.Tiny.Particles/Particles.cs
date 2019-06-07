using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Tiny.Interpolation;
using Unity.Tiny;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
/// <summary>
///  The Particles module allows you to create the particle effects such as smoke,
///  fire, explosions, and simple liquid effects.
///
/// The most important component is ParticleEmitter, which allows you to customize
///  emitter parameters such as the rate of emission, the number of particles emitted
///  the lifetime of emitted particles, and so on.
///
///  When setting up a particle system, you must also create an entity for the emitter
///  to use as a particle template. This is usually an entity with a Sprite2Sprite2DRenderer
///  component.
///
///  Other components  work with the emitter to control particle parameters such as
///  velocity, rotation, tint, scale, and so on, both initially and over the lifetime
///  of the particles.
/// </summary>
[assembly: ModuleDescription("Unity.Tiny.Particles", "Basic particle system functionality")]
namespace Unity.Tiny.Particles
{
    [IdAlias("0617b7f4431866888bfdb077d683c2ce")]
    public struct Range
    {
        public float start;
        public float end;
    }

    /// <summary>
    ///  Each spawned particle has this component attached to it automatically.
    /// </summary>
    public struct Particle : IComponentData
    {
        /// <summary>How long this particle has existed, in seconds.  From 0.0 to lifetime.</summary>
        public float time;

        /// <summary>The maximum lifetime of this particle, in seconds.</summary>
        public float lifetime;
    };

    /// <summary>
    /// The core particle emitter component.  Adding this component to an entity
    /// turns the entity into an emitter with the specified characteristics.
    /// You can add other components (for example, EmitterInitialScale, EmitterConeSource,
    /// and so on) to the same entity after the initial emission to further control
    /// how particles are emitted.
    /// </summary>
    public struct ParticleEmitter : IComponentData
    {
        /// <summary>
        ///  The "proto-particle" entity that is used as a particle template.
        //   This entity is instantiated for each emitted particle.
        /// </summary>
        public Entity particle;

        /// <summary>Maximum number of particles to emit.</summary>
        public uint maxParticles;

        /// <summary>Number of particles per second to emit.</summary>
        public float emitRate;

        /// <summary>Lifetime of each particle, in seconds.</summary>
        public Range lifetime;

        /// <summary>
        ///  Specifies whether the Transform of the emitted particles is a child
        ///  of this emitter.
        ///
        ///  If true, the emission position is set to the entity's local position,
        ///  and the particle is added as a child transform.
        ///
        ///  If false, the emitter's world position is added to the emission position,
        ///  and that result set as the local position.
        /// </summary>
        public bool attachToEmitter;
    }

    /// <summary>
    ///  The main entry system for particles. This system must be present
    ///  for any particles to be spawned.
    /// </summary>
#if false
struct ParticleSystemData : IEntityQuery {
    ParticleEmitter emitter;
    [Not] ParticleEmitterInternal emitterInternal;
}

struct ParticleEmitterData : IEntityQuery {
    [Optional] EmitterBoxSource boxSource;
    [Optional] EmitterInitialRotation initialRotation;
    [Optional] EmitterInitialScale initialScale;
    [Optional] EmitterInitialVelocity initialVelocity;
    [Optional] LifetimeAlpha lifetimeAlpha;
    [Optional] LifetimeColor lifetimeColor;
    [Optional] LifetimeRotation lifetimeRotation;
    [Optional] LifetimeScale lifetimeScale;
    [Optional] LifetimeVelocity lifetimeVelocity;
}
#endif

    /// <summary>
    ///  Spawns particles inside of a rectangular area on the X/Y plane.
    /// <summary>
    public struct EmitterBoxSource : IComponentData
    {
        /// <summary>
        ///  Particles are emitted from a random spot inside this rectangle, with
        ///  0,0 of the rect at the Emitter's position.
        /// </summary>
        public Rect rect;
    }

    /// <summary>
    ///  Spawns particles in a cone. Particles are emitted from the base of the cone,
    ///  which is a circle on the X/Z plane. The angle and speed parameters define
    ///  the initial particle velocity.
    /// </summary>
    public struct EmitterConeSource : IComponentData
    {
        /// <summary>The radius in which the particles are being spawned.</summary>
        public float radius;

        /// <summary>The angle of the cone.</summary>
        public float angle;

        /// <summary>The initial speed of the particles.</summary>
        public float speed;
    }

    /// <summary>
    ///  Spawns particles inside a circle on the X/Y plane.
    /// </summary>
    public struct EmitterCircleSource : IComponentData
    {

        /// <summary>The radius of the circle.</summary>
        public Range radius;

        /// <summary>The initial speed of the particles.</summary>
        public Range speed;

        /// <summary>
        ///  If true, particles that spawn closer to the center of the circle
        ///  have a lower speed.
        /// </summary>
        public bool speedBasedOnRadius;
    }

    /// <summary>
    ///  Multiplies the scale of the source particle by a random value in the range
    ///  specified by scale.
    /// </summary>
    public struct EmitterInitialScale : IComponentData
    {
        /// <summary>Min, max initial scale of each particle.</summary>
        public Range scale;
    }

    /// <summary>
    ///  Sets the initial velocity for particles.
    /// </summary>
    public struct EmitterInitialVelocity : IComponentData
    {
        public float3 velocity;
    }

    /// <summary>
    ///  Sets the initial rotation on the Z axis for particles to a random value
    ///  in the range specified by angle.
    /// </summary>
    public struct EmitterInitialRotation : IComponentData
    {
        public Range angle;
    }

    /// <summary>
    ///  Sets the initial angular velocity on the Z axis for particles to a random
    ///  value in the range specified by angularVelocity.
    /// </summary>
    public struct EmitterInitialAngularVelocity : IComponentData
    {
        public Range angularVelocity;
    }

    /// <summary>
    ///  Sets the initial color of the particles by multiplying the color of the
    ///  source particle by a random value obtained by sampling curve between time
    ///  0.0 and 1.0.
    /// </summary>
    public struct EmitterInitialColor : IComponentData
    {
        /// <summary>
        /// Entity with the [Bezier|Linear|Step]CurveColor component.
        /// The color is choosen randomly by sampling the curve between time 0.0 and 1.0.
        /// </summary>
        public Entity curve;
    }

    /// <summary>
    ///  Modifies the Sprite2DRenderer's color by multiplying it's initial color by
    ///  curve. The value of curve at time 0.0 defines the particle's color at the
    ///  beginning of its lifetime. The value at time 1.0 defines the particle's
    ///  color at the end of its lifetime.
    /// </summary>
    public struct LifetimeColor : IComponentData
    {
        /// <summary>Entity with the [Bezier|Linear|Step]CurveColor component.</summary>
        public Entity curve;
    }

    /// <summary>
    ///  Modifies the Transform's scale (uniform x/y/z scaling) by curve. The value
    ///  of curve at time 0.0 defines the particle's color at the beginning of its
    ///  lifetime. The value at time 1.0 defines the particle's color at the end
    ///  of its lifetime.
    /// </summary>
    public struct LifetimeScale : IComponentData
    {
        /// <summary>Entity with the [Bezier|Linear|Step]CurveFloat component.</summary>
        public Entity curve;
    }

    /// <summary>
    ///  The angular velocity over lifetime. The value of curve at time 0.0 defines
    ///  the particle's angular velocity at the beginning of its lifetime. The value
    ///  at time 1.0 defines the particle's angular velocity at the end of its lifetime.
    /// </summary>
    public struct LifetimeAngularVelocity : IComponentData
    {
        /// <summary>Entity with the [Bezier|Linear|Step]CurveFloat component.</summary>
        public Entity curve;
    }

    /// <summary>
    ///  The velocity over lifetime. The value of curve at time 0.0 defines
    ///  the particle's velocity at the beginning of its lifetime. The value
    ///  at time 1.0 defines the particle's velocity at the end of its lifetime.
    /// </summary>
    public struct LifetimeVelocity : IComponentData
    {
        /// <summary>Entity with the [Bezier|Linear|Step]CurveVector3 component.</summary>
        public Entity curve;
    }

    /// <summary>
    ///  Speed multiplier over lifetime. The value of curve at time 0.0 defines the
    ///  multiplier at the beginning of the particle's lifetime. The value at time
    ///  1.0 defines the multiplier at the end of the particle's lifetime.
    /// </summary>
    public struct LifetimeSpeedMultiplier : IComponentData
    {
        /// <summary>Entity with the [Bezier|Linear|Step]CurveFloat component.</summary>
        public Entity curve;
    }

    /// <summary>
    ///  An emitter with this component emits particles in bursts. A burst is a particle
    ///  event where a number of particles are all emitted at the same time. A cycle
    ///  is a single occurrence of a burst.
    /// </summary>
    public struct BurstEmission : IComponentData
    {
        /// <summary>
        ///  How many particles in every cycle.
        /// </summary>
        public Range count;

        /// <summary>
        ///  The interval between cycles, in seconds.
        /// </summary>
        public Range interval;

        /// <summary>
        ///  How many times to play the burst.
        /// </summary>
        public int cycles;
    }

    /// <summary>
    ///  A system that updates all particles and emitters.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class EmitterSystem : ComponentSystem
    {
        private void UpdateNewEmitters(EntityManager mgr)
        {
            var newEmitters = new List<Entity>();

            Entities
                .WithNone<ParticleEmitterInternal>()
                .ForEach((Entity e, ref ParticleEmitter particleEmitter) =>
            {
#if DEVELOPMENT
                if (!DevCheckParticleTemplate(mgr, e, particleEmitter.particle))
                    return;
#endif
                newEmitters.Add(e);
            });

            // Initialize new emitters and particle templates.
            foreach (var emitter in newEmitters)
            {
                var particleEmitter = mgr.GetComponentData<ParticleEmitter>(emitter);
                var emitterInternal = new ParticleEmitterInternal();

                if (!mgr.HasComponent<Disabled>(particleEmitter.particle))
                    mgr.AddComponentData(particleEmitter.particle, new Disabled());
                emitterInternal.particleTemplate = mgr.Instantiate(particleEmitter.particle);

                mgr.AddComponentData(emitterInternal.particleTemplate, new Particle());

                var position = new Translation() { Value = float3.zero };
                if (!mgr.HasComponent<Translation>(emitterInternal.particleTemplate))
                    mgr.AddComponentData(emitterInternal.particleTemplate, position);
                else
                    mgr.SetComponentData(emitterInternal.particleTemplate, position);

                mgr.AddComponentData(emitterInternal.particleTemplate, new ParticleEmitterReference()
                {
                    emitter = emitter
                });

                mgr.AddComponentData(emitter, emitterInternal);
            }

            CleanupBurstEmitters(mgr);
            InitBurstEmitters(mgr);
        }

        private void CleanupBurstEmitters(EntityManager mgr)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            // Remove BurstEmissionInternal component if the user removed BurstEmission.
            Entities
                .WithNone<BurstEmission>()
                .WithAll<ParticleEmitter, BurstEmissionInternal>()
                .ForEach((Entity e) =>
            {
                ecb.RemoveComponent<BurstEmissionInternal>(e);
            });

            ecb.Playback(mgr);
            ecb.Dispose();
        }

        private void InitBurstEmitters(EntityManager mgr)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            // Add BurstEmissionInternal component to newly created burst emitters.
            Entities.WithNone<BurstEmissionInternal>().WithAll<ParticleEmitter, BurstEmission>().ForEach((Entity e) =>
            {
                ecb.AddComponent(e, new BurstEmissionInternal());
            });

            ecb.Playback(mgr);
            ecb.Dispose();
        }

        private void CleanupEmitters(EntityManager mgr)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            Entities.WithNone<ParticleEmitter>().WithAll<ParticleEmitterInternal>().ForEach((Entity e) =>
            {
                DestroyParticlesForEmitter(mgr, ecb, e);
                ecb.RemoveComponent<ParticleEmitterInternal>(e);
            });

            ecb.Playback(mgr);
            ecb.Dispose();
        }

        private void DestroyParticlesForEmitter(EntityManager mgr, EntityCommandBuffer ecb, Entity emitter)
        {
            Entities.WithAll<Particle>().ForEach((Entity e, ref ParticleEmitterReference cEmitterRef) =>
            {
                if (cEmitterRef.emitter == emitter)
                    ecb.DestroyEntity(e);
            });
        }

        protected override void OnUpdate()
        {
            var env = World.TinyEnvironment();
            float deltaTime = env.frameDeltaTime;

            CleanupEmitters(EntityManager);
            UpdateNewEmitters(EntityManager);
        }
    }

    /// <summary>
    ///  A system that updates all particles and emitters.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EmitterSystem))]
    public class ParticleSystem : ComponentSystem
    {
        private void SpawnParticles(EntityManager mgr, float deltaTime)
        {
            List<Entity> emitters = new List<Entity>();

            Entities
                .WithAll<ParticleEmitter, ParticleEmitterInternal, Parent, Translation>()
                .ForEach((Entity emitter) =>
                {
                    emitters.Add(emitter);
                });

            foreach (var emitter in emitters)
                SpawnParticles(mgr, deltaTime, emitter);
        }

        private void SpawnParticles(EntityManager mgr, float deltaTime, Entity emitter)
        {
            var particleEmitter = mgr.GetComponentData<ParticleEmitter>(emitter);
            var particleEmitterInternal = mgr.GetComponentData<ParticleEmitterInternal>(emitter);

            uint particlesToSpawn = 0;

            if (mgr.HasComponent<BurstEmissionInternal>(emitter) && mgr.HasComponent<BurstEmission>(emitter))
            {
                // Burst emission mode.
                var burstEmission = mgr.GetComponentData<BurstEmission>(emitter);
                var burstEmissionInternal = mgr.GetComponentData<BurstEmissionInternal>(emitter);

                if (burstEmissionInternal.cycle < burstEmission.cycles)
                {
                    burstEmissionInternal.cooldown -= deltaTime;
                    if (burstEmissionInternal.cooldown < 0.0f)
                    {
                        particlesToSpawn = (uint)m_rand.NextInt((int)burstEmission.count.start, (int)burstEmission.count.end);
                        burstEmissionInternal.cycle++;
                        burstEmissionInternal.cooldown = m_rand.NextFloat(burstEmission.interval.start, burstEmission.interval.end);
                    }

                    mgr.SetComponentData(emitter, burstEmissionInternal);
                }
            }
            else
            {
                // Normal emission mode.
                if (particleEmitter.emitRate > 0.0f)
                {
                    particleEmitterInternal.particleSpawnCooldown += deltaTime;
                    float particleSpawnDelay = 1.0f / particleEmitter.emitRate;

                    particlesToSpawn = (uint)(particleEmitterInternal.particleSpawnCooldown / particleSpawnDelay);

                    if (particlesToSpawn > 0)
                    {
                        particleEmitterInternal.particleSpawnCooldown -= particleSpawnDelay * particlesToSpawn;
                        uint maxParticlesToSpawn = particleEmitter.maxParticles - particleEmitterInternal.numParticles;
                        if (particlesToSpawn > maxParticlesToSpawn)
                            particlesToSpawn = maxParticlesToSpawn;
                    }

                    mgr.SetComponentData(emitter, particleEmitterInternal);
                }


            }

            if (particlesToSpawn == 0)
                return;

            var newParticles = new NativeArray<Entity>((int)particlesToSpawn, Collections.Allocator.Persistent);

            // Before the new particles will spawn, Disabled component needs to be removed from the template particle.
            mgr.RemoveComponent<Disabled>(particleEmitterInternal.particleTemplate);
            mgr.Instantiate(particleEmitterInternal.particleTemplate, newParticles);
            mgr.AddComponentData(particleEmitterInternal.particleTemplate, new Disabled());

            InitTime(mgr, deltaTime, particleEmitter.lifetime, newParticles);

            if (particleEmitter.attachToEmitter)
            {
                foreach (var particle in newParticles)
                {
                    var node = new Parent() { Value = emitter };
                    if (mgr.HasComponent<Parent>(particle))
                        mgr.SetComponentData(particle, node);
                    else
                        mgr.AddComponentData(particle, node);

                    var position = mgr.GetComponentData<Translation>(particle);
                    position.Value = float3.zero;
                    mgr.SetComponentData(particle, position);
                }
            }
            else
            {
                if (mgr.HasComponent<LocalToWorld>(emitter))
                {
                    var toWorld = mgr.GetComponentData<LocalToWorld>(emitter);
                    var emitterWorldPos = new float3(toWorld.Value[3][0], toWorld.Value[3][1], toWorld.Value[3][2]);

                    foreach (var particle in newParticles)
                    {
                        var localPos = mgr.GetComponentData<Translation>(particle);
                        localPos.Value = emitterWorldPos;
                        mgr.SetComponentData(particle, localPos);
                    }
                }
            }

            if (mgr.HasComponent<EmitterInitialVelocity>(emitter))
            {
                var velocity = mgr.GetComponentData<EmitterInitialVelocity>(emitter).velocity;
                var particleVelocity = new ParticleVelocity();
                foreach (var particle in newParticles)
                {
                    particleVelocity.velocity = velocity;
                    mgr.AddComponentData(particle, particleVelocity);
                }
            }
            else if (mgr.HasComponent<LifetimeVelocity>(emitter))
            {
                foreach (var particle in newParticles)
                {
                    mgr.AddComponentData(particle, new ParticleVelocity()
                    {
                        velocity = float3.zero
                    });
                }
            }

            InitScale(mgr, emitter, newParticles);
            InitColor(mgr, emitter, newParticles);
            InitRotation(mgr, emitter, newParticles);

            // Init particle's position and the velocity based on the source.
            if (mgr.HasComponent<EmitterBoxSource>(emitter))
            {
                ParticlesSource.InitEmitterBoxSource(mgr, emitter, newParticles);
            }
            else if (mgr.HasComponent<EmitterCircleSource>(emitter))
            {
                ParticlesSource.InitEmitterCircleSource(mgr, emitter, newParticles);
            }
            else if (mgr.HasComponent<EmitterConeSource>(emitter))
            {
                ParticlesSource.InitEmitterConeSource(mgr, emitter, newParticles, particleEmitter.attachToEmitter);
            }

            newParticles.Dispose();
        }

        private void UpdateParticleLife(EntityManager mgr, float deltaTime)
        {
            // Reset particle counters in the emitters.
            Entities.ForEach((ref ParticleEmitterInternal priv) =>
            {
                priv.numParticles = 0;
            });

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            Entities.ForEach((Entity e, ref Particle particle, ref ParticleEmitterReference emitterReference) =>
            {
                var emitterInternal = mgr.GetComponentData<ParticleEmitterInternal>(emitterReference.emitter);

                particle.time += deltaTime;
                if (particle.time >= particle.lifetime)
                    ecb.DestroyEntity(e);
                else
                {
                    emitterInternal.numParticles++;
                    mgr.SetComponentData(emitterReference.emitter, emitterInternal);
                }
            });

            ecb.Playback(mgr);
            ecb.Dispose();
        }

        static void InitTime(EntityManager mgr, float deltaTime, Range lifetime, NativeArray<Entity> newParticles)
        {
            // The time is evenly distributted from 0.0 to deltaTime.

            // The time of each subsequent particle will be increased by this value.
            // particle.time[0..n] = (0 * timeStep, 1 * timeStep, 2 * timeStep, ..., n * timeStep).
            float timeStep = newParticles.Length > 1 ? (deltaTime / (float)(newParticles.Length - 1)) : 0.0f;

            // We need to subtract deltaTime from the particle's relative time, because later in
            // the same frame we are adding deltaTime to the particle's relative time when we process
            // them. This ensures that the first, newly created particle will start at a relative time 0.0.
            float time = -deltaTime;
            foreach (var particle in newParticles)
            {
                var cParticle = mgr.GetComponentData<Particle>(particle);
                cParticle.lifetime = ParticlesUtil.RandomRange(lifetime);
                cParticle.time = time;
                time += timeStep;
                mgr.SetComponentData(particle, cParticle);
            }
        }

        private void UpdateParticlePosition(EntityManager mgr, float deltaTime)
        {
            float normalizedLife = 0.0f;
            bool hasLifetimeVelocity = false;
            bool hasLifetimeSpeed = false;

            Entities.ForEach(
                (ref Particle cParticle, ref ParticleVelocity cVelocity, ref Translation cLocalPos,
                 ref ParticleEmitterReference cEmitterRef) =>
                {
                    hasLifetimeVelocity = mgr.HasComponent<LifetimeVelocity>(cEmitterRef.emitter);
                    hasLifetimeSpeed = mgr.HasComponent<LifetimeSpeedMultiplier>(cEmitterRef.emitter);
                    normalizedLife = cParticle.time / cParticle.lifetime;

                    if (hasLifetimeVelocity && hasLifetimeSpeed)
                    {
                        var lifetimeVel = mgr.GetComponentData<LifetimeVelocity>(cEmitterRef.emitter);
                        var lifetimeSpeed = mgr.GetComponentData<LifetimeSpeedMultiplier>(cEmitterRef.emitter);

                        var velocity = InterpolationService.EvaluateCurveFloat3(mgr, normalizedLife, lifetimeVel.curve);
                        var speed = InterpolationService.EvaluateCurveFloat(mgr, normalizedLife, lifetimeSpeed.curve);

                        cLocalPos.Value += (cVelocity.velocity + velocity) * speed * deltaTime;
                    }
                    else if (hasLifetimeVelocity)
                    {
                        var lifetimeVel = mgr.GetComponentData<LifetimeVelocity>(cEmitterRef.emitter);

                        var velocity = InterpolationService.EvaluateCurveFloat3(mgr, normalizedLife, lifetimeVel.curve);
                        cLocalPos.Value += (cVelocity.velocity + velocity) * deltaTime;
                    }
                    else if (hasLifetimeSpeed)
                    {
                        var lifetimeSpeed = mgr.GetComponentData<LifetimeSpeedMultiplier>(cEmitterRef.emitter);

                        var speed = InterpolationService.EvaluateCurveFloat(mgr, normalizedLife, lifetimeSpeed.curve);
                        cLocalPos.Value += cVelocity.velocity * speed * deltaTime;
                    }
                    else
                    {
                        cLocalPos.Value += cVelocity.velocity * deltaTime;
                    }
                });
        }

        private void UpdateParticleRotation(EntityManager mgr, float deltaTime)
        {
            Entities.ForEach(
                (ref Particle cParticle, ref Rotation cLocalRotation, ref ParticleAngularVelocity cAngularVelocity,
                 ref ParticleEmitterReference cEmitterRef) =>
                {
                    if (mgr.HasComponent<LifetimeAngularVelocity>(cEmitterRef.emitter))
                    {
                        var lifetime = mgr.GetComponentData<LifetimeAngularVelocity>(cEmitterRef.emitter);

                        float normalizedLife = cParticle.time / cParticle.lifetime;

                        var angularVelocity = InterpolationService.EvaluateCurveFloat(mgr, normalizedLife, lifetime.curve);
                        float angle = (cAngularVelocity.angularVelocity + angularVelocity) * deltaTime;
                        cLocalRotation.Value = math.mul(cLocalRotation.Value, quaternion.Euler(0, 0, angle));
                    }
                    else
                    {
                        float angle = cAngularVelocity.angularVelocity * deltaTime;
                        cLocalRotation.Value = math.mul(cLocalRotation.Value, quaternion.Euler(0, 0, angle));
                    }
                });
        }

        // Initialize the scale for the new particles.
        private void InitScale(EntityManager mgr, Entity emitter, NativeArray<Entity> newParticles)
        {
            bool hasInitialScale = mgr.HasComponent<EmitterInitialScale>(emitter);
            bool hasLifetimeScale = mgr.HasComponent<LifetimeScale>(emitter);

            if (!hasInitialScale && !hasLifetimeScale)
                return;

            if (hasLifetimeScale)
            {
                var lifetimeScale = mgr.GetComponentData<LifetimeScale>(emitter);

                if (hasInitialScale)
                {
                    var initialScale = mgr.GetComponentData<EmitterInitialScale>(emitter);

                    // LifetimeScale and EmitterInitialScale are present.
                    foreach (var particle in newParticles)
                    {
                        var localScale = mgr.GetComponentData<NonUniformScale>(particle).Value;
                        var scale = localScale * m_rand.NextFloat(initialScale.scale.start, initialScale.scale.end);
                        mgr.AddComponentData(particle, new ParticleLifetimeScale()
                        {
                            initialScale = localScale
                        });
                    }
                }
                else
                {
                    // Only LifetimeScale is present.
                    foreach (var particle in newParticles)
                    {
                        var localScale = mgr.GetComponentData<NonUniformScale>(particle).Value;
                        mgr.AddComponentData(particle, new ParticleLifetimeScale()
                        {
                            initialScale = localScale
                        });
                    }
                }
            }
            else if (hasInitialScale)
            {
                var initialScale = mgr.GetComponentData<EmitterInitialScale>(emitter);

                // Only EmitterInitialScale is present.
                foreach (var particle in newParticles)
                {
                    var localScale = mgr.GetComponentData<NonUniformScale>(particle);
                    localScale.Value *= m_rand.NextFloat(initialScale.scale.start, initialScale.scale.end);
                    mgr.SetComponentData(particle, localScale);
                }
            }
        }

        // Initialize the color for the new particles.
        private void InitColor(EntityManager mgr, Entity emitter, NativeArray<Entity> newParticles)
        {
            bool hasInitialColor = mgr.HasComponent<EmitterInitialColor>(emitter);
            bool hasLifetimeColor = mgr.HasComponent<LifetimeColor>(emitter);

            if (!hasInitialColor && !hasLifetimeColor)
                return;

            if (hasLifetimeColor)
            {
                var lifetimeColor = mgr.GetComponentData<LifetimeColor>(emitter);

                if (hasInitialColor)
                {
                    // LifetimeColor and EmitterInitialColor are present.

                    var initialColor = mgr.GetComponentData<EmitterInitialColor>(emitter);

                    foreach (var particle in newParticles)
                    {
                        var renderer = mgr.GetComponentData<Sprite2DRenderer>(particle);
                        var randomColor = InterpolationService.EvaluateCurveColor(
                            mgr, m_rand.NextFloat(), initialColor.curve);
                        var color = renderer.color * randomColor;
                        mgr.AddComponentData(particle, new ParticleLifetimeColor()
                        {
                            initialColor = color
                        });
                    }
                }
                else
                {
                    // Only LifetimeColor is present.
                    foreach (var particle in newParticles)
                    {
                        var rendererColor = mgr.GetComponentData<Sprite2DRenderer>(particle).color;
                        mgr.AddComponentData(particle, new ParticleLifetimeColor()
                        {
                            initialColor = rendererColor
                        });
                    }
                }
            }
            else if (hasInitialColor)
            {
                // Only EmitterInitialColor is present.

                var initialColor = mgr.GetComponentData<EmitterInitialColor>(emitter);

                foreach (var particle in newParticles)
                {
                    var renderer = mgr.GetComponentData<Sprite2DRenderer>(particle);
                    var randomColor = InterpolationService.EvaluateCurveColor(mgr, m_rand.NextFloat(), initialColor.curve);
                    renderer.color = renderer.color * randomColor;
                    mgr.SetComponentData(particle, renderer);
                }
            }
        }

        private void InitRotation(EntityManager mgr, Entity emitter, NativeArray<Entity> newParticles)
        {
            if (mgr.HasComponent<EmitterInitialRotation>(emitter))
            {
                var initialRotation = mgr.GetComponentData<EmitterInitialRotation>(emitter);
                foreach (var particle in newParticles)
                {
                    float angle = m_rand.NextFloat(initialRotation.angle.start, initialRotation.angle.end);
                    quaternion rotation = quaternion.Euler(0, 0, angle);
                    Rotation localRotation = new Rotation()
                    {
                        Value = rotation
                    };

                    if (mgr.HasComponent<Rotation>(particle))
                        mgr.SetComponentData(particle, localRotation);
                    else
                        mgr.AddComponentData(particle, localRotation);
                }
            }

            if (mgr.HasComponent<EmitterInitialAngularVelocity>(emitter))
            {
                var angularVelocity = mgr.GetComponentData<EmitterInitialAngularVelocity>(emitter).angularVelocity;

                ParticleAngularVelocity particleAngularVelocity;
                foreach (var particle in newParticles)
                {
                    particleAngularVelocity.angularVelocity = m_rand.NextFloat(angularVelocity.start, angularVelocity.end);
                    mgr.AddComponentData(particle, particleAngularVelocity);
                }
            }
            else if (mgr.HasComponent<LifetimeAngularVelocity>(emitter))
            {
                foreach (var particle in newParticles)
                {
                    mgr.AddComponentData(particle, new ParticleAngularVelocity());
                }
            }
        }

        private void UpdateParticleScale(EntityManager mgr, float deltaTime)
        {
            Entities.ForEach((ref Particle cParticle, ref ParticleLifetimeScale cLifetimeScale, ref NonUniformScale cLocalScale, ref ParticleEmitterReference cEmitterRef) =>
            {
                if (!mgr.HasComponent<LifetimeScale>(cEmitterRef.emitter))
                    return;

                var lifetimeScale = mgr.GetComponentData<LifetimeScale>(cEmitterRef.emitter);

                float normalizedLife = cParticle.time / cParticle.lifetime;

                var scale = InterpolationService.EvaluateCurveFloat(mgr, normalizedLife, lifetimeScale.curve);
                cLocalScale.Value = cLifetimeScale.initialScale * scale;
            });
        }

        private void UpdateParticleColor(EntityManager mgr, float deltaTime)
        {
            Entities.ForEach((ref Particle cParticle, ref Sprite2DRenderer cRenderer, ref ParticleLifetimeColor cLifetimeColor, ref ParticleEmitterReference cEmitterRef) =>
            {
                if (!mgr.HasComponent<LifetimeColor>(cEmitterRef.emitter))
                    return;

                var lifetimeColor = mgr.GetComponentData<LifetimeColor>(cEmitterRef.emitter);

                float normalizedLife = cParticle.time / cParticle.lifetime;

                var color = InterpolationService.EvaluateCurveColor(mgr, normalizedLife, lifetimeColor.curve);
                cRenderer.color = cLifetimeColor.initialColor * color;
            });
        }

        // TODO: Random() throws exception, so we need to use seed for now.
        private Random m_rand = new Random(1);

        protected override void OnUpdate()
        {
            var env = World.TinyEnvironment();
            float deltaTime = env.frameDeltaTime;

            SpawnParticles(EntityManager, deltaTime);
            UpdateParticleLife(EntityManager, deltaTime);
            UpdateParticlePosition(EntityManager, deltaTime);
            UpdateParticleRotation(EntityManager, deltaTime);
            UpdateParticleScale(EntityManager, deltaTime);
            UpdateParticleColor(EntityManager, deltaTime);
        }
    }
}
