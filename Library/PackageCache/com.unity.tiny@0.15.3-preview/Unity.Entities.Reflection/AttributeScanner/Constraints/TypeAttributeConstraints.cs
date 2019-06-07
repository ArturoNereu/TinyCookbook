using System;

namespace Unity.Entities.Reflection
{
    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    ///  Tests a type against a static type constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsStaticType<TAttribute> : IConstraint<TypeAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the type is a static type.
        /// </summary>
        /// <param name="typeAttribute">The <see cref="TypeAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the type satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(TypeAttribute<TAttribute> typeAttribute)
            => typeAttribute.Type.IsSealed && typeAttribute.Type.IsAbstract;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a type against a non-static type constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsNonStaticType<TAttribute> : IConstraint<TypeAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the type is a non-static type.
        /// </summary>
        /// <param name="typeAttribute">The <see cref="TypeAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the type satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(TypeAttribute<TAttribute> typeAttribute)
            => !(typeAttribute.Type.IsSealed && typeAttribute.Type.IsAbstract);
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a type against a generic type constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsGenericType<TAttribute> : IConstraint<TypeAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the type is a generic type.
        /// </summary>
        /// <param name="typeAttribute">The <see cref="TypeAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the type satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(TypeAttribute<TAttribute> typeAttribute)
            => typeAttribute.Type.IsGenericType;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a type against a non-generic type constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsNonGenericType<TAttribute> : IConstraint<TypeAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the type is not a generic type.
        /// </summary>
        /// <param name="typeAttribute">The <see cref="TypeAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the type satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(TypeAttribute<TAttribute> typeAttribute)
            => !typeAttribute.Type.IsGenericType;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a type against an abstract type constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsAbstractType<TAttribute> : IConstraint<TypeAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the type is an abstract type.
        /// </summary>
        /// <param name="typeAttribute">The <see cref="TypeAttribute{TAttribute}"/> instance to check<./param>
        /// <returns>True if the type satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(TypeAttribute<TAttribute> typeAttribute)
            => typeAttribute.Type.IsAbstract;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a type against a concrete type constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsConcreteType<TAttribute> : IConstraint<TypeAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the type is a concrete type.
        /// </summary>
        /// <param name="typeAttribute">The <see cref="TypeAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the type satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(TypeAttribute<TAttribute> typeAttribute)
            => !typeAttribute.Type.IsAbstract;
    }

    /// <inheritdoc cref="IConstraint{T}"/>
    /// <summary>
    /// Tests a type against a sealed type constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="Attribute"/> type.</typeparam>
    public struct IsSealedType<TAttribute> : IConstraint<TypeAttribute<TAttribute>>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the type is a sealed type.
        /// </summary>
        /// <param name="typeAttribute">The <see cref="TypeAttribute{TAttribute}"/> instance to check.</param>
        /// <returns>True if the type satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(TypeAttribute<TAttribute> typeAttribute)
            => typeAttribute.Type.IsSealed;
    }

    /// <inheritdoc cref="IConstraint{T1, T2}"/>
    /// <summary>
    /// Tests a type against a subclass-of-type constraint.
    /// </summary>
    /// <typeparam name="TAttribute">The <see cref="T:System.Attribute" /> type.</typeparam>
    public struct IsSubClassOfType<TAttribute> : IConstraint<TypeAttribute<TAttribute>, Type>
        where TAttribute : Attribute
    {
        /// <summary>
        /// Checks whether the type is a subclass of the provided type.
        /// </summary>
        /// <param name="typeAttribute">The <see cref="TypeAttribute{TAttribute}"/> instance to check.</param>
        /// <param name="parentType"></param>
        /// <returns>True if the type satisfies the constraint, false otherwise.</returns>
        public bool SatisfiesConstraint(TypeAttribute<TAttribute> typeAttribute, Type parentType)
        {
            return typeAttribute.Type.IsSubclassOf(parentType);
        }
    }
}
