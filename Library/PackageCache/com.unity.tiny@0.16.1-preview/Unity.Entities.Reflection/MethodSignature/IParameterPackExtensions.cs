using System;
using System.Linq;
using System.Reflection;
using Unity.Entities.Reflection.Modifiers;

namespace Unity.Entities.Reflection
{
    public static class IParameterPackExtensions
    {
        /// <summary>
        /// Returns a string version of the parameter pack in the form of `(arA1, ...)`.
        /// </summary>
        /// <param name="parameters">The parameter pack to return as a string.</param>
        /// <typeparam name="TParameterPack">The parameter pack type.</typeparam>
        /// <returns>The string version of the parameter pack.</returns>
        public static string GetPackName<TParameterPack>(this TParameterPack parameters)
            where TParameterPack : IParameterPack
        {
            return $"({string.Join(", ", parameters.ParameterTypes.Select(PrettyNames.GetPrettyName))})";
        }

        /// <summary>
        /// Checks whether the method can be called using a parameter pack.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="method">The method.</param>
        /// <returns>True if the provided method can be called with the parameters, false otherwise.</returns>
        public static bool CompatibleWith<TParameterPack>(this TParameterPack parameters, MethodInfo method)
            where TParameterPack : IParameterPack
        {
            var parameterInfos = method.GetParameters();
            var length = parameterInfos.Length;
            var types = parameters.ParameterTypes;
            if (length < types.Length)
            {
                return false;
            }

            for (var i = 0; i < types.Length; ++i)
            {
                if (!IsAssignable(parameterInfos[i], types[i]))
                {
                    return false;
                }
            }

            for (var i = types.Length; i < length; ++i)
            {
                if (!parameterInfos[i].IsOptional)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAssignable(ParameterInfo info, Type parameter)
        {
            // in
            if (info.IsIn && parameter.IsGenericType)
            {
                var first = parameter.GenericTypeArguments[0];
                return typeof(In<>) == parameter.GetGenericTypeDefinition() &&
                       info.ParameterType.GetElementType().IsAssignableFrom(first);
            }

            // ref
            if (false == info.IsOut && info.ParameterType.IsByRef && parameter.IsGenericType)
            {
                var first = parameter.GenericTypeArguments[0];
                return typeof(Ref<>) == parameter.GetGenericTypeDefinition() &&
                       info.ParameterType.GetElementType().IsAssignableFrom(first);
            }

            // out
            if (info.IsOut && parameter.IsGenericType)
            {
                var first = parameter.GenericTypeArguments[0];
                return typeof(Out<>) == parameter.GetGenericTypeDefinition() &&
                       info.ParameterType.GetElementType().IsAssignableFrom(first);
            }

            return info.ParameterType.IsAssignableFrom(parameter);
        }
    }
}
