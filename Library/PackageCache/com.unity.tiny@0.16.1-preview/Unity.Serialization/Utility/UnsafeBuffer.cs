namespace Unity.Serialization
{
    internal unsafe struct UnsafeBuffer<T> where T : unmanaged
    {
        public T* Buffer;
        public int Length;
    }
}