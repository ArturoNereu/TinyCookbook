namespace Unity.Editor
{
    public interface IOffsetDataProvider
    {
        void SetDataAtOffset<TData>(TData data, int offset)
            where TData : struct;

        TData GetDataAtOffset<TData>(int offset)
            where TData : struct;

        void SetDataAtOffset<TData>(TData data, int index, int offset)
            where TData : struct;

        TData GetDataAtOffset<TData>(int index, int offset)
            where TData : struct;
    }
}
