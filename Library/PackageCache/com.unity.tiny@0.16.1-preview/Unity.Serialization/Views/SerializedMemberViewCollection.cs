using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace Unity.Serialization
{
    public struct SerializedMemberViewCollection : IDisposable, IEnumerable<SerializedMemberView>
    {
        public struct Enumerator : IEnumerator<SerializedMemberView>
        {
            private readonly SerializedMemberViewCollection m_Collection;
            private int m_Index;

            internal Enumerator(SerializedMemberViewCollection collection)
            {
                m_Collection = collection;
                m_Index = -1;
            }

            public bool MoveNext()
            {
                m_Index++;
                return m_Index < m_Collection.m_Members.Length;
            }

            public void Reset()
            {
                m_Index = -1;
            }

            public SerializedMemberView Current => m_Collection.m_Members[m_Index];

            object IEnumerator.Current => Current;

            public void Dispose()
            {

            }
        }

        private NativeList<SerializedMemberView> m_Members;

        public SerializedMemberViewCollection(Allocator label)
        {
            m_Members = new NativeList<SerializedMemberView>(label);
        }

        public SerializedValueView this[string name]
        {
            get
            {
                if (TryGetValue(name, out var value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
        }

        public bool TryGetValue(string name, out SerializedValueView value)
        {
            foreach (var m in this)
            {
                if (!m.Name().Equals(name))
                {
                    continue;
                }

                value = m.Value();
                return true;
            }

            value = default;
            return false;
        }

        public void Add(SerializedMemberView view)
        {
            m_Members.Add(view);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<SerializedMemberView> IEnumerable<SerializedMemberView>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            m_Members.Dispose();
        }
    }
}
