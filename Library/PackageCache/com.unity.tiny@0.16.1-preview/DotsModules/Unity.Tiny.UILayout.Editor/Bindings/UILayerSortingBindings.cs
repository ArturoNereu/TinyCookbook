using JetBrains.Annotations;
using Unity.Entities;
using Unity.Editor.Bindings;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.UILayout.Editor
{
    internal abstract class UILayerSortingBaseBindings : IEntityBinding
    {
        public virtual void LoadBinding(Entity entity, IBindingContext context)
        {
        }

        public virtual void UnloadBinding(Entity entity, IBindingContext context)
        {
            var canvas = context.GetUnityComponent<UnityEngine.Canvas>(entity);
            canvas.overrideSorting = false;
            canvas.sortingLayerID = 0;
            canvas.sortingOrder = 0;
        }

        public virtual void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var canvas = context.GetUnityComponent<UnityEngine.Canvas>(entity);
            var tinyLayer = context.GetComponentData<LayerSorting>(entity);
            canvas.overrideSorting = true;
            canvas.sortingLayerID = tinyLayer.layer;
            canvas.sortingOrder = tinyLayer.order;
            UnityEditor.EditorUtility.SetDirty(canvas);
        }

        public virtual void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var canvas = context.GetUnityComponent<UnityEngine.Canvas>(entity);
            context.SetComponentData(entity, new LayerSorting
            {
                layer =  (short)canvas.sortingLayerID,
                order = (short)canvas.sortingOrder
            });
        }
    }

    [UsedImplicitly]
    internal class UILayerSortingWithCanvasBindings : UILayerSortingBaseBindings,
        IComponentBinding<LayerSorting>,
        IExcludeComponentBinding<SortingGroup>,
        IBindingDependency<UICanvasBindings>
    {
    }

    internal abstract class UILayerSortingWithNoCanvasBindings : UILayerSortingBaseBindings,
        IComponentBinding<LayerSorting, RectTransform>,
        IExcludeComponentBinding<SortingGroup, UICanvas>,
        IBindingDependency<RectTransformBindings>
    {
        public override void LoadBinding(Entity entity, IBindingContext context)
        {
            context.AddMissingUnityComponent<UnityEngine.Canvas>(entity);
        }

        public override void UnloadBinding(Entity entity, IBindingContext context)
        {
            context.RemoveUnityComponent<UnityEngine.Canvas>(entity);
        }
    }

    [UsedImplicitly]
    internal class LayerSortingWithRectTransformBindings : UILayerSortingWithNoCanvasBindings,
        IBindingDependency<UISprite2DRendererBindings>
    {
    }
}
