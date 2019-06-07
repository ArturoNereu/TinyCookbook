namespace Unity.Editor
{
    internal class Wrapper<T>
    {
        public T Value;

        public Wrapper(T value)
        {
            Value = value;
        }
    }
}
