using Unity.Entities;

namespace Unity.Editor.Extensions
{
    internal static class DynamicBufferExtensions
    {
        public static bool Contains<T>(this DynamicBuffer<T> buffer, T item) where T : struct
        {
            for (var i = 0; i < buffer.Length; ++i)
            {
                if (buffer[i].Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        public static void Remove<T>(this DynamicBuffer<T> buffer, T item) where T : struct
        {
            for (var i = 0; i < buffer.Length; ++i)
            {
                if (buffer[i].Equals(item))
                {
                    buffer.RemoveAt(i);
                    return;
                }
            }
        }

        public static int IndexOf<T>(this DynamicBuffer<T> buffer, T item) where T : struct
        {
            for (var i = 0; i < buffer.Length; ++i)
            {
                if (buffer[i].Equals(item))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
