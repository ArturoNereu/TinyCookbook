using JetBrains.Annotations;
using Unity.Tiny.Core2D;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    [UsedImplicitly]
    internal class RectInspector : IStructInspector<Rect>
    {
        private RectField m_Field;

        public VisualElement Build(InspectorDataProxy<Rect> proxy)
        {
            var c = proxy.Data;
            m_Field = new RectField(proxy.Name);
            m_Field.AddToClassList(proxy.Name);
            SetInputClass(m_Field.Q<FloatField>("unity-x-input"), "x");
            SetInputClass(m_Field.Q<FloatField>("unity-y-input"), "y");
            SetInputClass(m_Field.Q<FloatField>("unity-width-input"), "width");
            SetInputClass(m_Field.Q<FloatField>("unity-height-input"), "height");
            m_Field.RegisterValueChangedCallback(evt => RectChanged(proxy, evt));
            return m_Field;
        }

        private static void RectChanged(InspectorDataProxy<Rect> parent, ChangeEvent<UnityEngine.Rect> evt)
        {
            var c = evt.newValue;
            parent.Data = new Rect(c.x, c.y, c.width, c.height);
        }

        public void Update(InspectorDataProxy<Rect> proxy)
        {
            var c = proxy.Data;
            m_Field.SetValueWithoutNotify(new UnityEngine.Rect(c.x, c.y, c.width, c.height));
        }

        private static void SetInputClass(FloatField field, string label)
        {
            field.AddToClassList(label);
        }
    }
}
