using System;

namespace Unity.Tiny
{
    /// <summary>
    /// Add this attribute on a type or field to enable migration from pre-generated object IDs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = true)]
    public class IdAliasAttribute : Attribute
    {
        public string Alias { get; }

        public IdAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }
}
