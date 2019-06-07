using System;
using System.Reflection;

namespace Unity.Entities.Reflection
{
    /// <summary>
    /// Holds an <see cref="Attribute"/>, as well as a tagged method and the type that contains it.
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    public struct MethodAttribute<TAttribute> : IReflectedAttribute<TAttribute>
        where TAttribute : Attribute
    {
        /// <summary>
        /// The type that contains the tagged method.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The tagged method.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// The attribute on the method.
        /// </summary>
        public TAttribute Attribute { get; }

        internal MethodAttribute(Type type, MethodInfo method, TAttribute attribute)
        {
            Type = type;
            Method = method;
            Attribute = attribute;
        }
    }
}
