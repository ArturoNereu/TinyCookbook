using System;
using System.Globalization;

namespace Unity.Serialization
{
    public unsafe struct SerializedPrimitiveView : ISerializedView
    {
        private readonly PackedBinaryStream m_Stream;
        private readonly Handle m_Handle;

        internal SerializedPrimitiveView(PackedBinaryStream stream, Handle handle)
        {
            m_Stream = stream;
            m_Handle = handle;
        }

        public SerializedStringView AsString()
        {
            return new SerializedStringView(m_Stream, m_Handle);
        }

        public bool IsIntegral()
        {
            var ptr = m_Stream.GetBufferPtr<byte>(m_Handle);
            return Convert.IsIntegral((char*) (ptr + sizeof(int)), *(int*) ptr);
        }

        public bool IsDecimal()
        {
            var ptr = m_Stream.GetBufferPtr<byte>(m_Handle);
            return Convert.IsDecimal((char*) (ptr + sizeof(int)), *(int*) ptr);
        }

        public bool IsSigned()
        {
            var ptr = m_Stream.GetBufferPtr<byte>(m_Handle);
            return Convert.IsSigned((char*) (ptr + sizeof(int)), *(int*) ptr);
        }

        public bool IsBoolean()
        {
            var ptr = m_Stream.GetBufferPtr<byte>(m_Handle);
            var length = *(int*) ptr;
            var chars = (char*) (ptr + sizeof(int));

            if (length == 4 && chars[0] == 't'
                            && chars[1] == 'r'
                            && chars[2] == 'u'
                            && chars[3] == 'e')
            {
                return true;
            }

            if (length == 5 && chars[0] == 'f'
                            && chars[1] == 'a'
                            && chars[2] == 'l'
                            && chars[3] == 's'
                            && chars[4] == 'e')
            {
                return true;
            }

            return false;
        }

        public long AsInt64()
        {
            var ptr = m_Stream.GetBufferPtr<byte>(m_Handle);
            var result = Convert.StrToInt64((char*) (ptr + sizeof(int)), *(int*) ptr, out var value);
            if (result != Convert.ParseError.None)
            {
                throw new ParseErrorException($"Failed to parse Value=[{AsString().ToString()}] as Type=[{typeof(long)}] ParseError=[{result}]");
            }
            return value;
        }

        public ulong AsUInt64()
        {
            var ptr = m_Stream.GetBufferPtr<byte>(m_Handle);
            var result = Convert.StrToUInt64((char*) (ptr + sizeof(int)), *(int*) ptr, out var value);
            if (result != Convert.ParseError.None)
            {
                throw new ParseErrorException($"Failed to parse Value=[{AsString().ToString()}] as Type=[{typeof(ulong)}] ParseError=[{result}]");
            }
            return value;
        }

        public float AsFloat()
        {
            var ptr = m_Stream.GetBufferPtr<byte>(m_Handle);

            // @TODO we need to be able to actually parse float64 here...
            var result = Convert.StrToFloat32((char*) (ptr + sizeof(int)), *(int*) ptr, out var value);

            if (result != Convert.ParseError.None)
            {
                throw new ParseErrorException($"Failed to parse Value=[{AsString().ToString()}] as Type=[{typeof(float)}] ParseError=[{result}]");
            }

            return value;
        }

        public double AsDouble()
        {
            return double.Parse(AsString().ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        public bool AsBoolean()
        {
            var ptr = m_Stream.GetBufferPtr<byte>(m_Handle);
            var length = *(int*) ptr;
            var chars = (char*) (ptr + sizeof(int));

            if (length == 4 && chars[0] == 't'
                            && chars[1] == 'r'
                            && chars[2] == 'u'
                            && chars[3] == 'e')
            {
                return true;
            }

            if (length == 5 && chars[0] == 'f'
                            && chars[1] == 'a'
                            && chars[2] == 'l'
                            && chars[3] == 's'
                            && chars[4] == 'e')
            {
                return false;
            }

            throw new Exception("Primitive is not a boolean.");
        }
    }
}
