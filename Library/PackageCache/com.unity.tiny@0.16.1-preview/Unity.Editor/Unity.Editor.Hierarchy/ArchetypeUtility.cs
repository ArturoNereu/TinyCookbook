using System.Runtime.CompilerServices;
using Unity.Entities;

namespace Unity.Editor
{
    internal static class ArchetypeUtility
    {
        /// <summary>
        /// Ensure <paramref name="entity"/> has all component specified by <paramref name="archetype"/>, and add them if missing.
        /// Added components will be set to component type default value.
        /// </summary>
        /// <param name="entityManager"><see cref="EntityManager"/> the entity belongs to.</param>
        /// <param name="entity"><see cref="Entity"/> to validate.</param>
        /// <param name="archetype"><see cref="EntityArchetype"/> used for validation.</param>
        public static void AddMissingComponentsFromArchetype(EntityManager entityManager, Entity entity, EntityArchetype archetype)
        {
            foreach (var componentType in archetype.GetComponentTypes())
            {
                if (!entityManager.HasComponent(entity, componentType))
                {
                    entityManager.AddComponent(entity, componentType);
                    var defaultValue = DomainCache.GetDefaultValue(componentType.GetManagedType());
                    unsafe
                    {
                        var destination = entityManager.GetComponentDataRawRW(entity, componentType.TypeIndex);
                        Unsafe.Copy(destination, ref defaultValue);
                    }
                }
            }
        }
    }
}
