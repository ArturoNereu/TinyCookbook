using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    [UsedImplicitly]
    internal class Float3Inspector : IStructInspector<float3>
    {
        private Vector3Field m_VectorField;

        public VisualElement Build(InspectorDataProxy<float3> proxy)
        {
            m_VectorField = new Vector3Field(proxy.Name);
            m_VectorField.AddToClassList(proxy.Name);

            SetInputClass(m_VectorField.Q<FloatField>("unity-x-input"), "x");
            SetInputClass(m_VectorField.Q<FloatField>("unity-y-input"), "y");
            SetInputClass(m_VectorField.Q<FloatField>("unity-z-input"), "z");

            m_VectorField.RegisterValueChangedCallback(evt => ValueChanged(proxy, evt));
            return m_VectorField;
        }

        private static void ValueChanged(InspectorDataProxy<float3> parent, ChangeEvent<Vector3> evt)
        {
            parent.Data = evt.newValue;
        }

        public void Update(InspectorDataProxy<float3> proxy)
        {
            m_VectorField.SetValueWithoutNotify(proxy.Data);
        }

        private static void SetInputClass(FloatField field, string label)
        {
            field.AddToClassList(label);
        }
    }

}
