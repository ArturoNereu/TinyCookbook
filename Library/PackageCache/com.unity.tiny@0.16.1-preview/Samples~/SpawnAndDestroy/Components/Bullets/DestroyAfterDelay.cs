using Unity.Authoring.Core;
using Unity.Entities;

namespace SpawnAndDestroy
{
    public struct DestroyAfterDelay : IComponentData
    {
        public float DestroyDelay;
        [HideInInspector]
        public float Timer;
    }
}
