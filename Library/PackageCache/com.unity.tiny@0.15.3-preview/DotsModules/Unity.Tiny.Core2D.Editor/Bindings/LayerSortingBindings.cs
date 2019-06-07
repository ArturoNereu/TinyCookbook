using JetBrains.Annotations;
using Unity.Entities;
using Unity.Editor.Bindings;
using UnityEngine;

namespace Unity.Tiny.Core2D.Editor
{
    [UsedImplicitly]
    internal abstract class LayerSortingBaseBindings : IEntityBinding
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            var renderer = context.GetUnityComponent<UnityEngine.Renderer>(entity);
            if (!renderer || null == renderer)
            {
                return;
            }
            renderer.sortingLayerID = 0;
            renderer.sortingOrder = 0;
            UnityEditor.EditorUtility.SetDirty(renderer);
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var renderer = context.GetUnityComponent<UnityEngine.Renderer>(entity);
            if (!renderer || null == renderer)
            {
                return;
            }
            var layer = context.GetComponentData<LayerSorting>(entity);
            renderer.sortingLayerID = layer.id;
            renderer.sortingOrder = layer.order;
            UnityEditor.EditorUtility.SetDirty(renderer);
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var renderer = context.GetUnityComponent<UnityEngine.Renderer>(entity);
            if (renderer && null != renderer)
            {
                var sortingLayerId = renderer.sortingLayerID;
                context.SetComponentData(entity, new LayerSorting
                {
                    id = sortingLayerId,
                    layer = (short) SortingLayer.GetLayerValueFromID(sortingLayerId),
                    order = (short) renderer.sortingOrder
                });
            }
        }
    }

    internal class LayerSortingWithSprite2DRendererBindings : LayerSortingBaseBindings,
        IComponentBinding<LayerSorting>,
        IExcludeComponentBinding<SortingGroup>
    {
    }
}
