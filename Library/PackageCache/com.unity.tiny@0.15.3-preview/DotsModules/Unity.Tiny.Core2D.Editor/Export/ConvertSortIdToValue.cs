using Unity.Authoring;
using Unity.Entities;

namespace Unity.Tiny.Core2D.Editor
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SceneConversionSystemGroup))]
    public class ConvertSortIdToValue : ComponentSystem
    {
        private EntityQuery m_LayerSortingComponents;

        protected override void OnCreate()
        {
            m_LayerSortingComponents = EntityManager.CreateEntityQuery(typeof(LayerSorting));
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            Entities.With(m_LayerSortingComponents).ForEach((ref LayerSorting layerSorting) =>
            {
                layerSorting.layer = (short) UnityEngine.SortingLayer.GetLayerValueFromID(layerSorting.id);
            });
        }
    }
}
