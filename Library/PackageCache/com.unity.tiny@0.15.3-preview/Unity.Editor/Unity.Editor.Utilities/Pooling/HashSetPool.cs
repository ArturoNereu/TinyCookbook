using System;
using System.Collections.Generic;

namespace Unity.Editor
{
    internal static class HashSetPool<T>
    {
        public struct DisposableHashSet : IDisposable
        {
            public readonly HashSet<T> Set;

            internal static DisposableHashSet Make()
            {
                return new DisposableHashSet(Get());
            }

            public static implicit operator HashSet<T>(DisposableHashSet d)
            {
                return d.Set;
            }

            private DisposableHashSet(HashSet<T> set)
            {
                Set = set;
            }

            public void Dispose()
            {
                Release(Set);
            }
        }

        private static readonly ObjectPool<HashSet<T>> s_Pool = new ObjectPool<HashSet<T>>(null, l => l.Clear());

        public static DisposableHashSet GetDisposable()
        {
            return DisposableHashSet.Make();
        }

        public static HashSet<T> Get(LifetimePolicy lifetime = LifetimePolicy.Frame)
        {
            return s_Pool.Get(lifetime);
        }

        public static void Release(HashSet<T> toRelease)
        {
            s_Pool.Release(toRelease);
        }
    }
}
