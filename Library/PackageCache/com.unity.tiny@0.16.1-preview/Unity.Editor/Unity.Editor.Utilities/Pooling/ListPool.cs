using System;
using System.Collections.Generic;

namespace Unity.Editor
{
    internal static class ListPool<T>
    {
        public struct DisposableList : IDisposable
        {
            public readonly List<T> List;

            internal static DisposableList Make()
            {
                return new DisposableList(Get());
            }

            public static implicit operator List<T>(DisposableList d)
            {
                return d.List;
            }

            private DisposableList(List<T> list)
            {
                List = list;
            }

            public void Dispose()
            {
                Release(List);
            }
        }

        private static readonly ObjectPool<List<T>> s_Pool = new ObjectPool<List<T>>(null, l => l.Clear());

        public static DisposableList GetDisposable()
        {
            return DisposableList.Make();
        }

        public static List<T> Get(LifetimePolicy lifetime = LifetimePolicy.Frame)
        {
            return s_Pool.Get(lifetime);
        }

        public static void Release(List<T> toRelease)
        {
            s_Pool.Release(toRelease);
        }
    }
}
