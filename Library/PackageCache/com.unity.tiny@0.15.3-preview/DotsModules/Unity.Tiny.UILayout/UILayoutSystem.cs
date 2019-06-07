using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.UILayout
{
    /// <summary>
    ///  Lays out elements with both a RectTransform and Transform component by
    ///  setting their Transform.localPosition. The local position depends on the
    ///  RectTransform component.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(UICanvasSystem))]
    public class UILayoutSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity e, ref Parent node, ref RectTransform rt, ref Translation localPos) =>
            {
                var localPosition = GetLocalPosition(e, ref rt, ref node);
                localPos.Value = new float3(localPosition.x, localPosition.y, 0.0f);
            });
        }

        // Calculate anchor's reference point.
        private float2 GetAnchorReferencePoint(
            float2 anchorMin, float2 anchorMax, float2 pivotPoint, float2 parentSize)
        {
            return parentSize * (anchorMin + (anchorMax - anchorMin) * pivotPoint) - parentSize / 2.0f;
        }

        // Calculate the pivot point in world space units.
        private float2 GetPivotRealUnits(float2 normalizedPivot, float2 size)
        {
            return new float2(size.x * (normalizedPivot.x - 0.5f), size.y * (normalizedPivot.y - 0.5f));
        }

        /**
         * Modify the position based on pivot point, rotation and scale.
         * Rotation and Scale are handled in the Transform component, but pivot point is a part of RectTransform. If pivot point
         * is not in the center of RectTransform and Rotation and/or Scale is applied, then we have to change the position
         * manually, and this is exactly what this function does.
         */
        private float2 TransformPosition(
            float2 position, float2 pivotRealUnits, quaternion localRotation, float3 localScale)
        {
            float3 positionOffset = new float3(pivotRealUnits.x, pivotRealUnits.y, 0);

            // Apply scale
            positionOffset = positionOffset * localScale;

            // Apply rotation
            positionOffset = math.rotate(localRotation, positionOffset);

            // Go back
            positionOffset -= new float3(pivotRealUnits.x, pivotRealUnits.y, 0);

            return new float2(position.x - positionOffset.x, position.y - positionOffset.y);
        }

        /**
         * Calculate correct Transform.localPosition based on RectTransform component.
         */
        private float2 GetLocalPosition(
            Entity e, ref RectTransform rectTransform, ref Parent parent)
        {
            var parentSize = UILayoutService.GetRectTransformSizeOfParent(this, parent.Value);
            var size = UILayoutService.ComputeRectTransformSize(rectTransform.anchorMin, rectTransform.anchorMax,
                                                                  rectTransform.sizeDelta, parentSize);
            var anchorReferencePoint =
                GetAnchorReferencePoint(rectTransform.anchorMin, rectTransform.anchorMax, rectTransform.pivot, parentSize);
            var pivotRealUnits = GetPivotRealUnits(rectTransform.pivot, size);
            var position = anchorReferencePoint + rectTransform.anchoredPosition - pivotRealUnits;

            float3 scale;
            if (EntityManager.HasComponent<NonUniformScale>(e))
                scale = EntityManager.GetComponentData<NonUniformScale>(e).Value;
            else
                scale = new float3(1, 1, 1);

            quaternion rotation;
            if (EntityManager.HasComponent<Rotation>(e))
                rotation = EntityManager.GetComponentData<Rotation>(e).Value;
            else
                rotation = quaternion.identity;

            position = TransformPosition(position, pivotRealUnits, rotation, scale);

            return position;
        }
    }
}
