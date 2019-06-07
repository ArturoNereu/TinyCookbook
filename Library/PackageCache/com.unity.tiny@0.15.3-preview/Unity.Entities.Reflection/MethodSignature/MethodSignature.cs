namespace Unity.Entities.Reflection
{
    /// <summary>
    /// Helper class to get method signatures.
    /// </summary>
    public static class MethodSignature
    {
        public static ActionSignature Action() => default;
        public static ActionSignature<T1> Action<T1>() => default;
        public static ActionSignature<T1, T2> Action<T1, T2>() => default;
        public static ActionSignature<T1, T2, T3> Action<T1, T2, T3>() => default;
        public static ActionSignature<T1, T2, T3, T4> Action<T1, T2, T3, T4>() => default;
        public static ActionSignature<T1, T2, T3, T4, T5> Action<T1, T2, T3, T4, T5>() => default;
        public static ActionSignature<T1, T2, T3, T4, T5, T6> Action<T1, T2, T3, T4, T5, T6>() => default;
        public static ActionSignature<T1, T2, T3, T4, T5, T6, T7> Action<T1, T2, T3, T4, T5, T6, T7>() => default;
        public static ActionSignature<T1, T2, T3, T4, T5, T6, T7, T8> Action<T1, T2, T3, T4, T5, T6, T7, T8>() => default;
        
        public static FuncSignature<TReturn> Func<TReturn>() => default;
        public static FuncSignature<T1, TReturn> Func<T1, TReturn>() => default;
        public static FuncSignature<T1, T2, TReturn> Func<T1, T2, TReturn>() => default;
        public static FuncSignature<T1, T2, T3, TReturn> Func<T1, T2, T3, TReturn>() => default;
        public static FuncSignature<T1, T2, T3, T4, TReturn> Func<T1, T2, T3, T4, TReturn>() => default;
        public static FuncSignature<T1, T2, T3, T4, T5, TReturn> Func<T1, T2, T3, T4, T5, TReturn>() => default;
        public static FuncSignature<T1, T2, T3, T4, T5, T6, TReturn> Func<T1, T2, T3, T4, T5, T6, TReturn>() => default;
        public static FuncSignature<T1, T2, T3, T4, T5, T6, T7, TReturn> Func<T1, T2, T3, T4, T5, T6, T7, TReturn>() => default;
        public static FuncSignature<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>() => default;
        
        public static ParameterPack Params() => default;
        public static ParameterPack<T1> Params<T1>() => default;
        public static ParameterPack<T1, T2> Params<T1, T2>() => default;
        public static ParameterPack<T1, T2, T3> Params<T1, T2, T3>() => default;
        public static ParameterPack<T1, T2, T3, T4> Params<T1, T2, T3, T4>() => default;
        public static ParameterPack<T1, T2, T3, T4, T5> Params<T1, T2, T3, T4, T5>() => default;
        public static ParameterPack<T1, T2, T3, T4, T5, T6> Params<T1, T2, T3, T4, T5, T6>() => default;
        public static ParameterPack<T1, T2, T3, T4, T5, T6, T7> Params<T1, T2, T3, T4, T5, T6, T7>() => default;
        public static ParameterPack<T1, T2, T3, T4, T5, T6, T7, T8> Params<T1, T2, T3, T4, T5, T6, T7, T8>() => default;
    }
}
