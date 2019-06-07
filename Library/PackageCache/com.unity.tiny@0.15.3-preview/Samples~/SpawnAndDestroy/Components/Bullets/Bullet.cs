using Unity.Authoring.Core;
using Unity.Entities;
using Unity.Mathematics;

namespace SpawnAndDestroy
{
    public struct Bullet : IComponentData
    {
        [HideInInspector]
        public bool Initialized;
        public float3 Velocity;
    }
}
