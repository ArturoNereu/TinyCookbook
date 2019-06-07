using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    [UsedImplicitly]
    internal class QuaternionInspector : IStructInspector<quaternion>
    {
        private Vector3Field m_VectorField;

        public VisualElement Build(InspectorDataProxy<quaternion> proxy)
        {
            m_VectorField = new Vector3Field(proxy.Name);
            m_VectorField.AddToClassList(proxy.Name);

            SetInputClass(m_VectorField.Q<FloatField>("unity-x-input"), "x");
            SetInputClass(m_VectorField.Q<FloatField>("unity-y-input"), "y");
            SetInputClass(m_VectorField.Q<FloatField>("unity-z-input"), "z");

            m_VectorField.RegisterValueChangedCallback(evt => ValueChanged(proxy, evt));
            Update(proxy);
            return m_VectorField;
        }

        private static void ValueChanged(InspectorDataProxy<quaternion> parent, ChangeEvent<Vector3> evt)
        {
            var radians = evt.newValue;
            parent.Data = quaternion.Euler(Mathf.Deg2Rad * radians);
        }

        public void Update(InspectorDataProxy<quaternion> proxy)
        {
            var v = proxy.Data.value;
            m_VectorField.SetValueWithoutNotify(new Quaternion(v.x, v.y, v.z, v.w).eulerAngles);
        }

        private static void SetInputClass(FloatField field, string label)
        {
            field.AddToClassList(label);
        }
    }
}
