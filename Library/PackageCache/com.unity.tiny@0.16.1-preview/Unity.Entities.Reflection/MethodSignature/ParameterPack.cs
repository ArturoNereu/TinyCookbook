using System;

namespace Unity.Entities.Reflection
{
    public interface IParameterPack
    {
        Type[] ParameterTypes { get; }
    }
    
    public struct ParameterPack : IParameterPack
    {
        private static readonly Type[] k_ParameterTypes = {};
        public Type[] ParameterTypes => k_ParameterTypes;
    }
    
    public struct ParameterPack<T1> : IParameterPack
    {
        private static readonly Type[] k_ParameterTypes = {typeof(T1)};
        public Type[] ParameterTypes => k_ParameterTypes;
    }
    
    public struct ParameterPack<T1, T2> : IParameterPack
    {
        private static readonly Type[] k_ParameterTypes = {typeof(T1), typeof(T2)};
        public Type[] ParameterTypes => k_ParameterTypes;
    }
    
    public struct ParameterPack<T1, T2, T3> : IParameterPack
    {
        private static readonly Type[] k_ParameterTypes = {typeof(T1), typeof(T2), typeof(T3)};
        public Type[] ParameterTypes => k_ParameterTypes;
    }
    
    public struct ParameterPack<T1, T2, T3, T4> : IParameterPack
    {
        private static readonly Type[] k_ParameterTypes = {typeof(T1), typeof(T2), typeof(T3), typeof(T4)};
        public Type[] ParameterTypes => k_ParameterTypes;
    }
    
    public struct ParameterPack<T1, T2, T3, T4, T5> : IParameterPack
    {
        private static readonly Type[] k_ParameterTypes = {typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)};
        public Type[] ParameterTypes => k_ParameterTypes;
    }
    
    public struct ParameterPack<T1, T2, T3, T4, T5, T6> : IParameterPack
    {
        private static readonly Type[] k_ParameterTypes = {typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)};
        public Type[] ParameterTypes => k_ParameterTypes;
    }
    
    public struct ParameterPack<T1, T2, T3, T4, T5, T6, T7> : IParameterPack
    {
        private static readonly Type[] k_ParameterTypes = {typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)};
        public Type[] ParameterTypes => k_ParameterTypes;
    }
    
    public struct ParameterPack<T1, T2, T3, T4, T5, T6, T7, T8> : IParameterPack
    {
        private static readonly Type[] k_ParameterTypes = {typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)};
        public Type[] ParameterTypes => k_ParameterTypes;
    }
}