using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.Particles
{
    static class ParticlesSource
    {
        private static Random m_rand = new Random(1);

        public static void InitEmitterCircleSource(EntityManager mgr, Entity emitter, NativeArray<Entity> particles)
        {
            var source = mgr.GetComponentData<EmitterCircleSource>(emitter);
            foreach (var particle in particles)
            {
                float randomAngle = m_rand.NextFloat((float)-math.PI, (float)math.PI);
                float radiusNormalized =
                    source.radius.end != 0.0f
                        ? math.sqrt(m_rand.NextFloat(source.radius.start / source.radius.end, 1.0f))
                        : 0.0f;
                float randomRadius = radiusNormalized * source.radius.end;

                var positionNormalized = new float2(math.sin(randomAngle), math.cos(randomAngle));
                var position = new float3(positionNormalized.x * randomRadius, positionNormalized.y * randomRadius, 0.0f);
                var Translation = mgr.GetComponentData<Translation>(particle);
                Translation.Value += position;
                mgr.SetComponentData(particle, Translation);

                if (source.speed.start != 0.0f && source.speed.end != 0.0f)
                {
                    float randomSpeed = m_rand.NextFloat(source.speed.start, source.speed.end);
                    if (source.speedBasedOnRadius)
                        randomSpeed *= radiusNormalized;

                    var particleVelocity = new ParticleVelocity()
                    {
                        velocity = new float3(positionNormalized.x * randomSpeed, positionNormalized.y * randomSpeed, 0.0f)
                    };

                    if (mgr.HasComponent<ParticleVelocity>(particle))
                        mgr.SetComponentData(particle, particleVelocity);
                    else
                        mgr.AddComponentData(particle, particleVelocity);
                }
            }
        }

        public static void InitEmitterConeSource(EntityManager mgr, Entity emitter, NativeArray<Entity> particles, bool attachToEmitter)
        {
            var source = mgr.GetComponentData<EmitterConeSource>(emitter);
            float3 localPositionOnConeBase;

            quaternion rotation = quaternion.identity;
            if (!attachToEmitter)
            {
                if (mgr.HasComponent<LocalToWorld>(emitter))
                {
                    var toWorld = mgr.GetComponentData<LocalToWorld>(emitter);
                    rotation = new quaternion(toWorld.Value);
                }
            }

            foreach (var particle in particles)
            {
                float angle = m_rand.NextFloat((float)-math.PI, (float)math.PI);
                float radiusNormalized = math.sqrt(m_rand.NextFloat(0.0f, 1.0f));
                float radius = source.radius * radiusNormalized;

                localPositionOnConeBase.x = math.sin(angle);
                localPositionOnConeBase.z = math.cos(angle);
                localPositionOnConeBase.y = 0.0f;

                var worldPositionOnConeBase = math.rotate(rotation, localPositionOnConeBase);
                var Translation = mgr.GetComponentData<Translation>(particle);
                Translation.Value += worldPositionOnConeBase * radius;
                mgr.SetComponentData(particle, Translation);

                ParticleVelocity particleVelocity = new ParticleVelocity();
                float spreadAngle = source.angle * radiusNormalized;

                float directionRadius = math.sin(spreadAngle);
                float directionHeight = math.cos(spreadAngle);
                particleVelocity.velocity.x = localPositionOnConeBase.x * directionRadius;
                particleVelocity.velocity.z = localPositionOnConeBase.z * directionRadius;
                particleVelocity.velocity.y = directionHeight;
                particleVelocity.velocity *= source.speed;

                particleVelocity.velocity = math.rotate(rotation, particleVelocity.velocity);

                if (mgr.HasComponent<ParticleVelocity>(particle))
                    mgr.SetComponentData(particle, particleVelocity);
                else
                    mgr.AddComponentData(particle, particleVelocity);
            }
        }

        public static void InitEmitterBoxSource(EntityManager mgr, Entity emitter, NativeArray<Entity> particles)
        {
            var source = mgr.GetComponentData<EmitterBoxSource>(emitter);

            foreach (var particle in particles)
            {
                var pos = ParticlesUtil.RandomPointInRect(source.rect);

                // center the box at the origin.
                // TODO: we could precompute the proper source rect (basically move the origin x/y by half) and
                // stash it somewhere to avoid division here
                pos.x -= source.rect.width / 2.0f;
                pos.y -= source.rect.height / 2.0f;

                var Translation = mgr.GetComponentData<Translation>(particle);
                Translation.Value += new float3(pos.x, pos.y, 0.0f);
                mgr.SetComponentData(particle, Translation);
            }
        }
    }
} // namespace Particles
