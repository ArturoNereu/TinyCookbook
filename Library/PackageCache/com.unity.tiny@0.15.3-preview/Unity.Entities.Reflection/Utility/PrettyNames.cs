using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Entities.Reflection
{
    internal static class PrettyNames
    {
        private static readonly Dictionary<Type, string> s_ReservedTypeNames;

        static PrettyNames()
        {
            s_ReservedTypeNames = new Dictionary<Type, string>()
            {
                { typeof(bool), "bool" },
                { typeof(byte), "byte" },
                { typeof(char), "char" },
                { typeof(decimal), "decimal" },
                { typeof(double), "double" },
                { typeof(float), "float" },
                { typeof(int), "int" },
                { typeof(long), "long" },
                { typeof(sbyte), "sbyte" },
                { typeof(short), "short" },
                { typeof(string), "string" },
                { typeof(uint), "uint" },
                { typeof(ulong), "ulong" },
                { typeof(ushort), "ushort" },
                { typeof(void), "void" },
            };
        }

        public static string GetPrettyName(Type type)
        {
            if (!type.IsGenericType)
            {
                return s_ReservedTypeNames.TryGetValue(type, out var reservedName) ? reservedName : type.Name;
            }
            var genericArguments = type.GetGenericArguments();
            var typeDefinition = type.Name;
            var unmangledName = typeDefinition.Substring(0, typeDefinition.IndexOf("`"));
            return unmangledName + "<" + string.Join(", ", genericArguments.Select(GetPrettyName)) + ">";
        }
    }
}
