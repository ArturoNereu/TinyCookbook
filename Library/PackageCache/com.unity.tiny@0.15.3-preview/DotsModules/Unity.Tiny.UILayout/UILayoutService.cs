using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.UILayout
{
    public static class UILayoutService
    {
        /// <summary>
        ///  Gets the dimensions of a RectTransform component.
        ///  This method assumes that entity has both a Transform component and a
        ///  RectTransform component.
        /// </summary>
        public static float2 GetRectTransformSizeOfEntity(ComponentSystem sys, Entity entity)
        {
            if (entity == Entity.Null)
                return float2.zero;

            var mgr = sys.EntityManager;
            if (!mgr.HasComponent<RectTransform>(entity) || !mgr.HasComponent<Parent>(entity))
                return float2.zero;

            var rectTransform = mgr.GetComponentData<RectTransform>(entity);
            var Parent = mgr.GetComponentData<Parent>(entity);

            var parentSize = GetRectTransformSizeOfParent(sys, Parent.Value);
            return ComputeRectTransformSize(
                rectTransform.anchorMin, rectTransform.anchorMax, rectTransform.sizeDelta, parentSize);
        }

        /// <summary>
        ///  Returns the size of the childTransform's parent. If the childTransform
        ///  doesn't have a parent, returns the screen size.
        /// </summary>
        public static float2 GetRectTransformSizeOfParent(ComponentSystem sys, Entity parent)
        {
            // If there is no parent, return screen size.
            if (parent == Entity.Null)
                return GetScreenSize(sys);
            return GetRectTransformSizeOfEntity(sys, parent);
        }

        public static float2 GetScreenSize(ComponentSystemBase sys)
        {
            var env = sys.World.TinyEnvironment();
            DisplayInfo di = env.GetConfigData<DisplayInfo>();
            return new float2(di.width, di.height);
        }

        /// <summary>
        ///  Calculates the dimensions of a RectTransform component using the supplied
        ///  values.
        /// </summary>
        public static float2 ComputeRectTransformSize(
            float2 anchorMin, float2 anchorMax, float2 sizeDelta, float2 parentSize)
        {
            return parentSize * (anchorMax - anchorMin) + sizeDelta;
        }

#if false // XXX Jukka restore this once support for ref has been added.
        /// <summary>
        ///  Sets the RectTransform calculated rect to a given size.
        /// </summary>
        void SetRectTransformSize(ComponentSystem sys, Transform transform, ref RectTransform rectTransform, float2 size);
#endif
    }
}
