using System;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.Entities.Reflection
{
    /// <summary>
    /// Utility to help fetch types and methods that are tagged with an <see cref="Attribute"/>.
    /// </summary>
    /// <typeparam name="T">The root attribute type.</typeparam>
    public struct AttributeScanner<T>
        where T : Attribute
    {
        private static readonly List<TypeAttribute<T>> s_TypeAttributes = new List<TypeAttribute<T>>();
        private static readonly List<MethodAttribute<T>> s_MethodAttributes = new List<MethodAttribute<T>>();

        /// <summary>
        /// Returns a <see cref="TypeAttribute{TAttribute}"/> wrapper for each type tagged with a <see cref="T"/>.
        /// </summary>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> that the class must be tagged with.</typeparam>
        /// <returns></returns>p
        public IEnumerable<TypeAttribute<TAttribute>> GetTypeAttributes<TAttribute>()
            where TAttribute : T
            => Convert<TAttribute>(s_TypeAttributes);

        /// <summary>
        /// Returns a <see cref="MethodAttribute{TAttribute}"/> wrapper for each method tagged with a <see cref="T"/>.
        /// </summary>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> that the method must be tagged with.</typeparam>
        /// <returns></returns>
        public IEnumerable<MethodAttribute<TAttribute>> GetMethodAttributes<TAttribute>()
            where TAttribute : T
            => Convert<TAttribute>(s_MethodAttributes);

        static AttributeScanner()
        {
            foreach (var type in EditorTypes.CompiledTypesInEditor)
            {
                var tinyAttributes = type.GetCustomAttributes<T>();
                foreach (var attribute in tinyAttributes)
                {
                    s_TypeAttributes.Add(new TypeAttribute<T>(type, attribute));
                }

                foreach (var method in GetAllMethodInfo(type))
                {
                    var tinyMethodAttributes = method.GetCustomAttributes<T>();
                    foreach (var attribute in tinyMethodAttributes)
                    {
                        s_MethodAttributes.Add(new MethodAttribute<T>(type, method, attribute));
                    }
                }
            }
            s_TypeAttributes.Sort(Compare);
            s_MethodAttributes.Sort(Compare);
        }

        private static int Compare<TReflected>(TReflected lhs, TReflected rhs)
            where TReflected : IReflectedAttribute<T>
        {
            if (lhs.Attribute is IComparable<T> comparable)
            {
                return comparable.CompareTo(rhs.Attribute);
            }
            return lhs.GetHashCode().CompareTo(rhs.GetHashCode());
        }

        private static IEnumerable<TypeAttribute<TAttribute>> Convert<TAttribute>(
            IEnumerable<TypeAttribute<T>> typeAttributes)
            where TAttribute : T
        {
            foreach (var typeAttribute in typeAttributes)
            {
                if (typeAttribute.Attribute is TAttribute attribute)
                {
                    yield return new TypeAttribute<TAttribute>(typeAttribute.Type, attribute);
                }
            }
        }

        private static IEnumerable<MethodAttribute<TAttribute>> Convert<TAttribute>(
            IEnumerable<MethodAttribute<T>> methodAttributes)
            where TAttribute : T
        {
            foreach (var methodAttribute in methodAttributes)
            {
                if (methodAttribute.Attribute is TAttribute attribute)
                {
                    yield return new MethodAttribute<TAttribute>(methodAttribute.Type, methodAttribute.Method, attribute);
                }
            }
        }

        private static IEnumerable<MethodInfo> GetAllMethodInfo(IReflect type)
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                   BindingFlags.NonPublic);
        }
    }
}
