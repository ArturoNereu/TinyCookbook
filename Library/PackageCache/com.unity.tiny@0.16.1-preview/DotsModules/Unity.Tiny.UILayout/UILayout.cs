using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Tiny.Core2D;

/// <summary>
///  UILayout module, used for anchor-based layouting of UI elements.
///  @module
///  @name Unity.Tiny
/// </summary>
namespace Unity.Tiny.UILayout
{
    /// <summary>
    ///  Defines the position, size, anchor and pivot information for a rectangle.
    /// </summary>
    public struct RectTransform : IComponentData
    {
        /// <summary>
        ///  The normalized position in the parent RectTransform that the lower
        ///  left corner of the rectangle is anchored to.
        /// </summary>
        public float2 anchorMin;

        /// <summary>
        ///  The normalized position in the parent RectTransform that the upper
        ///  right corner of the rectangle is anchored to.
        /// </summary>
        public float2 anchorMax;

        /// <summary>
        ///  The size of this RectTransform relative to the distances between the
        ///  anchors.
        ///  If the anchors are together, sizeDelta is the same as size. If the
        ///  anchors are in each of the four corners of the parent, the sizeDelta
        ///  is how much bigger or smaller the rectangle is compared to its parent.
        /// </summary>
        public float2 sizeDelta;

        /// <summary>
        ///  The position of this RectTransform's pivot of relative to the anchor
        ///  reference point.
        /// </summary>
        public float2 anchoredPosition;

        /// <summary>
        ///  The normalized position in this RectTransform that it rotates around.
        /// </summary>
        public float2 pivot;

        public float2 offsetMin
        {
            get => anchoredPosition - sizeDelta * pivot;
            set
            {
                var a = value - (anchoredPosition - sizeDelta * pivot);
                sizeDelta -= a;
                anchoredPosition += a * ( new float2(1f) - pivot);
            }
        }

        public float2 offsetMax
        {
            get => anchoredPosition + sizeDelta * (new float2(1f) - pivot);
            set
            {
                var a = value - (anchoredPosition + sizeDelta * (new float2(1f) - this.pivot));
                sizeDelta += a;
                anchoredPosition += a *pivot;
            }
        }

        public static RectTransform Default { get; } = new RectTransform
        {
            anchorMin = new float2(0.5f),
            anchorMax = new float2(0.5f),
            pivot = new float2(0.5f),
            sizeDelta = new float2(100f),
            anchoredPosition = float2.zero,
        };
}

    /// <summary>
    ///  Determines how UI elements in the Canvas are scaled.
    /// </summary>
    public enum UIScaleMode
    {
        /// <summary>
        ///  In Constant Pixel Size mode, you specify UI element positions and sizes
        ///  in pixels on the screen.
        ///
        ///  In practical terms, this means that  elements retain the same size,
        ///  in pixels, regardless of screen size.
        /// </summary>
        ConstantPixelSize,

        /// <summary>
        ///  In Scale With Screen Size mode, the Canvas has a specified reference
        ///  resolution and you specify UI element positions and sizes in pixels
        ///  within that resolution.
        ///
        ///  If the screen resolution is different from the the Canvas resolution,
        ///  the Canvas scales up or down, as needed, to fit the screen.
        ///
        ///  In practical terms, this means that UI elements get bigger as the screen
        ///  gets bigger, and vice versa.
        /// </summary>
        ScaleWithScreenSize
    }

    /// <summary>
    ///  Root component for UI elements (entities with RectTransform and Transform components).
    /// </summary>
    public struct UICanvas : IComponentData
    {
        /// <summary>
        ///  The entity with the Camera2D component used for layouting calculations.
        /// </summary>
        [EntityWithComponents(typeof(Camera2D))]
        public Entity camera;

        /// <summary>
        ///  How UI elements in the Canvas are scaled.
        /// </summary>
        public UIScaleMode uiScaleMode;

        /// <summary>
        ///  The resolution the UI layout is designed for (in pixels).
        /// </summary>
        public float2 referenceResolution;

        /// <summary>
        ///  Scales the Canvas to match the width or height of the reference
        ///  resolution, or a combination of width and height.
        ///
        ///  A value of 0 scales the Canvas according to the difference between the
        ///  current screen resolution width and the reference resolution width.
        ///
        ///  A value of 1 scales Canvas according to the difference between the
        ///  current screen resolution height and the reference resolution height.
        ///
        ///  Values  between 0 and 1 scale the Canvas based on a combination of
        ///  the relative width and height.
        /// </summary>
        public float matchWidthOrHeight;

        public static UICanvas Default { get; } = new UICanvas
        {
            camera = Entity.Null,
            uiScaleMode = UIScaleMode.ScaleWithScreenSize,
            referenceResolution = new float2(1280, 720),
            matchWidthOrHeight = 1
        };
    }
}
