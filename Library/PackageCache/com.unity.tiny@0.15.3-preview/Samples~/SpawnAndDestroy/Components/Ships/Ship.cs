using Unity.Authoring.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Scenes;

namespace SpawnAndDestroy
{
    public struct Ship : IComponentData
    {
        [HideInInspector]
        public bool Initialized;
        [HideInInspector]
        public float3 DestinationPosition;
        public float MoveSpeed;
        public bool IsAlly;
        public SceneReference Bullet;
        [HideInInspector]
        public float FireTimer;
        public float MinFireCooldown;
        public float MaxFireCooldown;
        [HideInInspector]
        public bool SpawningBullet;
        public float3 BulletSpawnOffset;
    }
}
