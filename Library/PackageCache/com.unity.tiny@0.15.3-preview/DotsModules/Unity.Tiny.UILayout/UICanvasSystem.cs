using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using Unity.Collections;

namespace Unity.Tiny.UILayout
{
    /// <summary>
    ///  Sets the desired scale for entities with the UICanvas component. The scale
    ///  value is calculated based on the UICanvas.uiScaleMode field.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class UICanvasSystem : ComponentSystem
    {
        private static readonly Translation DefaultCanvasLocalPosition =
            new Translation() { Value = float3.zero };

        private static readonly Rotation DefaultCanvasLocalRotation =
            new Rotation() { Value = quaternion.identity };

        private static readonly NonUniformScale DefaultCanvasLocalScale =
            new NonUniformScale() { Value = new float3(1, 1, 1) };

        private static readonly RectTransform DefaultCanvasRectTransform =
            new RectTransform();

        protected override void OnUpdate()
        {
            AddMissingComponentsToCanvas();

            float2 screenSize = UILayoutService.GetScreenSize(this);

            Entities.ForEach((ref UICanvas canvas, ref RectTransform rectTransform, ref NonUniformScale scale,
                              ref Translation localPosition, ref Rotation localRotation) =>
            {
                if (!EntityManager.HasComponent<Camera2D>(canvas.camera))
                    return;

                var camera = EntityManager.GetComponentData<Camera2D>(canvas.camera);

                float cameraScale = (camera.halfVerticalSize * 2.0f) / screenSize.y;
                float canvasScale = GetUICanvasScale(screenSize, ref canvas);
                scale.Value = canvasScale * cameraScale;

                rectTransform.sizeDelta = screenSize / canvasScale;

                // If the camera is translated or rotated, we need to apply the same transformation on the canvas,
                // so the UI is always in the center.
                AlignCanvasWithCamera(canvas, ref rectTransform, ref localRotation);

                rectTransform.anchoredPosition.x += screenSize.x / 2.0f;
                rectTransform.anchoredPosition.y += screenSize.y / 2.0f;

                // Pivot and anchors never change for the Canvas.
                rectTransform.pivot = new float2(0.5f, 0.5f);
                rectTransform.anchorMin = float2.zero;
                rectTransform.anchorMax = float2.zero;
            });
        }

        private void AddMissingComponentsToCanvas()
        {
            AddMissingComponent<UICanvas, RectTransform>(DefaultCanvasRectTransform);
            AddMissingComponent<UICanvas, Translation>(DefaultCanvasLocalPosition);
            AddMissingComponent<UICanvas, Rotation>(DefaultCanvasLocalRotation);
            AddMissingComponent<UICanvas, NonUniformScale>(DefaultCanvasLocalScale);
        }

        private void AddMissingComponent<T, MissingT>(MissingT c)
            where MissingT : struct, IComponentData
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithAll<T>()
                .WithNone<MissingT>()
                .ForEach(e => ecb.AddComponent(e, c));
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private float GetUICanvasScale(float2 screenSize, ref UICanvas canvas)
        {
            switch (canvas.uiScaleMode)
            {
                case UIScaleMode.ConstantPixelSize:
                    return 1.0f;

                case UIScaleMode.ScaleWithScreenSize:
                    float logWidth = math.log2(screenSize.x / canvas.referenceResolution.x);
                    float logHeight = math.log2(screenSize.y / canvas.referenceResolution.y);
                    float logWeightedAverage = math.lerp(logWidth, logHeight, canvas.matchWidthOrHeight);
                    float canvasScale = math.pow(2, logWeightedAverage);
                    return canvasScale;
            }

            return 1.0f;
        }

        private void AlignCanvasWithCamera(UICanvas canvas, ref RectTransform rectTransform,
                                           ref Rotation localRotation)
        {
            // I can't use LocalToWorld here, cos it's one frame behind. I need a up-to-date world matrix.
            var toWorldMatrix = TransformHelpers.ComputeWorldMatrix(this, canvas.camera);
            var cameraWorldPos = toWorldMatrix[3].xy;

            // We are assuming here that uicanvas is always a root, that's why we can assign camera's world position
            // to uicanvas' local position.
            rectTransform.anchoredPosition = cameraWorldPos;

            localRotation.Value = new quaternion(toWorldMatrix);
        }
    }
}
