using Unity.Authoring.Core;
using Unity.Entities;

namespace Unity.Tiny.Core2D
{
    public enum DisplayOrientation
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    ///  The rendering mode for <see cref="DisplayInfo"/>.
    /// </summary>
    public enum RenderMode
    {
        /// <summary>
        ///  Selects a rendering mode automatically based on available modules and device support.
        /// </summary>
        Auto,

        /// <summary>
        ///  Forces DisplayInfo to use HTML5 canvas rendering.
        /// </summary>
        Canvas,

        /// <summary>
        ///  Forces DisplayInfo to use WebGL rendering.
        /// </summary>
        WebGL
    }

    /// <summary>
    ///  Configures display-related parameters. You can access this component via
    ///  TinyEnvironment.Get/SetConfigData&lt;DisplayInfo&gt;()
    /// </summary>
    [HideInInspector]
    public struct DisplayInfo : IComponentData
    {
        public static DisplayInfo Default { get; } = new DisplayInfo
        {
            width = 1280,
            height = 720,
            autoSizeToFrame = true
        };

        /// <summary>
        /// Specifies the output width, in logical pixels. Writing will resize the window, where supported.
        /// </summary>
        public int width;

        /// <summary>
        /// Specifies the output height, in logical pixels.
        /// Writing will resize the window, where supported.
        /// </summary>
        public int height;

        /// <summary>
        /// Specifies the output height, in physical pixels.
        /// Read-only, but it can be useful for shaders or texture allocations.
        /// </summary>
        public int framebufferHeight;

        /// <summary>
        /// Specifies the output width, in physical pixels.
        /// Read-only, but it can be useful for shaders or texture allocations.
        /// </summary>
        public int framebufferWidth;

        /// <summary>
        ///  If set to true, the output automatically resizes to fill the frame
        ///  (the browser or application window), and match the orientation.
        ///  Changing output width or height manually has no effect.
        /// </summary>
        public bool autoSizeToFrame;

        /// <summary>
        ///  Specifies the frame width, in pixels. This is the width of the browser
        ///  or application window.
        /// </summary>
        public int frameWidth;

        /// <summary>
        ///  Specifies the frame height, in pixels. This is the height of the browser
        ///  or application window.
        /// </summary>
        public int frameHeight;

        /// <summary>
        ///  Specifies the device display (screen) width, in pixels.
        /// </summary>
        public int screenWidth;

        /// <summary>
        ///  Specifies the device display (screen) height, in pixels.
        /// </summary>
        public int screenHeight;

        /// <summary>
        ///  Specifies the scale of the device display (screen) DPI relative to.
        ///  96 DPI. For example, a value of 2.0 yields 192 DPI (200% of 96).
        /// </summary>
        public float screenDpiScale;

        /// <summary>
        ///  Specifies the device display (screen) orientation. Can be Horizontal
        ///  or Vertical.
        /// </summary>
        public DisplayOrientation orientation;

        /// <summary>
        ///  Forces DisplayInfo to use a specific renderer.
        /// </summary>
        /// <remarks>
        ///  This allows switching between WebGL and Canvas rendering in the HTML runtime.
        ///  The RenderMode enum defines possible renderers. The default is Auto.
        ///  Switching renderers at runtime is usually possible, but may not be seamless.
        /// </remarks>
        public RenderMode renderMode;

        /// <summary>
        ///  Specifies whether the browser or application window has focus.
        ///  Read only; setting this value has no effect.
        /// </summary>
        public bool focused;

        /// <summary>
        ///  Specifies whether the browser or application window is currently visible
        ///  on the screen/device display.
        ///  Read only; setting this value has no effect.
        /// </summary>
        public bool visible;
    }
}
