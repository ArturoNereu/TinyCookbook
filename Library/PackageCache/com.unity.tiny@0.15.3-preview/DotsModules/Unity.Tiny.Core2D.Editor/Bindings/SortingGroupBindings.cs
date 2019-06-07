using JetBrains.Annotations;
using Unity.Entities;
using Unity.Editor.Bindings;
using UnityEngine;

namespace Unity.Tiny.Core2D.Editor
{
    [UsedImplicitly]
    internal class SortingGroupBindings : IEntityBinding,
        IComponentBinding<SortingGroup>,
        IExcludeComponentBinding<LayerSorting>,
        IBindingDependency<ParentBindings>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            context.AddMissingUnityComponent<UnityEngine.Rendering.SortingGroup>(entity);
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            context.RemoveUnityComponent<UnityEngine.Rendering.SortingGroup>(entity);
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            // Nothing to do.
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            // Nothing to do.
        }
    }

    [UsedImplicitly]
    internal class SortingGroupWithLayerSortingBindings : IEntityBinding,
        IComponentBinding<SortingGroup, LayerSorting>,
        IBindingDependency<ParentBindings>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            context.AddMissingUnityComponent<UnityEngine.Rendering.SortingGroup>(entity);
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            context.RemoveUnityComponent<UnityEngine.Rendering.SortingGroup>(entity);
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var group = context.GetUnityComponent<UnityEngine.Rendering.SortingGroup>(entity);
            var tinyLayer = context.GetComponentData<LayerSorting>(entity);

            group.sortingLayerID = tinyLayer.id;
            group.sortingOrder = tinyLayer.order;
            UnityEditor.EditorUtility.SetDirty(group);
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var group = context.GetUnityComponent<UnityEngine.Rendering.SortingGroup>(entity);
            if (group && null != group)
            {
                var sortingLayerId = @group.sortingLayerID;
                context.SetComponentData(entity, new LayerSorting
                {
                    id = sortingLayerId,
                    layer = (short) SortingLayer.GetLayerValueFromID(sortingLayerId),
                    order = (short) group.sortingOrder
                });
            }
        }
    }
}
