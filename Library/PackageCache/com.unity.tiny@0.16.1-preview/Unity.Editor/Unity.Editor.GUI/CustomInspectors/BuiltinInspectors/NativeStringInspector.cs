using Unity.Entities;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal abstract class NativeStringInspector<TNativeString> : IStructInspector<TNativeString>
        where TNativeString : struct
    {
        private TextField m_TextField;
        private InspectorDataProxy<TNativeString> m_Proxy;

        protected abstract int MaxSize { get; }
        protected ref InspectorDataProxy<TNativeString> Proxy => ref m_Proxy;

        public virtual VisualElement Build(InspectorDataProxy<TNativeString> proxy)
        {
            m_Proxy = proxy;
            m_TextField = new TextField
            {
                label = proxy.Name,
                maxLength = MaxSize
            };
            m_TextField.RegisterValueChangedCallback(StringChanged);
            return m_TextField;
        }

        protected abstract void SetData(string str);

        private void StringChanged(ChangeEvent<string> evt)
        {
            SetData(evt.newValue);
        }

        public void Update(InspectorDataProxy<TNativeString> proxy)
        {
            m_TextField.SetValueWithoutNotify(proxy.Data.ToString());
        }
    }

    internal class NativeString64Inspector : NativeStringInspector<NativeString64>
    {
        protected override int MaxSize { get; } = NativeString64.MaxLength;

        protected override void SetData(string str)
        {
            Proxy.Data = new NativeString64(str);
        }
    }

    internal class NativeString512Inspector : NativeStringInspector<NativeString512>
    {
        protected override int MaxSize { get; } = NativeString512.MaxLength;

        protected override void SetData(string str)
        {
            Proxy.Data = new NativeString512(str);
        }
    }

    internal class NativeString4096Inspector : NativeStringInspector<NativeString4096>
    {
        protected override int MaxSize { get; } = NativeString4096.MaxLength;

        protected override void SetData(string str)
        {
            Proxy.Data = new NativeString4096(str);
        }
    }
}
