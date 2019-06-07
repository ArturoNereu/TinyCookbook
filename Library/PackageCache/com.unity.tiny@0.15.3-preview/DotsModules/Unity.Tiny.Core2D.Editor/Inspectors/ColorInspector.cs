using JetBrains.Annotations;
using Unity.Tiny.Core2D;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    [UsedImplicitly]
    public class ColorInspector : IStructInspector<Color>
    {
        private ColorField m_Field;

        public VisualElement Build(InspectorDataProxy<Color> proxy)
        {
            var c = proxy.Data;
            m_Field = new ColorField(proxy.Name);
            m_Field.AddToClassList(proxy.Name);
            m_Field.RegisterValueChangedCallback(evt => ColorChanged(proxy, evt));
            return m_Field;
        }

        private static void ColorChanged(InspectorDataProxy<Color> parent, ChangeEvent<UnityEngine.Color> evt)
        {
            var c = evt.newValue;
            parent.Data = new Color(c.r, c.g, c.b, c.a);
        }

        public void Update(InspectorDataProxy<Color> proxy)
        {
            var c = proxy.Data;
            m_Field.SetValueWithoutNotify(new UnityEngine.Color(c.r, c.g, c.b, c.a));
        }
    }
}
