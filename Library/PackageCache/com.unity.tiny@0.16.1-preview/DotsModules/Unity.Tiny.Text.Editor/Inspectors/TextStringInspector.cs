using Unity.Editor;
using Unity.Entities;
using Unity.Tiny.Core;
using UnityEngine.UIElements;

namespace Unity.Tiny.Text.Editor
{
    public class TextStringInspector : IDynamicBufferInspector<TextString>
    {
        private TextField m_TextField;
        private InspectorDataProxy<DynamicBuffer<TextString>> m_Proxy;

        public VisualElement Build(InspectorDataProxy<DynamicBuffer<TextString>> proxy)
        {
            m_TextField = new TextField("Value");
            m_Proxy = proxy;
            m_TextField.RegisterValueChangedCallback(ValueChanged);

            return m_TextField;
        }

        private void ValueChanged(ChangeEvent<string> evt)
        {
            m_Proxy.Data.Reinterpret<char>().FromString(evt.newValue);
        }

        public void Update(InspectorDataProxy<DynamicBuffer<TextString>> proxy)
        {
            m_TextField.value = proxy.Data.Reinterpret<char>().AsString();
        }
    }
}
