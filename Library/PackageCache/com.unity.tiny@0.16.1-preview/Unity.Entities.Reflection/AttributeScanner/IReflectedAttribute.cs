using System;

namespace Unity.Entities.Reflection
{
    internal interface IReflectedAttribute<out TAttribute>
        where TAttribute: Attribute
    {
        TAttribute Attribute { get; }
    }
}