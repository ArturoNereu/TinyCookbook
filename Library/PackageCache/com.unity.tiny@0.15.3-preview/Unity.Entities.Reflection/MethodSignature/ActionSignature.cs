using System;

namespace Unity.Entities.Reflection
{
    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of an action that takes no parameters.
    /// </summary>
    public struct ActionSignature : IMethodSignature
    {
        private static readonly ParameterPack k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(void);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of an action that takes one parameter.
    /// </summary>
    public struct ActionSignature<T1> : IMethodSignature
    {
        private static readonly ParameterPack<T1> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(void);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of an action that takes two parameters.
    /// </summary>
    public struct ActionSignature<T1, T2> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(void);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of an action that takes three parameters.
    /// </summary>
    public struct ActionSignature<T1, T2, T3> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2, T3> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(void);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of an action that takes four parameters.
    /// </summary>
    public struct ActionSignature<T1, T2, T3, T4> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2, T3, T4> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(void);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of an action that takes five parameters.
    /// </summary>
    public struct ActionSignature<T1, T2, T3, T4, T5> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2, T3, T4, T5> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(void);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of an action that takes six parameters.
    /// </summary>
    public struct ActionSignature<T1, T2, T3, T4, T5, T6> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2, T3, T4, T5, T6> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(void);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <summary>
    /// Defines the signature of an action that takes seven parameters.
    /// </summary>
    public struct ActionSignature<T1, T2, T3, T4, T5, T6, T7> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2, T3, T4, T5, T6, T7> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(void);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of an action that takes eight parameters.
    /// </summary>
    public struct ActionSignature<T1, T2, T3, T4, T5, T6, T7, T8> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2, T3, T4, T5, T6, T7, T8> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(void);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }
}
