using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Tiny.Core2D
{
    /// <summary>
    ///  Drawing mode used by <see cref="Sprite2DRendererOptions"/>
    ///  When a sprite size is manually set, the drawing mode specifies how the
    ///  sprite fills the area.
    /// </summary>
    [IdAlias("a7ae2ef1f120627af99607cbecca330e")]
    public enum DrawMode
    {
        /// <summary>
        ///  Tiles the sprite continuously if the area is larger than the source sprite, or cuts it off it is smaller.
        /// </summary>
        ContinuousTiling,

        /// <summary>
        ///  Adaptively tiles the sprite. When the target area is smaller, the sprite is scaled down, like in Stretch mode.
        ///  If the area is larger a combination of scaling and tiling is used, that minimizes scaling but always tiles complete tiles.
        /// </summary>
        AdaptiveTiling,

        /// <summary>
        ///  Scale the sprite to fill the new area.
        /// </summary>
        Stretch
    }

    /// <summary>
    ///  A component for basic 2D sprite rendering. Specifies an <see cref="Sprite2D"/> to draw and rendering
    ///  modifiers, such as a color tint.
    /// </summary>
    [IdAlias("1c504a8a7963db0d32e89b527b90e057")]
    public struct Sprite2DRenderer : IComponentData
    {
        public static Sprite2DRenderer Default { get; } = new Sprite2DRenderer()
        {
            color = Color.Default
        };

        /// <summary>
        ///  The Entity on which to look for a <see cref="Sprite2D"/> component to describe the sprite to render.
        ///  If null, the <see cref="Sprite2D"/> is looked for on the same entity as the <see cref="Sprite2DRenderer"/> .
        /// </summary>
        [EntityWithComponents(typeof(Sprite2D))]
        public Entity sprite;

        /// <summary>
        /// A color tint to apply to the sprite image.  For normal rendering, this should be opaque
        /// white (1, 1, 1, 1).
        /// </summary>
        public Color color;

        /// <summary>
        ///  Blend op for rendering the sprite. The default and regular mode is Alpha.
        /// </summary>
        public BlendOp blending;
    }

    /// <summary>
    ///  A modifier component, when added alongside a Sprite2DRenderer it overrides the
    ///  world space size computation.
    /// </summary>
    /// <remarks>
    ///  Regular sprites have a base (untransformed) size that is computed as
    ///  image asset size times sprite pixelsToWorldUnits.
    ///  When this component is added to an entity the base size is set explicitly,
    ///  and the image is placed inside that region depending on the repeat mode.
    /// </remarks>
    [IdAlias("be2f221c9af74ec5c0a4495a2d6769a3")]
    public struct Sprite2DRendererOptions : IComponentData
    {
        /// <summary>
        ///  Sprite size in world units
        /// </summary>
        /// <remarks>
        ///  This is used to override the computed natural sprite size.
        ///  The natural size is Image2D.imagePixelSize * Sprite2D.pixelsToWorldUnits * Sprite2D.imageRegion.size.
        ///  The new size is also used in hit testing.
        /// </remarks>
        public float2 size;

        /// <summary>
        ///  Draw mode, defaults to ContinuousTiling.
        ///  This mode specifies how the natural sized sprite is mapped into the new size.
        /// </summary>
        public DrawMode drawMode;

        public static Sprite2DRendererOptions Default { get; } = new Sprite2DRendererOptions
        {
            size = new float2(1f),
            drawMode = DrawMode.Stretch
        };
    }

    /// <summary>
    ///  Modifier component. Add alongside a Sprite2D to add a border.
    ///  The border is used for sliced tiling modes
    /// </summary>
    public struct Sprite2DBorder : IComponentData
    {
        public static Sprite2DBorder Default { get; } = new Sprite2DBorder
        {
            topRight = new float2(1f, 1f)
        };

        /// <summary>
        ///  Bottom left slice inset point, normalized [0..1] to sprite (not image)
        ///  Defaults to (0,0) for no border.
        /// </summary>
        public float2 bottomLeft;

        /// <summary>
        ///  Top right slice inset point, normalized [0..1] to sprite (not image)
        ///  Defaults to (1,1) for no border.
        /// </summary>
        public float2 topRight;
    }
}
