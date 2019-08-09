using Unity.Entities;

namespace SpawnAndDestroy
{
    public struct AttackConfiguration : IComponentData
    {
        public bool IsAttacking;
    }
}
