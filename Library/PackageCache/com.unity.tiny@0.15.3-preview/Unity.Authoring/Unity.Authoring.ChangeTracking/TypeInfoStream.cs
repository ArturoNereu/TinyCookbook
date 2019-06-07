#if !NET_DOTS
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Unity.Authoring.ChangeTracking
{
    internal unsafe struct TypeInfo
    {
        public int ElementSize;
        public int EntityOffsetCount;
        public ulong StableTypeHash;
        public int* EntityOffsets;
    }

    /// <summary>
    /// Custom blittable type info stream specialized for ChangeTracking use case.
    ///
    /// @TODO Remove this when real type information is available in bursted jobs
    /// </summary>
    internal struct TypeInfoStream : IDisposable
    {
        private struct Layout
        {
            public int ElementSize;
            public int EntityOffsetCount;
            public ulong StableTypeHash;
            public int EntityOffsetPosition;
        }

        [NativeDisableContainerSafetyRestriction] private NativeArray<Layout> m_TypeInfo;
        [NativeDisableContainerSafetyRestriction] private NativeList<byte> m_EntityOffsets;

        public readonly int LinkedEntityGroupTypeIndex;

        public TypeInfoStream(Allocator label)
        {
            m_TypeInfo = new NativeArray<Layout>(TypeManager.MaximumTypesCount, label);
            m_EntityOffsets = new NativeList<byte>(1, label);
            LinkedEntityGroupTypeIndex = TypeManager.GetTypeIndex<LinkedEntityGroup>();
        }

        public void Dispose()
        {
            m_TypeInfo.Dispose();
            m_EntityOffsets.Dispose();
        }

        public unsafe TypeInfo GetTypeInfo(int typeIndex)
        {
            var layout = m_TypeInfo[typeIndex & TypeManager.ClearFlagsMask];

            return new TypeInfo
            {
                ElementSize = layout.ElementSize,
                EntityOffsetCount = layout.EntityOffsetCount,
                StableTypeHash = layout.StableTypeHash,
                EntityOffsets = (int*) ((byte*) m_EntityOffsets.GetUnsafePtr() + layout.EntityOffsetPosition)
            };
        }

        public unsafe void Add(TypeManager.TypeInfo typeInfo)
        {
            if (m_TypeInfo[typeInfo.TypeIndex & TypeManager.ClearFlagsMask].StableTypeHash != 0)
            {
                return;
            }

            m_TypeInfo[typeInfo.TypeIndex & TypeManager.ClearFlagsMask] = new Layout
            {
                ElementSize = typeInfo.ElementSize,
                EntityOffsetCount = typeInfo.EntityOffsetCount,
                StableTypeHash = typeInfo.StableTypeHash,
                EntityOffsetPosition = m_EntityOffsets.Length
            };

            fixed (TypeManager.EntityOffsetInfo* offsetInfo = typeInfo.EntityOffsets)
            {
                m_EntityOffsets.AddRange(offsetInfo, typeInfo.EntityOffsetCount * sizeof(TypeManager.EntityOffsetInfo));
            }
        }
    }
}
#endif
