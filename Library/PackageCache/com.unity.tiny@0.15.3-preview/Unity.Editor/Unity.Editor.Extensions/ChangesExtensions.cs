using System;
using System.Collections.Generic;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Authoring.Hashing;
using Unity.Entities;
using Unity.Tiny.Core2D;

namespace Unity.Editor.Extensions
{
    internal static class ChangesExtensions
    {
        public static IEnumerable<Guid> ReparentedEntities(this Changes change)
        {
            var typeHash = TypeManager.GetTypeInfo<Parent>().StableTypeHash;
            var hashes = change.WorldDiff.TypeHashes;
            for (var index = 0; index < change.WorldDiff.SetCommands.Length; ++index)
            {
                var diff = change.WorldDiff.SetCommands[index];
                if (hashes[diff.TypeHashIndex] == typeHash)
                {
                    yield return change.WorldDiff.Entities[diff.EntityIndex].ToGuid();
                }
            }
        }
    }
}
