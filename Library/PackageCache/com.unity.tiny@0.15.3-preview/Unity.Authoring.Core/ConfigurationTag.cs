using Unity.Entities;

namespace Unity.Authoring.Core
{
    /// <summary>
    /// Tag component that identifies an entity as the unique configuration entity.
    /// </summary>
    [HideInInspector]
    public struct ConfigurationTag : IComponentData
    {
    }
}
