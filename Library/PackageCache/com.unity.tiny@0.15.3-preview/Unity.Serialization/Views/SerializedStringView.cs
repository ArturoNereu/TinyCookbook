using System;

namespace Unity.Serialization
{
    public unsafe struct SerializedStringView : ISerializedView, IEquatable<string>
    {
        private readonly PackedBinaryStream m_Stream;
        private readonly Handle m_Handle;

        internal SerializedStringView(PackedBinaryStream stream, Handle handle)
        {
            m_Stream = stream;
            m_Handle = handle;
        }

        public int Length()
        {
            return *m_Stream.GetBufferPtr<int>(m_Handle);
        }

        public char this[int index]
        {
            get
            {
                var ptr = m_Stream.GetBufferPtr<byte>(m_Handle);

                if ((uint) index > *(int*) ptr)
                {
                    throw new IndexOutOfRangeException();
                }

                var chars = (char*) (ptr + sizeof(int));
                return chars[index];
            }
        }

        public bool Equals(string other)
        {
            var ptr = m_Stream.GetBufferPtr<byte>(m_Handle);

            if (null == other)
            {
                return *(int*) ptr == 0;
            }

            if (other.Length != *(int*) ptr)
            {
                return false;
            }

            var chars = (char*) (ptr + sizeof(int));

            for (var i = 0; i < other.Length; i++)
            {
                if (chars[i] != other[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            var buffer = m_Stream.GetBufferPtr<byte>(m_Handle);
            var ptr = (char*) (buffer + sizeof(int));
            var len = *(int*) buffer;

            var chars = stackalloc char[len];
            var charIndex = 0;

            for (var i = 0; i < len; i++)
            {
                if (ptr[i] == '\\')
                {
                    i++;

                    switch (ptr[i])
                    {
                        case '\\':
                            chars[charIndex] = '\\';
                            break;
                        case '\"':
                            chars[charIndex] = '\"';
                            break;
                        case '\t':
                            chars[charIndex] = '\t';
                            break;
                        case '\r':
                            chars[charIndex] = '\r';
                            break;
                        case '\n':
                            chars[charIndex] = '\n';
                            break;
                        case '\b':
                            chars[charIndex] = '\b';
                            break;
                    }

                    charIndex++;
                    continue;
                }

                chars[charIndex++] = ptr[i];
            }

            return new string(chars, 0, charIndex);
        }
    }
}
