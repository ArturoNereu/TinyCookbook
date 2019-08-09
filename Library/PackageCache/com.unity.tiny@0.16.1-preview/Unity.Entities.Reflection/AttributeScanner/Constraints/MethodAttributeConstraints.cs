using System;

namespace Unity.Entities.Reflection
{
    /// <inheritdoc cref="IConstraint{T1, T2}"/>
    /// <summary>
    /// Tests a method against a specific signature constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    /// <typeparam name="TSignature">The <see cref="IMethodSignature"/> type.</typeparam>
    public struct MethodMatchesSignature<TAttribute, TSignature> : IConstraint<MethodAttribute<TAttribute>, TSignature>
        where TAttribute : Attribute
        where TSignature : IMethodSignature
    {
        /// <summary>
        /// Checks whether the method matches a signature.
        /// </summary>
        /// <param name="methodAttribute">The <see cref="MethodAttribute{TAttribute}"/> instance to check.</param>
        /// <param name="signature">The <see cref="IMethodSignature"/> signature.</param>
        /// <returns>True if the method satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(MethodAttribute<TAttribute> methodAttribute, TSignature signature)
            => signature.MatchesExactly(methodAttribute.Method);
    }

    /// <inheritdoc cref="IConstraint{T1, T2}"/>
    /// <summary>
    /// Tests a method against a parameter pack constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    /// <typeparam name="TParameterPack">The <see cref="IParameterPack"/> type.</typeparam>
    public struct MethodCallableWithParameterPack<TAttribute, TParameterPack> : IConstraint<MethodAttribute<TAttribute>, TParameterPack>
        where TAttribute : Attribute
        where TParameterPack : IParameterPack
    {
        /// <summary>
        /// Checks whether the method can be called using a parameter pack.
        /// </summary>
        /// <param name="methodAttribute">The <see cref="MethodAttribute{TAttribute}"/> instance to check.</param>
        /// /// <param name="parameterPack">The <see cref="IParameterPack"/> pack.</param>
        /// <returns>True if the method satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(MethodAttribute<TAttribute> methodAttribute, TParameterPack parameterPack)
            => parameterPack.CompatibleWith(methodAttribute.Method);
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a method against a static method constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsStaticMethod<TAttribute> : IConstraint<MethodAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the method is a static method.
        /// </summary>
        /// <param name="methodAttribute">The <see cref="MethodAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the method satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(MethodAttribute<TAttribute> methodAttribute)
            => methodAttribute.Method.IsStatic;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a method against an instance method constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsInstanceMethod<TAttribute> : IConstraint<MethodAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the method is an instance method.
        /// </summary>
        /// <param name="methodAttribute">The <see cref="MethodAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the method satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(MethodAttribute<TAttribute> methodAttribute)
            => !methodAttribute.Method.IsStatic;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a method against a generic method constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsGenericMethod<TAttribute> : IConstraint<MethodAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the method is a generic method.
        /// </summary>
        /// <param name="methodAttribute">The <see cref="MethodAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the method satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(MethodAttribute<TAttribute> methodAttribute)
            => methodAttribute.Method.IsGenericMethod;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a method against a non-generic method constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsNonGenericMethod<TAttribute> : IConstraint<MethodAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the method is not a generic method.
        /// </summary>
        /// <param name="methodAttribute">The <see cref="MethodAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the method satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(MethodAttribute<TAttribute> methodAttribute)
            => !methodAttribute.Method.IsGenericMethod;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a method against a public method constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsPublicMethod<TAttribute> : IConstraint<MethodAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the method is a public method.
        /// </summary>
        /// <param name="methodAttribute">The <see cref="MethodAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the method satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(MethodAttribute<TAttribute> methodAttribute)
            => methodAttribute.Method.IsPublic;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a method against a private method constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsPrivateMethod<TAttribute> : IConstraint<MethodAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the method is a private method.
        /// </summary>
        /// <param name="methodAttribute">The <see cref="MethodAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the method satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(MethodAttribute<TAttribute> methodAttribute)
            => methodAttribute.Method.IsPrivate;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a method against a protected method constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsProtectedMethod<TAttribute> : IConstraint<MethodAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the method is a protected method.
        /// </summary>
        /// <param name="methodAttribute">The <see cref="MethodAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the method satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(MethodAttribute<TAttribute> methodAttribute)
            => methodAttribute.Method.IsFamily;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a method against an internal method constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsInternalMethod<TAttribute> : IConstraint<MethodAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the method is an internal method.
        /// </summary>
        /// <param name="methodAttribute">The <see cref="MethodAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the method satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(MethodAttribute<TAttribute> methodAttribute)
            => methodAttribute.Method.IsAssembly;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a method against an abstract method constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsAbstractMethod<TAttribute> : IConstraint<MethodAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the method is an abstract method.
        /// </summary>
        /// <param name="methodAttribute">The <see cref="MethodAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the method satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(MethodAttribute<TAttribute> methodAttribute)
            => methodAttribute.Method.IsAbstract;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a method against a concrete method constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsConcreteMethod<TAttribute> : IConstraint<MethodAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the method is a concrete method.
        /// </summary>
        /// <param name="methodAttribute">The <see cref="MethodAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the method satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(MethodAttribute<TAttribute> methodAttribute)
            => !methodAttribute.Method.IsAbstract;
    }
}
