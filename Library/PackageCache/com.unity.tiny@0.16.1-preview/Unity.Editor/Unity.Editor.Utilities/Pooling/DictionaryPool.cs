using System;
using System.Collections.Generic;

namespace Unity.Editor
{
    internal static class DictionaryPool<TKey, TValue>
    {
        public struct DisposableDictionary : IDisposable
        {
            public readonly Dictionary<TKey, TValue> Dictionary;

            internal static DisposableDictionary Make()
            {
                return new DisposableDictionary(Get());
            }

            public static implicit operator Dictionary<TKey, TValue>(DisposableDictionary d)
            {
                return d.Dictionary;
            }

            private DisposableDictionary(Dictionary<TKey, TValue> dictionary)
            {
                Dictionary = dictionary;
            }

            public void Dispose()
            {
                Release(Dictionary);
            }
        }

        private static readonly ObjectPool<Dictionary<TKey, TValue>> s_Pool = new ObjectPool<Dictionary<TKey, TValue>>(null, l => l.Clear());

        public static DisposableDictionary GetDisposable()
        {
            return DisposableDictionary.Make();
        }

        public static Dictionary<TKey, TValue> Get(LifetimePolicy lifetime = LifetimePolicy.Frame)
        {
            return s_Pool.Get(lifetime);
        }

        public static void Release(Dictionary<TKey, TValue> toRelease)
        {
            s_Pool.Release(toRelease);
        }
    }
}
