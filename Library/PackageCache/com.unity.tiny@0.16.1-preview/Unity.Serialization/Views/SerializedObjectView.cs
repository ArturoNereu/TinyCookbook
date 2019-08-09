using System.Collections;
using System.Collections.Generic;

namespace Unity.Serialization
{
    public struct SerializedObjectView : ISerializedView, IEnumerable<SerializedMemberView>
    {
        public struct Enumerator : IEnumerator<SerializedMemberView>
        {
            private readonly PackedBinaryStream m_Stream;
            private readonly Handle m_Start;
            private Handle m_Current;

            internal Enumerator(PackedBinaryStream stream, Handle start)
            {
                m_Stream = stream;
                m_Start = start;
                m_Current = new Handle {Index = -1, Version = -1};
            }

            public bool MoveNext()
            {
                var startIndex = m_Stream.GetTokenIndex(m_Start);
                var startToken = m_Stream.GetToken(startIndex);

                if (startToken.Length == 1)
                {
                    return false;
                }

                if (m_Current.Index == -1)
                {
                    m_Current = m_Stream.GetChild(m_Start);
                    return true;
                }

                if (!m_Stream.IsValid(m_Current))
                {
                    return false;
                }

                var currentIndex = m_Stream.GetTokenIndex(m_Current);
                var currentToken = m_Stream.GetToken(currentIndex);

                if (currentIndex + currentToken.Length >= startIndex + startToken.Length)
                {
                    return false;
                }

                m_Current = m_Stream.GetHandle(currentIndex + currentToken.Length);
                return true;
            }

            public void Reset()
            {
                m_Current = new Handle {Index = -1, Version = -1};
            }

            public SerializedMemberView Current => new SerializedMemberView(m_Stream, m_Current);

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }

        private readonly PackedBinaryStream m_Stream;
        private readonly Handle m_Handle;

        internal SerializedObjectView(PackedBinaryStream stream, Handle handle)
        {
            m_Stream = stream;
            m_Handle = handle;
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

        public bool TryGetMember(string name, out SerializedMemberView member)
        {
            foreach (var m in this)
            {
                if (!m.Name().Equals(name))
                {
                    continue;
                }

                member = m;
                return true;
            }

            member = default;
            return false;
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

        public Enumerator GetEnumerator()
        {
            return new Enumerator(m_Stream, m_Handle);
        }

        IEnumerator<SerializedMemberView> IEnumerable<SerializedMemberView>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
