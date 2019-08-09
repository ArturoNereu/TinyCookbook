using Unity.Entities;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal class EntityGuidInspector : IComponentInspector<EntityGuid>
    {
        private TextField m_TextField;
        public VisualElement Build(InspectorDataProxy<EntityGuid> proxy)
        {
            m_TextField = new TextField {label = "guid"};
            m_TextField.RegisterValueChangedCallback(ValueChanged);
            return m_TextField;
        }

        // HACK: We want to allow people to select and copy the guid, but not modify it.
        private void ValueChanged(ChangeEvent<string> evt)
        {
            m_TextField.SetValueWithoutNotify(evt.previousValue);
        }

        public void Update(InspectorDataProxy<EntityGuid> proxy)
        {
            m_TextField.SetValueWithoutNotify(proxy.Data.ToString());
        }
    }
}
