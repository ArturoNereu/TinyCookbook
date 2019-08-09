using System;
using System.Linq;
using System.Reflection;
using Unity.Entities.Reflection.Modifiers;

namespace Unity.Entities.Reflection
{
    public static class IMethodSignatureExtensions
    {
        /// <summary>
        /// Returns a string version of a signature in the form of `(arA1, ...)=>ReturnType`.
        /// </summary>
        /// <param name="signature">The signature to return as a string.</param>
        /// <typeparam name="TSignature">The signature type.</typeparam>
        /// <returns>The string version of the signature.</returns>
        public static string GetSignatureName<TSignature>(this TSignature signature)
            where TSignature : IMethodSignature
        {
            return $"({string.Join(", ", signature.ParameterTypes.Select(PrettyNames.GetPrettyName))})=>{PrettyNames.GetPrettyName(signature.ReturnType)}";
        }

        /// <summary>
        /// Checks whether the method matches the signature exactly.
        /// </summary>
        /// <param name="signature">The signature.</param>
        /// <param name="info">The method.</param>
        /// <returns>True if the provided method matches the signature exactly, false otherwise.</returns>
        public static bool MatchesExactly(this IMethodSignature signature, MethodInfo info)
            => MatchesSignatureExactly(
                info.GetParameters(), info.ReturnParameter,
                signature.ParameterTypes, signature.ReturnType);

        private static bool MatchesSignatureExactly(
            ParameterInfo[] methodParameters, ParameterInfo methodReturnParameter,
            Type[] signatureParameters, Type signatureReturnParameter)
        {
            return MatchesMethodParameter(methodParameters, signatureParameters)
                   && null != methodReturnParameter
                   && MatchesMethodReturn(methodReturnParameter, signatureReturnParameter);
        }

        private static bool MatchesMethodParameter(ParameterInfo[] infos, params Type[] parameters)
        {
            if (infos.Length != parameters.Length)
            {
                return false;
            }

            for (var i = 0; i < infos.Length; ++i)
            {
                if (false == MatchesMethodParameter(infos[i], parameters[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MatchesMethodParameter(ParameterInfo info, Type parameter)
        {
            // return value
            if (info.IsRetval)
            {
                return false;
            }

            // in
            if (info.IsIn && parameter.IsGenericType)
            {
                var first = parameter.GenericTypeArguments[0];
                return typeof(In<>) == parameter.GetGenericTypeDefinition() &&
                       info.ParameterType.GetElementType() == first;
            }

            // ref
            if (false == info.IsOut && info.ParameterType.IsByRef && parameter.IsGenericType)
            {
                var first = parameter.GenericTypeArguments[0];
                return typeof(Ref<>) == parameter.GetGenericTypeDefinition() &&
                       info.ParameterType.GetElementType() == first;
            }

            // out
            if (info.IsOut && parameter.IsGenericType)
            {
                var first = parameter.GenericTypeArguments[0];
                return typeof(Out<>) == parameter.GetGenericTypeDefinition() &&
                       info.ParameterType.GetElementType() == first;
            }

            return info.ParameterType == parameter;
        }

        private static bool MatchesMethodReturn(ParameterInfo info, Type parameter)
        {
            if (!info.IsRetval)
            {
                return false;
            }

            return info.ParameterType == parameter;
        }
    }
}
