namespace Unity.Editor.Extensions
{
    internal static class ArrayExtensions
    {
        public static T[] AsArray<T>(this T obj)
        {
            return new T[] { obj };
        }

        public static T[] Concat<T>(this T[] array, T obj)
        {
            var result = new T[array.Length + 1];
            array.CopyTo(result, 0);
            result[array.Length] = obj;
            return result;
        }

        public static T[] Concat<T>(this T[] array, T[] other)
        {
            var result = new T[array.Length + other.Length];
            array.CopyTo(result, 0);
            other.CopyTo(result, array.Length);
            return result;
        }
    }
}
