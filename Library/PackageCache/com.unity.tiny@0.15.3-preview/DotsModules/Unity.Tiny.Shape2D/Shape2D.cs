using Unity.Authoring.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny;

[assembly: ModuleDescription("Unity.Tiny.Shape2D", "2D shapes")]
namespace Unity.Tiny.Core2D
{

    /// <summary>
    /// A component describing a 2d closed polygon shape.
    /// </summary>
    /// <remarks>
    /// If there are no indices, vertices are rendered as a triangle fan.
    /// Indices can be provided with <see cref="Shape2DIndex"/>
    /// Shapes are limited to 64k vertices.
    /// </remarks>
    [HideInInspector]
    public struct Shape2DVertex : IBufferElementData
    {
        public float2 position;
    }

    /// <summary>
    ///  Optional indices into the vertices of the shape.
    /// </summary>
    /// <remarks>
    ///  Every 3 indices form one triangle.
    ///  Index values must be between 0 and vertices.size.
    ///  If there are no indices, vertices are rendered as a triangle fan.
    /// </remarks>
    [HideInInspector]
    public struct Shape2DIndex : IBufferElementData
    {
        public ushort index;
    }

    /// <summary>
    ///  A component for basic 2D shape rendering. Specifies how to render
    ///  <see cref="Shape2DVertex"/> and <see cref="Shape2DIndex"/> data.
    /// </summary>
    [HideInInspector]
    public struct Shape2DRenderer : IComponentData
    {
        /// <summary>
        ///  The Entity on which to look for a <see cref="Shape2DVertex"/> component to describe the shape to render.
        ///  If null, the entity that holds the <see cref="Shape2DRenderer"/> is searched.
        /// </summary>
        public Entity shape;

        /// <summary>
        ///  A color tint to apply to the sprite image.  For normal rendering, this should be opaque
        ///  white (1, 1, 1, 1).
        /// </summary>
        public Color color;

        /// <summary>
        ///  Blend op for rendering the sprite. The default and regular mode is Alpha.
        /// </summary>
        public BlendOp blending;
    }

    internal class MakeEntryShape : IExternalDisplayListEntryMaker
    {
        public int GetRendererComponent()
        {
            return TypeManager.GetTypeIndex<Shape2DRenderer>();
        }

        public bool DoNotClip()
        {
            return false;
        }

        public void Filter(ref EntityQueryBuilder query)
        {
            query.WithAll<Shape2DRenderer>();
        }

        public bool MakeEntry(EntityManager mgr, ref DisplayListEntry de)
        {
            var shapeRenderer = mgr.GetComponentData<Shape2DRenderer>(de.e);
            if (shapeRenderer.color.a <= 0)
                return false;
            if (shapeRenderer.shape == Entity.Null) // fix up invalid ref to point to self
                shapeRenderer.shape = de.e;
#if DEVELOPMENT
            if (!man.hasBuffer<Shape2DVertex>(shapeRenderer->shape))
            {
                logWarning("Shape renderer entity %s has no valid Shape2DVertex buffer!", man.formatEntity(det.e).c_str());
                return false;
            }
#endif
            var shape = mgr.GetBuffer<Shape2DVertex>(shapeRenderer.shape);
            if (shape.Length < 3)
                return false;
#if DEVELOPMENT
            if (shape.size() < 3)
            {
                logWarning("Shape entity %s has less than three (%i) vertices!", man.formatEntity(det.e).c_str(),
                           (int)shape.size());
                return false;
            }
#endif
            float2 bbMin = shape[0].position;
            float2 bbMax = bbMin;
            for (int i = 1; i < (int)shape.Length; i++)
            {
                bbMin = math.min(bbMin, shape[i].position);
                bbMax = math.max(bbMax, shape[i].position);
            }
            de.inBounds.x = bbMin.x;
            de.inBounds.y = bbMin.y;
            de.inBounds.width = bbMax.x - bbMin.x;
            de.inBounds.height = bbMax.y - bbMin.y;
            de.type = DisplayListEntryType.Shape;
            return true;
        }

        public void Update(ComponentSystem cs) { }
    }

    /// <summary>
    /// An initialization only system required to initialize shape rendering
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(DisplayListSystem))]
    internal class Shape2DInitSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            DisplayListSystem.RegisterDisplayListEntryMaker(new MakeEntryShape());
        }

        protected override void OnDestroy()
        {
            DisplayListSystem.DeRegisterExternalDisplayListEntryMaker(TypeManager.GetTypeIndex<Shape2DRenderer>());
        }

        protected override void OnUpdate()
        {
        }
    }
}
