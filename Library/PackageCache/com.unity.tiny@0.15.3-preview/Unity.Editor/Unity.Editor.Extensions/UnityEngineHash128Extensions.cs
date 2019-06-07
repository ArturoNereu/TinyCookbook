using System;

namespace Unity.Editor.Extensions
{
    internal static class UnityEngineHash128Extensions
    {
        public static Guid ToGuid(this UnityEngine.Hash128 hash128)
        {
            return new Guid(hash128.ToString());
        }

        public static UnityEngine.Hash128 FromGuid(this UnityEngine.Hash128 hash128, Guid guid)
        {
            return UnityEngine.Hash128.Parse(guid.ToString("N"));
        }
    }
}
