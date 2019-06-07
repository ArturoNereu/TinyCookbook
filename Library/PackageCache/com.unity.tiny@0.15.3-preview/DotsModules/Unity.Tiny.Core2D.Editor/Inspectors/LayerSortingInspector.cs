using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.Tiny.Core2D;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    public class LayerSortingInspector : IComponentInspector<LayerSorting>
    {
        private InspectorDataProxy<LayerSorting> m_Proxy;
        private PopupField<string> m_Layers;
        private IntegerField m_Order;
        private List<SortingLayer> m_Names;

        public VisualElement Build(InspectorDataProxy<LayerSorting> proxy)
        {
            m_Proxy = proxy;
            var root = new VisualElement();
            m_Names = UnityEngine.SortingLayer.layers.ToList();
            m_Layers = new PopupField<string>(
                nameof(LayerSorting.layer),
                m_Names.Select(sl => sl.name).ToList(),
                0,
                null, null);
            m_Layers.RegisterValueChangedCallback(LayerChanged);

            m_Order = new IntegerField(nameof(LayerSorting.order));
            m_Order.RegisterValueChangedCallback(OrderChanged);

            root.contentContainer.Add(m_Layers);
            root.contentContainer.Add(m_Order);

            return root;
        }

        private void OrderChanged(ChangeEvent<int> evt)
        {
            var data = m_Proxy.Data;
            data.order = (short)evt.newValue;
            m_Proxy.Data = data;
        }

        private void LayerChanged(ChangeEvent<string> evt)
        {
            var data = m_Proxy.Data;
            var id = SortingLayer.NameToID(evt.newValue);
            data.id = id;
            data.layer = (short)SortingLayer.GetLayerValueFromID(id);
            m_Proxy.Data = data;
        }

        public void Update(InspectorDataProxy<LayerSorting> proxy)
        {
            var data = proxy.Data;
            var index = m_Names.FindIndex(l => l.id == data.id);
            m_Layers.index = index < 0 ? 0 : index;
            m_Order.SetValueWithoutNotify(data.order);
        }
    }
}
