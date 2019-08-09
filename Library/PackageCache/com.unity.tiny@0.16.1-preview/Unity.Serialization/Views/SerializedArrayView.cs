using System.Collections;
using System.Collections.Generic;

namespace Unity.Serialization
{
    public struct SerializedArrayView : ISerializedView, IEnumerable<SerializedValueView>
    {
        public struct Enumerator : IEnumerator<SerializedValueView>
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

            public SerializedValueView Current => new SerializedValueView(m_Stream, m_Current);

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
        
        private readonly PackedBinaryStream m_Stream;
        private readonly Handle m_Handle;
      
        internal SerializedArrayView(PackedBinaryStream stream, Handle handle)
        {
            m_Stream = stream;
            m_Handle = handle;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(m_Stream, m_Handle);
        }

        IEnumerator<SerializedValueView> IEnumerable<SerializedValueView>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}