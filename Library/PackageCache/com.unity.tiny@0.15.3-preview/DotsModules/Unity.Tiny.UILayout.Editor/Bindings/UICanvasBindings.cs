using JetBrains.Annotations;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Editor.Bindings;

namespace Unity.Tiny.UILayout.Editor
{
    [UsedImplicitly]
    internal class UICanvasBindings : IEntityBinding,
        IComponentBinding<Parent>,
        IComponentBinding<RectTransform>,
        IComponentBinding<UICanvas>
    {
        public void LoadBinding(Entity entity, IBindingContext context)
        {
            context.AddMissingUnityComponent<UnityEngine.Canvas>(entity);
            context.AddMissingUnityComponent<UnityEngine.UI.CanvasScaler>(entity);
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            context.RemoveUnityComponent<UnityEngine.UI.CanvasScaler>(entity);
            context.RemoveUnityComponent<UnityEngine.Canvas>(entity);
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var canvas = context.GetUnityComponent<UnityEngine.Canvas>(entity);
            var uiCanvas = context.GetComponentData<UICanvas>(entity);

            canvas.worldCamera = context.GetUnityComponent<UnityEngine.Camera>(uiCanvas.camera);

            var scaler = context.GetUnityComponent<UnityEngine.UI.CanvasScaler>(entity);
            scaler.referenceResolution = uiCanvas.referenceResolution;
            scaler.matchWidthOrHeight = uiCanvas.matchWidthOrHeight;
            scaler.uiScaleMode = uiCanvas.uiScaleMode.Convert();

            SetUnsupportedFields(canvas, scaler);
            UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(canvas.transform as UnityEngine.RectTransform);
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var canvas = context.GetUnityComponent<UnityEngine.Canvas>(entity);
            var scaler = context.GetUnityComponent<UnityEngine.UI.CanvasScaler>(entity);

            context.SetComponentData(entity, new UICanvas
            {
                matchWidthOrHeight = scaler.matchWidthOrHeight,
                referenceResolution = scaler.referenceResolution,
                uiScaleMode = scaler.uiScaleMode.Convert(),
                camera = context.GetEntityFromUnityComponent(canvas)
            });

            SetUnsupportedFields(canvas, scaler);
        }

        private static void SetUnsupportedFields(UnityEngine.Canvas canvas, UnityEngine.UI.CanvasScaler scaler)
        {
            canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera;
            canvas.pixelPerfect = false;
            scaler.referencePixelsPerUnit = 1;
        }
    }
}
