using UnityEngine.UIElements;

namespace Unity.Editor
{
    public interface IInspector
    {

    }

    public interface IInspector<T> : IInspector
    {
        /// <summary>
        /// This method is called once to build the hierarchy of <see cref="VisualElement"/>s in the Inspector.
        /// </summary>
        /// <param name="proxy">The <see cref="TComponentData"/> data provider.</param>
        /// <returns>The root <see cref="VisualElement"/> of the Inspector.</returns>
        VisualElement Build(InspectorDataProxy<T> proxy);

        /// <summary>
        /// This method is called if the underlying data changes.
        /// </summary>
        /// <param name="proxy"><see cref="TComponentData"/> data provider.</param>
        void Update(InspectorDataProxy<T> proxy);
    }
}
