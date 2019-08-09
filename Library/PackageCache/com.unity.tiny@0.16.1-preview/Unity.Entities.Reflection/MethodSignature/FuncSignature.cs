using System;

namespace Unity.Entities.Reflection
{
    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of a function that takes no parameters.
    /// </summary>
    public struct FuncSignature<TReturn> : IMethodSignature
    {
        private static readonly ParameterPack k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(TReturn);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of a function that takes one parameter.
    /// </summary>
    public struct FuncSignature<T1, TReturn> : IMethodSignature
    {
        private static readonly ParameterPack<T1> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(TReturn);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of a function that takes two parameters.
    /// </summary>
    public struct FuncSignature<T1, T2, TReturn> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(TReturn);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of a function that takes three parameters.
    /// </summary>
    public struct FuncSignature<T1, T2, T3, TReturn> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2, T3> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(TReturn);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of a function that takes four parameters.
    /// </summary>
    public struct FuncSignature<T1, T2, T3, T4, TReturn> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2, T3, T4> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(TReturn);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of a function that takes five parameters.
    /// </summary>
    public struct FuncSignature<T1, T2, T3, T4, T5, TReturn> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2, T3, T4, T5> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(TReturn);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of a function that takes six parameters.
    /// </summary>
    public struct FuncSignature<T1, T2, T3, T4, T5, T6, TReturn> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2, T3, T4, T5, T6> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(TReturn);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of a function that takes seven parameters.
    /// </summary>
    public struct FuncSignature<T1, T2, T3, T4, T5, T6, T7, TReturn> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2, T3, T4, T5, T6, T7> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(TReturn);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }

    /// <inheritdoc cref="IMethodSignature"/>>
    /// <summary>
    /// Defines the signature of a function that takes eight parameters.
    /// </summary>
    public struct FuncSignature<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> : IMethodSignature
    {
        private static readonly ParameterPack<T1, T2, T3, T4, T5, T6, T7, T8> k_ParameterPack = default;
        private static readonly Type k_ReturnType = typeof(TReturn);

        public Type[] ParameterTypes => k_ParameterPack.ParameterTypes;
        public Type ReturnType => k_ReturnType;
    }
}
