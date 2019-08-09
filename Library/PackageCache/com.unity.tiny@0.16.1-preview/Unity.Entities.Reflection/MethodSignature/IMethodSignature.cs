using System;

namespace Unity.Entities.Reflection
{
    /// <summary>
    /// Defines the signature of a method.
    /// </summary>
    public interface IMethodSignature
    {
        Type[] ParameterTypes { get; }
        Type ReturnType { get; }
    }
}

