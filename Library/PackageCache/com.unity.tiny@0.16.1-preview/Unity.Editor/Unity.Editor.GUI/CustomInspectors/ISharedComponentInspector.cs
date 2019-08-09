using Unity.Entities;

namespace Unity.Editor
{
    public interface ISharedComponentInspector<TSharedComponentData> : IStructInspector<TSharedComponentData>
        where TSharedComponentData : struct, ISharedComponentData
    {
    }
}
