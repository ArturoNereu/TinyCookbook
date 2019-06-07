using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    [UsedImplicitly]
    internal class Float4Inspector : IStructInspector<float4>
    {
        private Vector4Field m_VectorField;

        public VisualElement Build(InspectorDataProxy<float4> proxy)
        {
            m_VectorField = new Vector4Field(proxy.Name);
            m_VectorField.AddToClassList(proxy.Name);

            SetInputClass(m_VectorField.Q<FloatField>("unity-x-input"), "x");
            SetInputClass(m_VectorField.Q<FloatField>("unity-y-input"), "y");
            SetInputClass(m_VectorField.Q<FloatField>("unity-z-input"), "z");
            SetInputClass(m_VectorField.Q<FloatField>("unity-w-input"), "w");

            m_VectorField.RegisterValueChangedCallback(evt => ValueChanged(proxy, evt));
            return m_VectorField;
        }

        private static void ValueChanged(InspectorDataProxy<float4> parent, ChangeEvent<Vector4> evt)
        {
            parent.Data = evt.newValue;
        }

        public void Update(InspectorDataProxy<float4> proxy)
        {
            m_VectorField.SetValueWithoutNotify(proxy.Data);
        }

        private static void SetInputClass(FloatField field, string label)
        {
            field.AddToClassList(label);
        }
    }

}
