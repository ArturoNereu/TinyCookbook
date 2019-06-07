using JetBrains.Annotations;
using Unity.Entities;
using Unity.Editor.Bindings;
using Unity.Tiny.Core2D;
using Unity.Tiny.Core2D.Editor;

namespace Unity.Tiny.UILayout.Editor
{
    [UsedImplicitly]
    internal class RectTransformBindings : IEntityBinding,
        IComponentBinding<Parent, Translation, RectTransform>,
        IBindingDependency<ParentBindings>,
        IBindingDependency<TranslationBindings>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            context.AddMissingUnityComponent<UnityEngine.RectTransform>(entity);
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            // The best thing to do here would be to remove the RectTransform component entirely. However, if you remove
            // it, undo and then redo, Unity will most likely crash, due to a fence operation in native code.
            //RemoveComponent<RectTransform>(entity);
            var rectTransform = context.GetUnityComponent<UnityEngine.RectTransform>(entity);
            if (null == rectTransform)
            {
                return;
            }
            rectTransform.pivot = rectTransform.anchorMin = rectTransform.anchorMax = new UnityEngine.Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = UnityEngine.Vector2.zero;
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var rectTransform = context.GetUnityComponent<UnityEngine.RectTransform>(entity);
            var tinyRectTransform = context.GetComponentData<RectTransform>(entity);
            rectTransform.anchoredPosition = tinyRectTransform.anchoredPosition;
            rectTransform.anchorMin = tinyRectTransform.anchorMin;
            rectTransform.anchorMax = tinyRectTransform.anchorMax;
            rectTransform.sizeDelta = tinyRectTransform.sizeDelta;
            rectTransform.pivot = tinyRectTransform.pivot;
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var rectTransform = context.GetUnityComponent<UnityEngine.RectTransform>(entity);
            context.SetComponentData(entity, new RectTransform
            {
                anchoredPosition = rectTransform.anchoredPosition,
                anchorMin = rectTransform.anchorMin,
                anchorMax = rectTransform.anchorMax,
                sizeDelta = rectTransform.sizeDelta,
                pivot = rectTransform.pivot
            });
        }
    }
}
