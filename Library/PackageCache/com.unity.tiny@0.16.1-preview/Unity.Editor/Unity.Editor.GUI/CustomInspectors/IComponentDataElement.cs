using System;
using Unity.Entities;

namespace Unity.Editor
{
    /// <summary>
    /// Defines the API of a <see cref="VisualElement"/> that is used to store an <see cref="IComponentInspector"/>
    /// </summary>
    internal interface IComponentDataElement : IStructDataElement
    {
        Type ComponentType { get; }
        void RegisterUpdater(IDataUpdater updater);
        void SetDataAtOffset<T>(T data, int offset) where T : struct;
        T GetDataAtOffset<T>(int offset) where T : struct;
    }

    /// <inheritdoc />
    /// <typeparam name="TComponentData">The <see cref="IComponentData"/> type to inspect.</typeparam>
    internal interface IComponentDataElement<TComponentData> : IComponentDataElement, IDataProvider<TComponentData>
        where TComponentData : struct
    {
    }
}
