using System;

namespace Unity.Authoring.Core
{
    /// <summary>
    /// Prevents a Component from being exported to run-time data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public class NonExportedAttribute : Attribute
    {
    }
}
