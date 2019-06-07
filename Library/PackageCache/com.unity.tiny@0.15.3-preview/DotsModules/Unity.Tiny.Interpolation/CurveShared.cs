using Unity.Entities;

namespace Unity.Tiny.Interpolation
{
    internal static class CurveShared
    {
        public static int GetKeyIndex<T>(float time, DynamicBuffer<T> keys)
            where T : struct, IKey
        {
            // TODO: binary search.
            for (int index = 0; index < keys.Length - 1; index++)
            {
                if (keys[index].GetTime() <= time && keys[index + 1].GetTime() > time)
                    return index;
            }

            return -1;
        }

        public static float GetNormalizedTime(float time, float timeBefore, float timeAfter)
        {
            return (time - timeBefore) / (timeAfter - timeBefore);
        }

        public static void GetNormalizedTimeAndKeyIndex<T>(float time, DynamicBuffer<T> keys, out float normalizeTime, out int keyIndex)
            where T : struct, IKey
        {
            keyIndex = GetKeyIndex(time, keys);
            if (keyIndex == -1)
            {
                normalizeTime = 0.0f;
                return;
            }

            normalizeTime = GetNormalizedTime(time, keys[keyIndex].GetTime(), keys[keyIndex + 1].GetTime());
        }
    }
}
