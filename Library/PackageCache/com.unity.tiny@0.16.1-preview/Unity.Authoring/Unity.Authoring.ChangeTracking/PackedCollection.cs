#if !NET_DOTS
using System;
using Unity.Collections;

namespace Unity.Authoring.ChangeTracking
{
    internal struct PackedCollection<T> : IDisposable
        where T : unmanaged, IEquatable<T>
    {
        private NativeList<T> m_List;
        private NativeHashMap<T, int> m_Lookup;

        public NativeList<T> List => m_List;

        public int Length => m_List.Length;

        public PackedCollection(int capacity, Allocator label)
        {
            m_List = new NativeList<T>(capacity, label);
            m_Lookup = new NativeHashMap<T, int>(capacity, label);
        }

        public void Dispose()
        {
            m_List.Dispose();
            m_Lookup.Dispose();
        }

        /// <summary>
        /// Adds a new element to the collection.
        /// </summary>
        /// <remarks>
        /// This will NOT throw if the item already exists and will allow for duplicates.
        /// </remarks>
        public int Add(T value)
        {
            var index = m_List.Length;
            m_List.Add(value);
            m_Lookup.TryAdd(value, index);
            return index;
        }

        /// <summary>
        /// Get or add the value to the packed collection.
        /// </summary>
        /// <remarks>
        /// If the item already exists in the collection it will return the LAST added index.
        /// </remarks>
        public int GetOrAdd(T value)
        {
            if (!m_Lookup.TryGetValue(value, out var index))
            {
                index = m_List.Length;
                m_List.Add(value);
                m_Lookup.TryAdd(value, index);
            }
            return index;
        }
    }
}
#endif
