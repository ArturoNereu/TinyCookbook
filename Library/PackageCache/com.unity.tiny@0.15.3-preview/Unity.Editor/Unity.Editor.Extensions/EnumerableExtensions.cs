using System.Collections.Generic;

namespace Unity.Editor.Extensions
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this T obj)
        {
            yield return obj;
        }
    }
}
