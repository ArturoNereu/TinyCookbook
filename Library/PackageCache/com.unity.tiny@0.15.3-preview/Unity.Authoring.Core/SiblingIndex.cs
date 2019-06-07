using Unity.Entities;

namespace Unity.Authoring.Core
{
    /// <summary>
    /// Allows you to keep a deterministic ordering in the hierarchy.
    /// </summary>
    [HideInInspector]
    public struct SiblingIndex : IComponentData
    {
        public int Index;

        public static SiblingIndex Default { get; } = new SiblingIndex {Index = int.MaxValue};
    }
}
