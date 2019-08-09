using System;
using Unity.Authoring.Hashing;

namespace Unity.Authoring
{
    internal static class ConfigurationScene
    {
        public static readonly string Path = "Configuration";
        public static readonly Guid Guid = GuidUtility.NewGuid(Path);
    }
}
