using Unity.Entities;

namespace Unity.Editor
{
    public interface IDynamicBufferInspector<TBufferElementData> : IStructInspector<DynamicBuffer<TBufferElementData>>
        where TBufferElementData : struct, IBufferElementData
    {
    }
}
