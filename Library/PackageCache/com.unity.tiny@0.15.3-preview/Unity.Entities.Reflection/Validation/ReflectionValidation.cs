using System;

namespace Unity.Entities.Reflection
{
    internal static class ReflectionValidation
    {
        public static string AbstractValidationMessage<TAttribute>(TypeAttribute<TAttribute> attribute, string prefix = null)
            where TAttribute : Attribute
            => GetTypePropertyValidationString(prefix, attribute.Type.FullName, $"{(attribute.Type.IsAbstract ? "not " : "")} be abstract");

        public static string StaticValidationMessage<TAttribute>(TypeAttribute<TAttribute> attribute, string prefix = null)
            where TAttribute : Attribute
            => GetTypePropertyValidationString(prefix, attribute.Type.FullName, $"{(attribute.Type.IsSealed && attribute.Type.IsAbstract ? "not " : "")}be static");

        public static string GenericValidationMessage<TAttribute>(TypeAttribute<TAttribute> attribute, string prefix = null)
            where TAttribute : Attribute
            => GetTypePropertyValidationString(prefix, attribute.Type.FullName, $"{(attribute.Type.IsGenericType ? "not " : "")}be generic");

        public static string StaticValidationMessage<TAttribute>(MethodAttribute<TAttribute> attribute, string prefix)
            where TAttribute : Attribute
            => GetMethodPropertyValidationString(prefix, attribute.Type.FullName, attribute.Method.Name, $"{(attribute.Method.IsStatic ? "not " : "")}be static");

        public static string GenericValidationMessage<TAttribute>(MethodAttribute<TAttribute> attribute, string prefix)
            where TAttribute : Attribute
            => GetMethodPropertyValidationString(prefix, attribute.Type.FullName, attribute.Method.Name, $"{(attribute.Method.IsStatic ? "not " : "")}be generic");

        public static string SignatureValidationMessage<TAttribute>(MethodAttribute<TAttribute> attribute, IMethodSignature signature, string prefix)
            where TAttribute : Attribute
            => GetMethodPropertyValidationString(prefix, attribute.Type.FullName, attribute.Method.Name, $"match `{signature.GetSignatureName()}` signature");

        public static string ParameterPackValidationMessage<TAttribute, TParameterPack>(MethodAttribute<TAttribute> attribute, TParameterPack parameters, string prefix)
            where TAttribute : Attribute
            where TParameterPack : IParameterPack
            => GetMethodPropertyValidationString(prefix, attribute.Type.FullName, attribute.Method.Name, $"be callable using `{parameters.GetPackName()}` parameters");

        private static string GetTypePropertyValidationString(string prefix, string typeName, string constraint)
            => $"{prefix}{(string.IsNullOrEmpty(prefix) ? "" : ": ")}`{typeName}` type must {constraint}.";

        private static string GetMethodPropertyValidationString(string prefix, string typeName, string methodName, string constraint)
            => $"{prefix}{(string.IsNullOrEmpty(prefix) ? "" : ": ")}`{typeName}.{methodName}` method must {constraint}.";
    }
}

