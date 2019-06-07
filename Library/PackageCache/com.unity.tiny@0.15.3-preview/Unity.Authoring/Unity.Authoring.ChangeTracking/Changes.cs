#if !NET_DOTS
using System;
using System.Collections.Generic;
using Unity.Authoring.Hashing;
using Unity.Entities;

namespace Unity.Authoring.ChangeTracking
{
    internal delegate void ChangeEventHandler(Changes changes);

    /// <summary>
    /// A structure containing all information about a change to the world.
    /// </summary>
    internal struct Changes : IDisposable
    {
        /// <summary>
        /// Changes that were made to the world.
        /// </summary>
        private readonly WorldDiff m_WorldDiff;

        /// <summary>
        /// The inverse set of changes that were made to the world.
        /// </summary>
        private readonly WorldDiff m_InverseWorldDiff;

        internal WorldDiff WorldDiff => m_WorldDiff;
        internal WorldDiff InverseDiff => m_InverseWorldDiff;

        public bool ComponentsWereAdded => WorldDiff.AddComponents.Length > 0;
        public bool ComponentsWereRemoved => WorldDiff.RemoveComponents.Length > 0;
        public bool ComponentsWereModified => WorldDiff.SetCommands.Length > 0 || WorldDiff.SharedSetCommands.Length > 0;
        public bool EntitiesWereCreated => WorldDiff.NewEntityCount > 0;
        public bool EntitiesWereDeleted => WorldDiff.DeletedEntityCount > 0;

        internal Changes(WorldDiff worldDiff, WorldDiff inverseWorldDiff)
        {
            m_WorldDiff = worldDiff;
            m_InverseWorldDiff = inverseWorldDiff;
        }

        public void Dispose()
        {
            m_WorldDiff.Dispose();
            m_InverseWorldDiff.Dispose();
        }

        public IEnumerable<Guid> AllChangedEntities()
        {
            for (var index = 0; index < WorldDiff.Entities.Length; ++index)
            {
                yield return WorldDiff.Entities[index].ToGuid();
            }
        }

        public IEnumerable<Guid> CreatedEntities()
        {
            for (var index = 0; index < WorldDiff.NewEntityCount; ++index)
            {
                yield return WorldDiff.Entities[index].ToGuid();
            }
        }

        public IEnumerable<Guid> DeletedEntities()
        {
            var startIndex = WorldDiff.Entities.Length - WorldDiff.DeletedEntityCount;
            var count = WorldDiff.DeletedEntityCount;
            for (var index = startIndex; index < startIndex + count; ++index)
            {
                yield return WorldDiff.Entities[index].ToGuid();
            }
        }

        public IEnumerable<Guid> EnabledStateChanged()
        {
            var typeHash = TypeManager.GetTypeInfo<Disabled>().StableTypeHash;
            var hashes = WorldDiff.TypeHashes;
            for (var index = 0; index < WorldDiff.AddComponents.Length; ++index)
            {
                var diff = WorldDiff.AddComponents[index];
                if (hashes[diff.TypeHashIndex] == typeHash)
                {
                    yield return WorldDiff.Entities[diff.EntityIndex].ToGuid();
                }
            }
            for (var index = 0; index < WorldDiff.RemoveComponents.Length; ++index)
            {
                var diff = WorldDiff.RemoveComponents[index];
                if (hashes[diff.TypeHashIndex] == typeHash)
                {
                    yield return WorldDiff.Entities[diff.EntityIndex].ToGuid();
                }
            }
        }

        public IEnumerable<Guid> ChangedEntitiesWithSetComponent<T>()
            where T : struct
        {
            var typeHash = TypeManager.GetTypeInfo<T>().StableTypeHash;
            var hashes = m_WorldDiff.TypeHashes;

            for (var index = 0; index < m_WorldDiff.SetCommands.Length; ++index)
            {
                var diff = m_WorldDiff.SetCommands[index];
                if (hashes[diff.TypeHashIndex] == typeHash)
                {
                    yield return m_WorldDiff.Entities[diff.EntityIndex].ToGuid();
                }
            }
        }

        public IEnumerable<Guid> ChangedEntitiesWithSetSharedComponent<T>()
            where T : struct, ISharedComponentData
        {
            var typeHash = TypeManager.GetTypeInfo<T>().StableTypeHash;
            var hashes = m_WorldDiff.TypeHashes;

            for (var index = 0; index < m_WorldDiff.SharedSetCommands.Length; ++index)
            {
                var diff = m_WorldDiff.SharedSetCommands[index];
                if (hashes[diff.TypeHashIndex] == typeHash)
                {
                    yield return m_WorldDiff.Entities[diff.EntityIndex].ToGuid();
                }
            }
        }

        public IEnumerable<Guid> ChangedEntitiesWithAddComponent<T>()
            where T : struct
        {
            var typeHash = TypeManager.GetTypeInfo<T>().StableTypeHash;
            var hashes = m_WorldDiff.TypeHashes;

            for (var index = 0; index < m_WorldDiff.AddComponents.Length; ++index)
            {
                var diff = m_WorldDiff.AddComponents[index];

                if (hashes[diff.TypeHashIndex] == typeHash)
                {
                    yield return m_WorldDiff.Entities[diff.EntityIndex].ToGuid();
                }
            }
        }

        public IEnumerable<Guid> ChangedEntitiesWithRemoveComponent<T>()
            where T : struct
        {
            var typeHash = TypeManager.GetTypeInfo<T>().StableTypeHash;
            var hashes = m_WorldDiff.TypeHashes;

            for (var index = 0; index < m_WorldDiff.RemoveComponents.Length; ++index)
            {
                var diff = m_WorldDiff.RemoveComponents[index];

                if (hashes[diff.TypeHashIndex] == typeHash)
                {
                    yield return m_WorldDiff.Entities[diff.EntityIndex].ToGuid();
                }
            }
        }
    }
}
#endif
