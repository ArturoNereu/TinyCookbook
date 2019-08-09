using Unity.Entities;

namespace Unity.Editor
{
    /// <inheritdoc />
    /// <typeparam name="TComponentData">The <see cref="IComponentData"/> type to inspect.</typeparam>
    public interface IComponentInspector<TComponentData> : IStructInspector<TComponentData>
        where TComponentData : struct, IComponentData
    {
    }
}
