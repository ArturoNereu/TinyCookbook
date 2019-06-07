using System;

namespace Unity.Serialization
{
    public struct SerializedValueView : ISerializedView
    {
        private readonly PackedBinaryStream m_Stream;
        private readonly Handle m_Handle;

        internal SerializedValueView(PackedBinaryStream stream, Handle handle)
        {
            m_Stream = stream;
            m_Handle = handle;
        }

        public TokenType Type => m_Stream.GetToken(m_Handle).Type;

        public bool IsMember()
        {
            var token = m_Stream.GetToken(m_Handle);

            if (token.Parent != -1 && m_Stream.GetToken(token.Parent).Type != TokenType.Object)
            {
                return false;
            }

            return token.Type == TokenType.String || token.Type == TokenType.Primitive;
        }

        public SerializedArrayView AsArrayView()
        {
            CheckValueType(TokenType.Array);
            return new SerializedArrayView(m_Stream, m_Handle);
        }

        public SerializedObjectView AsObjectView()
        {
            CheckValueType(TokenType.Object);
            return new SerializedObjectView(m_Stream, m_Handle);
        }

        public SerializedStringView AsStringView()
        {
            var token = m_Stream.GetToken(m_Handle);

            if (token.Type != TokenType.String && token.Type != TokenType.Primitive)
            {
                throw new Exception($"Failed to read value RequestedType=[{TokenType.String}|{TokenType.Primitive}] ActualType=[{token.Type}]");
            }

            return new SerializedStringView(m_Stream, m_Handle);
        }

        public SerializedMemberView AsMemberView()
        {
            if (!IsMember())
            {
                throw new Exception($"Failed to read value as member");
            }

            return new SerializedMemberView(m_Stream, m_Handle);
        }

        public long AsInt64()
        {
            return AsPrimitiveView().AsInt64();
        }

        public ulong AsUInt64()
        {
            return AsPrimitiveView().AsUInt64();
        }

        public float AsFloat()
        {
            return AsPrimitiveView().AsFloat();
        }

        public double AsDouble()
        {
            return AsPrimitiveView().AsDouble();
        }

        public bool AsBoolean()
        {
            return AsPrimitiveView().AsBoolean();
        }

        public SerializedPrimitiveView AsPrimitiveView()
        {
            CheckValueType(TokenType.Primitive);
            return new SerializedPrimitiveView(m_Stream, m_Handle);
        }

        private void CheckValueType(TokenType type)
        {
            var token = m_Stream.GetToken(m_Handle);

            if (token.Type != type)
            {
                throw new Exception($"Failed to read value RequestedType=[{type}] ActualType=[{token.Type}]");
            }
        }

        public override string ToString()
        {
            var token = m_Stream.GetToken(m_Handle);
            return token.ToString();
        }
    }
}
