using System;
using System.Collections.Generic;

namespace Unity.Entities.Reflection
{
    public static class MethodAttributeExtensions
    {
        /// <summary>
        /// Filters a <see cref="MethodAttribute{TAttribute}"/> to find instances that match an exact <see cref="IMethodSignature"/>.
        /// </summary>
        /// <param name="source">The <see cref="MethodAttribute{TAttribute}"/> to filter.</param>
        /// <param name="signature">The signature to match.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <typeparam name="TSignature">The <see cref="IMethodSignature"/> type.</typeparam>
        /// <returns><see cref="MethodAttribute{TAttribute}"/> instances that match the <see cref="IMethodSignature"/>.</returns>
        public static IEnumerable<MethodAttribute<TAttribute>> WithSignature<TAttribute, TSignature>(
            this IEnumerable<MethodAttribute<TAttribute>> source,
            TSignature signature,
            Action<MethodAttribute<TAttribute>, TSignature> onMismatchCallback = null)
            where TAttribute : Attribute
            where TSignature : IMethodSignature
        {
            return source.WithConstraint(new MethodMatchesSignature<TAttribute, TSignature>(), signature, onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="MethodAttribute{TAttribute}"/> to find instances that can be called from a <see cref="IParameterPack"/>.
        /// </summary>
        /// <param name="source">The <see cref="MethodAttribute{TAttribute}"/> to filter.</param>
        /// <param name="parameters">The parameter pack to match.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <typeparam name="TParameterPack">The <see cref="IParameterPack"/> type.</typeparam>
        /// <returns><see cref="MethodAttribute{TAttribute}"/> instances that match the <see cref="IMethodSignature"/>.</returns>
        public static IEnumerable<MethodAttribute<TAttribute>> CallableWith<TAttribute, TParameterPack>(
            this IEnumerable<MethodAttribute<TAttribute>> source,
            TParameterPack parameters,
            Action<MethodAttribute<TAttribute>, TParameterPack> onMismatchCallback = null)
            where TAttribute : Attribute
            where TParameterPack : IParameterPack
        {
            return source.WithConstraint(new MethodCallableWithParameterPack<TAttribute, TParameterPack>(), parameters, onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="MethodAttribute{TAttribute}"/> to find instances on static methods.
        /// </summary>
        /// <param name="source">The <see cref="MethodAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="MethodAttribute{TAttribute}"/> instances on static methods.</returns>
        public static IEnumerable<MethodAttribute<TAttribute>> Static<TAttribute>(
            this IEnumerable<MethodAttribute<TAttribute>> source,
            Action<MethodAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsStaticMethod<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="MethodAttribute{TAttribute}"/> to find instances on instance methods.
        /// </summary>
        /// <param name="source">The <see cref="MethodAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="MethodAttribute{TAttribute}"/> instances on instance methods.</returns>
        public static IEnumerable<MethodAttribute<TAttribute>> Instance<TAttribute>(
            this IEnumerable<MethodAttribute<TAttribute>> source,
            Action<MethodAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsInstanceMethod<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="MethodAttribute{TAttribute}"/> to find instances on generic methods.
        /// </summary>
        /// <param name="source">The <see cref="MethodAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="MethodAttribute{TAttribute}"/> instances on generic methods.</returns>
        public static IEnumerable<MethodAttribute<TAttribute>> Generic<TAttribute>(
            this IEnumerable<MethodAttribute<TAttribute>> source,
            Action<MethodAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsGenericMethod<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="MethodAttribute{TAttribute}"/> to find instances on non-generic methods.
        /// </summary>
        /// <param name="source">The <see cref="MethodAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="MethodAttribute{TAttribute}"/> instances on non-generic methods.</returns>
        public static IEnumerable<MethodAttribute<TAttribute>> NonGeneric<TAttribute>(
            this IEnumerable<MethodAttribute<TAttribute>> source,
            Action<MethodAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsNonGenericMethod<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="MethodAttribute{TAttribute}"/> to find instances on public methods.
        /// </summary>
        /// <param name="source">The <see cref="MethodAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="MethodAttribute{TAttribute}"/> instances on public methods.</returns>
        public static IEnumerable<MethodAttribute<TAttribute>> Public<TAttribute>(
            this IEnumerable<MethodAttribute<TAttribute>> source,
            Action<MethodAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsPublicMethod<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="MethodAttribute{TAttribute}"/> to find instances on private methods.
        /// </summary>
        /// <param name="source">The <see cref="MethodAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="MethodAttribute{TAttribute}"/> instances on private methods.</returns>
        public static IEnumerable<MethodAttribute<TAttribute>> Private<TAttribute>(
            this IEnumerable<MethodAttribute<TAttribute>> source,
            Action<MethodAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsPrivateMethod<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="MethodAttribute{TAttribute}"/> to find instances on protected methods.
        /// </summary>
        /// <param name="source">The <see cref="MethodAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="MethodAttribute{TAttribute}"/> instances on protected methods.</returns>
        public static IEnumerable<MethodAttribute<TAttribute>> Protected<TAttribute>(
            this IEnumerable<MethodAttribute<TAttribute>> source,
            Action<MethodAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsProtectedMethod<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="MethodAttribute{TAttribute}"/> to find instances on internal methods.
        /// </summary>
        /// <param name="source">The <see cref="MethodAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="MethodAttribute{TAttribute}"/> instances on internal methods.</returns>
        public static IEnumerable<MethodAttribute<TAttribute>> Internal<TAttribute>(
            this IEnumerable<MethodAttribute<TAttribute>> source,
            Action<MethodAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsInternalMethod<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="MethodAttribute{TAttribute}"/> to find instances on abstract methods.
        /// </summary>
        /// <param name="source">The <see cref="MethodAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="MethodAttribute{TAttribute}"/> instances on abstract methods.</returns>
        public static IEnumerable<MethodAttribute<TAttribute>> Abstract<TAttribute>(
            this IEnumerable<MethodAttribute<TAttribute>> source,
            Action<MethodAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsAbstractMethod<TAttribute>(), onMismatchCallback);
        }

        /// <summary>
        /// Filters a <see cref="MethodAttribute{TAttribute}"/> to find instances on concrete methods.
        /// </summary>
        /// <param name="source">The <see cref="MethodAttribute{TAttribute}"/> to filter.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
        /// <returns><see cref="MethodAttribute{TAttribute}"/> instances on concrete methods.</returns>
        public static IEnumerable<MethodAttribute<TAttribute>> Concrete<TAttribute>(
            this IEnumerable<MethodAttribute<TAttribute>> source,
            Action<MethodAttribute<TAttribute>> onMismatchCallback = null)
            where TAttribute : Attribute
        {
            return source.WithConstraint(new IsConcreteMethod<TAttribute>(), onMismatchCallback);
        }
    }
}
