using System.Linq;

namespace Unity.Entities.Reflection
{
    /// <summary>
    /// Cache around attributes.
    /// </summary>
    /// <typeparam name="T">The type to query.</typeparam>
    public static class AttributeCache<T>
    {
        private struct Lookup<TAttribute>
        {
            public static readonly bool Any;

            static Lookup()
            {
                var type = typeof(T);
                Any = type.GetCustomAttributes(typeof(TAttribute), true).Any();

                if (!Any && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DynamicBufferContainer<>))
                {
                    Any = type.GetGenericArguments()[0].GetCustomAttributes(typeof(TAttribute), true).Any();
                }
            }
        }

        /// <summary>
        /// Checks whether the <see cref="T"/> has the attribute <see cref="TAttribute"/>.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type.</typeparam>
        /// <returns>True if the <see cref="T"/> has the attribute <see cref="TAttribute"/>, false otherwise.</returns>
        public static bool HasAttribute<TAttribute>()
        {
            return Lookup<TAttribute>.Any;
        }
    }
}
