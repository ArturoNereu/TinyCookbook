using System;
using Unity.Authoring.Hashing;

namespace Unity.Authoring
{
    internal static class AssetsScene
    {
        public static readonly string Path = "Assets";
        public static readonly Guid Guid = GuidUtility.NewGuid(Path);
    }
}
