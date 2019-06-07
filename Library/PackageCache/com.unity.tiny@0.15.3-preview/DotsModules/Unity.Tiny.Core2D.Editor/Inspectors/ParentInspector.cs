using JetBrains.Annotations;
using Unity.Tiny.Core2D;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    [UsedImplicitly]
    public class ParentInspector : IComponentInspector<Parent>
    {
        public VisualElement Build(InspectorDataProxy<Parent> proxy)
            => null;

        public void Update(InspectorDataProxy<Parent> proxy)
        {
        }
    }
}
