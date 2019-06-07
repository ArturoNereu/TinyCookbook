using System;
using System.Collections.Generic;

namespace Unity.Entities.Reflection
{
    public static class TypeAttributeExtensions
    {
        /// <summary>
        /// Filters a <see cref="TypeAttribute{TAttribute}"/> to find instances on static types.
        /// </summary>
        /// <param name="source">The <see cref="TypeAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="TypeAttribute{TAttribute}"/> instances on static types.</returns>
        public static IEnumerable<TypeAttribute<TAttribute>> Static<TAttribute>(
            this IEnumerable<TypeAttribute<TAttribute>> source,
            Action<TypeAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsStaticType<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="TypeAttribute{TAttribute}"/> to find instances on non-static types.
        /// </summary>
        /// <param name="source">The <see cref="TypeAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="TypeAttribute{TAttribute}"/> instances on non-static types.</returns>
        public static IEnumerable<TypeAttribute<TAttribute>> NonStatic<TAttribute>(
            this IEnumerable<TypeAttribute<TAttribute>> source,
            Action<TypeAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsNonStaticType<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="TypeAttribute{TAttribute}"/> to find instances on abstract types.
        /// </summary>
        /// <param name="source">The <see cref="TypeAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="TypeAttribute{TAttribute}"/> instances on abstract types.</returns>
        public static IEnumerable<TypeAttribute<TAttribute>> Abstract<TAttribute>(
            this IEnumerable<TypeAttribute<TAttribute>> source,
            Action<TypeAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsAbstractType<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="TypeAttribute{TAttribute}"/> to find instances on concrete types.
        /// </summary>
        /// <param name="source">The <see cref="TypeAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="TypeAttribute{TAttribute}"/> instances on concrete types.</returns>
        public static IEnumerable<TypeAttribute<TAttribute>> Concrete<TAttribute>(
            this IEnumerable<TypeAttribute<TAttribute>> source,
            Action<TypeAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsConcreteType<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="TypeAttribute{TAttribute}"/> to find instances on types that are subclasses of the provided type.
        /// </summary>
        /// <param name="source">The <see cref="TypeAttribute{TAttribute}"/> to filter.</param>
        /// <param name="parentType">The parent type.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="TypeAttribute{TAttribute}"/> instances on types that are subclasses of the provided type.</returns>
        public static IEnumerable<TypeAttribute<TAttribute>> SubClassOf<TAttribute>(
            this IEnumerable<TypeAttribute<TAttribute>> source,
            Type parentType,
            Action<TypeAttribute<TAttribute>, Type> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsSubClassOfType<TAttribute>(), parentType, onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="TypeAttribute{TAttribute}"/> to find instances on non-generic types.
        /// </summary>
        /// <param name="source">The <see cref="TypeAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="TypeAttribute{TAttribute}"/> instances on non-generic types.</returns>
        public static IEnumerable<TypeAttribute<TAttribute>> NonGeneric<TAttribute>(
            this IEnumerable<TypeAttribute<TAttribute>> source,
            Action<TypeAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsNonGenericType<TAttribute>(), onMismatchCallback);
        }
    }
}
