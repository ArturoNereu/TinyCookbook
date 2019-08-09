namespace Unity.Serialization
{
    public struct SerializedMemberView
    {
        private readonly PackedBinaryStream m_Stream;
        private readonly Handle m_Handle;

        internal SerializedMemberView(PackedBinaryStream stream, Handle handle)
        {
            m_Stream = stream;
            m_Handle = handle;
        }

        public SerializedStringView Name()
        {
            return new SerializedStringView(m_Stream, m_Handle);
        }

        public SerializedValueView Value()
        {
            return new SerializedValueView(m_Stream, m_Stream.GetChild(m_Handle));
        } 
    }
}