using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Tiny.Core2D
{
    /// <summary>
    ///  List of options for clearing a camera's viewport before rendering.
    ///  Used by the Camera2D component.
    /// </summary>
    public enum CameraClearFlags {

        /// <summary>
        ///  Do not clear. Use this when the camera renders to the entire screen,
        ///  and in situations where multiple cameras render to the same screen area.
        /// </summary>
        Nothing,

        /// <summary>
        ///  Clears the viewport with a solid background color.
        /// </summary>
        SolidColor
    }

    /// <summary>
    ///  List of options for interpreting a camera's culling mask. These are used
    ///  by the Camera2D component.
    /// </summary>
    public enum CameraCullingMode {

        /// <summary>
        ///  Renders every renderable entity. Nothing is culled.
        /// </summary>
        NoCulling,

        /// <summary>
        ///  Renders only entities that have all of their components in cullingMask.
        /// </summary>
        All,

        /// <summary>
        ///  Renders only entities that have at least one of their components in cullingMask.
        ///  This is similar to how Unity's cullingMask works.
        /// </summary>
        Any,

        /// <summary>
        ///  Renders all entities except for those that have the components in cullingMask set.
        /// </summary>
        None
    }

    public struct Camera2D : IComponentData
    {
        /// <summary>
        /// Default Camera2D preset.
        /// </summary>
        public static Camera2D Default { get; } = new Camera2D()
        {
            halfVerticalSize = 5f,
            backgroundColor = new Color(49f / 255f, 77f / 255f, 121f / 255f, 1f), // Unity blue
            rect = new Rect(0f, 0f, 1f, 1f),
            depth = 0f,
            cullingMode = CameraCullingMode.NoCulling,
            clearFlags = CameraClearFlags.SolidColor
        };
        
        /// <summary>
        ///  Specifies half of the vertical size in world units.
        /// </summary>
        /// <remarks>The horizontal
        ///  size is calculated from the output display size and aspect ratio.
        ///  The Transform component's world position defines the camera origin's
        ///  location, size, and rotation.
        /// </remarks>
        public float halfVerticalSize;

        /// <summary>
        /// Specifies the coordinates of the viewport rectangle in the
        /// output frame that this camera should render to.
        /// </summary>
        public Rect rect;

        /// <summary>
        ///  Specifies the background color this camera should render before rendering
        ///  the scene when clearFlags is set to SolidColor.
        /// </summary>
        public Color backgroundColor;

        /// <summary>
        ///  Specifies how to clear this camera's viewport before rendering.
        /// </summary>
        public CameraClearFlags clearFlags;

        /// <summary>
        ///  The camera's render order relative to other cameras.
        /// </summary>
        /// <remarks>
        /// Cameras with lower values render first. Cameras with higher values
        /// render on top of cameras with lower values.
        /// </remarks>
        public float depth;

        /// <summary> Specifies which CameraCullingMode option this camera uses to
        ///  interpret its culling mask.
        /// </summary>
        /// <remarks>
        ///  The culling mask is specified via the cullingMask field directly
        /// </remarks>
        public CameraCullingMode cullingMode;

        /// <summary> Specifies which components to consider when culling
        /// entities according to the cullingMode.
        /// </summary>
        /// <remarks>
        /// The cullingMask is a bit mask that references the 16 CameraLayer0 to CameraLayer15 components.
        /// An easy way to set the bit mask is using binary literals 0b0000_0000_0000_0000_0000_0000_0000_0000 and setting desired entries to 1.
        /// 0b1000_0000_1000_0010 for example would mean the set of CameraLayer1, CameraLayer7, and CameraLayer15.
        /// The upper 16 bits of the cullingMask can be used for custom components. Those components have to be registered with the
        /// DisplayListSystem via SetCameraLayerComponent.
        /// Examples:
        /// * cullingMode = CameraCullingMode.NoCulling -> cullingMask is ignored, camera renders all entities
        /// * cullingMode = CameraCullingMode.All, cullingMask = 0b001001 -> only entities that have at least both the CameraLayer0 and CameraLayer3 component layer are rendered by this camera
        /// * cullingMode = CameraCullingMode.Any, cullingMask = 0b000101 -> only entities that have at least one of CameraLayer0 and CameraLayer2 component layer are rendered by this camera
        /// * cullingMode = CameraCullingMode.None, cullingMask = 0b010001 -> render all entities except those that have either CameraLayer0 or CameraLayer4 components set
        /// </remarks>
        public uint cullingMask; 
    }

    public struct CameraLayer0 : IComponentData { }
    public struct CameraLayer1 : IComponentData { }
    public struct CameraLayer2 : IComponentData { }
    public struct CameraLayer3 : IComponentData { }
    public struct CameraLayer4 : IComponentData { }
    public struct CameraLayer5 : IComponentData { }
    public struct CameraLayer6 : IComponentData { }
    public struct CameraLayer7 : IComponentData { }
    public struct CameraLayer8 : IComponentData { }
    public struct CameraLayer9 : IComponentData { }
    public struct CameraLayer10 : IComponentData { }
    public struct CameraLayer11 : IComponentData { }
    public struct CameraLayer12 : IComponentData { }
    public struct CameraLayer13 : IComponentData { }
    public struct CameraLayer14 : IComponentData { }
    public struct CameraLayer15 : IComponentData { }

    /// <summary>
    /// Add this component to the same entity as Camera2D to render to a texture instead of to the screen.
    /// </summary>
    public struct Camera2DRenderToTexture : IComponentData
    {
        public static Camera2DRenderToTexture Default { get; } = new Camera2DRenderToTexture()
        {
            width = 1,
            height = 1
        };

        /// <summary> Width of the target render texture. Must be a power of two. </summary>
        public int width;

        /// <summary> Height of the target render texture. Must be a power of two. </summary>
        public int height;

        /// <summary>
        ///  Freeze render to texture operation. Setting this flag to true skips re-rendering the texture.
        ///  It is a lightweight way to temporarily disable render to texture for this camera without de-allocations.
        /// </summary>
        public bool freeze;

        /// <summary>
        ///  The target entity to render to. If NONE, this entity is used.
        ///  The target entity must have an Image2D and an Image2DRenderToTexture component.
        /// </summary>
        public Entity target;
    }

    /// <summary>
    ///  Add this component to an entity with a Camera2D component to change the
    ///  default sorting axis.
    /// </summary>
    public struct Camera2DAxisSort : IComponentData
    {
        public static Camera2DAxisSort Default { get; } = new Camera2DAxisSort()
        {
            axis = new float3(0f, 0f, 1f)
        };
        
        /// <summary>
        ///  A sorting axis selector that uses the dot product of the camera-space
        ///  position and this vector as a sorting value.
        /// </summary>
        /// <remarks>
        ///  The default z axis sorting uses (0,0,1).
        ///  For negative y sorting, required for some 2D setups, use (0,-1,0).
        ///  Isometric perspectives may require unusual combinations such as (1,1,0).
        /// </remarks>
        public float3 axis;
    }

    /// <summary>
    ///  Add this component to an entity with a Camera2D component to specify the
    ///  distances from the camera to its near and far clipping planes. The camera
    ///  draws elements between the clipping planes and ignores elements outside of them.
    /// </summary>
    public struct Camera2DClippingPlanes : IComponentData
    {
        public static Camera2DClippingPlanes Default { get; } = new Camera2DClippingPlanes()
        {
            near = 0f,
            far = 1000f
        };
        
        /// <summary>
        ///  The distance to the near clipping plane. The camera does not draw anything
        ///  closer than this distance.
        /// </summary>
        public float near;

        /// <summary>
        ///  The distance to the far clipping plane. The camera does not draw anything
        ///  beyond this distance.
        /// </summary>
        public float far;
    }
}
