using Unity.Entities;

namespace Unity.Authoring.Core
{
    /// <summary>
    /// Allows you to show components in a user-specified order in the Inspector.
    /// </summary>
    [HideInInspector]
    public struct ComponentOrder : IBufferElementData
    {
        public ulong StableTypeHash;
    }
}
