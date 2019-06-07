using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    [UsedImplicitly]
    internal class Float2Inspector : IStructInspector<float2>
    {
        private Vector2Field m_VectorField;

        public VisualElement Build(InspectorDataProxy<float2> proxy)
        {
            m_VectorField = new Vector2Field(proxy.Name);
            m_VectorField.AddToClassList(proxy.Name);

            SetInputClass(m_VectorField.Q<FloatField>("unity-x-input"), "x");
            SetInputClass(m_VectorField.Q<FloatField>("unity-y-input"), "y");

            m_VectorField.RegisterValueChangedCallback(evt => ValueChanged(proxy, evt));
            return m_VectorField;
        }

        private static void ValueChanged(InspectorDataProxy<float2> parent, ChangeEvent<Vector2> evt)
        {
            parent.Data = evt.newValue;
        }

        public void Update(InspectorDataProxy<float2> proxy)
        {
            m_VectorField.SetValueWithoutNotify(proxy.Data);
        }

        private static void SetInputClass(FloatField field, string label)
        {
            field.AddToClassList(label);
        }
    }

}
