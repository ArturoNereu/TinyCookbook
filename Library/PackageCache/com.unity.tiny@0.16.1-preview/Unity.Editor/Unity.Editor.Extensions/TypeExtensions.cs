using System;
using System.Linq;
using System.Reflection;

namespace Unity.Editor.Extensions
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Determines whether the current <see cref="Type"/> derives from the specified raw generic <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The current <see cref="Type"/>.</param>
        /// <param name="generic">The generic <see cref="Type"/> to compare with the current <see cref="Type"/>.</param>
        /// <returns><see langword="true"/> if the current <see cref="Type"/> derives from <paramref name="generic"/>; otherwise, <see langword="false"/>. This method also returns <see langword="false"/> if <paramref name="generic"/> and the current <see cref="Type"/> are equal.</returns>
        public static bool IsSubclassOfRawGeneric(this Type type, Type generic)
        {
            while (type != null && type != typeof(object))
            {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (generic == cur)
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Searches the current <see cref="Type"/> for the specified public method whose parameters match the specified argument types.
        /// </summary>
        /// <param name="type">Type of the object the method belongs to.</param>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
        /// <param name="name">The string containing the name of the method to get.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <returns>A <see cref="MethodInfo"/> object that represents the constructed method formed by substituting the elements of <paramref name="typeArguments"/> for the type parameters of the current generic method definition.</returns>
        internal static MethodInfo GetGenericMethod(this Type type, string name, BindingFlags bindingAttr, params Type[] typeArguments)
        {
            var method = type.GetMethod(name, bindingAttr);
            if (method == null)
            {
                return null;
            }

            var genericMethod = method.MakeGenericMethod(typeArguments);
            if (genericMethod == null || !genericMethod.IsGenericMethod)
            {
                return null;
            }

            return genericMethod;
        }

        /// <summary>
        /// Searches the current <see cref="Type"/> for the specified method whose parameters match the specified argument types and modifiers, using the specified calling convention.
        /// </summary>
        /// <param name="type">Type of the object the method belongs to.</param>
        /// <param name="name">The string containing the name of the method to get.</param>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="obj">The object on which to invoke the method or constructor. If a method is static, this argument is ignored. If a constructor is static, this argument must be <see langword="null"/> or an instance of the class that defines the constructor.</param>
        /// <param name="parameters">An argument list for the invoked method or constructor. This is an array of objects with the same number, order, and type as the parameters of the method or constructor to be invoked. If there are no parameters, parameters should be <see langword="null"/>.</param>
        /// <returns>An object containing the return value of the invoked method, or <see langword="null"/> in the case of a constructor.</returns>
        internal static object InvokeGenericMethod(this Type type, string name, BindingFlags bindingAttr, Type[] typeArguments, object obj, params object[] parameters)
        {
            var genericMethod = GetGenericMethod(type, name, bindingAttr, typeArguments);
            if (genericMethod == null)
            {
                throw new InvalidOperationException($"Method '{name}' with type arguments '{string.Join(", ", typeArguments.Select(t => t.FullName))}' not found on type '{type.FullName}'");
            }
            return genericMethod.Invoke(obj, parameters);
        }
    }
}
