namespace Unity.Editor
{
    public interface IStructInspector<T> : IInspector<T>
        where T : struct
    {
    }
}
