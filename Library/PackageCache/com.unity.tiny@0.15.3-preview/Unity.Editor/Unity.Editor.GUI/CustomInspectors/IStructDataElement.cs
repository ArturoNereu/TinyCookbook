using Unity.Properties;

namespace Unity.Editor
{
    internal interface IStructDataElement
    {
        /// <summary>
        /// Called to build the inspector when a <see cref="IComponentInspector{TComponentData}"/> is defined.
        /// </summary>
        void BuildFromVisitor<TProperty, TContainer, TValue>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref TValue value, InspectorContext context)
            where TProperty : IProperty<TContainer, TValue>;
    }

    internal interface IStructDataElement<T> : IStructDataElement, IDataProvider<T>
        where T : struct
    {
    }
}
