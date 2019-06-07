using System;

namespace Unity.Entities.Reflection
{
    /// <summary>
    /// Holds an <see cref="Attribute"/> and the tagged type that contains it.
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    public struct TypeAttribute<TAttribute> : IReflectedAttribute<TAttribute>
        where TAttribute : Attribute
    {
        /// <summary>
        /// The tagged type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The attribute on the class.
        /// </summary>
        public TAttribute Attribute { get; }

        internal TypeAttribute(Type type, TAttribute attribute)
        {
            Type = type;
            Attribute = attribute;
        }
    }
}
