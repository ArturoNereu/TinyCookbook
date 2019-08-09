using System;
using Unity.Entities;

namespace Unity.Authoring
{
    /// <summary>
    /// Base class that defines which systems are called during the export pipeline in order to do build-time data conversions.
    /// </summary>
    [DisableAutoCreation]
    public sealed class SceneConversionSystemGroup : ComponentSystemGroup
    {
    }

    /// <summary>
    /// Attribute that sets the current version of system. Changing the version number invalidates any caching that may have occurred.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SystemVersionAttribute : Attribute
    {
        public int Version { get; }

        public SystemVersionAttribute(int version)
        {
            Version = version;
        }
    }
}
