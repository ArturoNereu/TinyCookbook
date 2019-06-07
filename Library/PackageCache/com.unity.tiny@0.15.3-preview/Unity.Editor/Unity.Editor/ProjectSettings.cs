using System;

namespace Unity.Editor
{
    /// <summary>
    /// Stores all of the information for a Project.
    /// </summary>
    public class ProjectSettings
    {
        /// <summary>
        /// Returns the default values for Project Settings.
        /// </summary>
        public static ProjectSettings Default => new ProjectSettings
        {
            DefaultTextureSettings = TextureSettings.Default,
            DotNetSettings = DotNetSettings.Default,
            WebSettings = WebSettings.Default
        };

        public Guid Configuration;
        public Guid MainAsmdef;

        /// <summary>
        /// Texture settings to use by default when a texture does not override them.
        /// </summary>
        public TextureSettings DefaultTextureSettings = new TextureSettings();

        /// <summary>
        /// Platform specific settings for .Net.
        /// </summary>
        public DotNetSettings DotNetSettings = new DotNetSettings();

        /// <summary>
        /// Web-platform specific settings.
        /// </summary>
        public WebSettings WebSettings = new WebSettings();
    }

    /// <summary>
    /// Stores platform specific settings for .Net.
    /// </summary>
    public class DotNetSettings
    {
        /// <summary>
        /// Returns the default .Net platform settings.
        /// </summary>
        public static DotNetSettings Default => new DotNetSettings
        {
        };
    }

    /// <summary>
    /// Stores web-platform specific settings.
    /// </summary>
    public class WebSettings
    {
        /// <summary>
        /// Returns the default settings for the Web platform.
        /// </summary>
        public static WebSettings Default => new WebSettings
        {
            MemorySizeInMB = 32,
            SingleFileOutput = false
        };

        /// <summary>
        /// The amount of memory allocated for the program, in MB. This value can range from 16 to 2032, and must be a multiple of 16.
        /// </summary>
        public uint MemorySizeInMB;

        /// <summary>
        /// If true, all build output files are combined into a single file.
        /// </summary>
        public bool SingleFileOutput;

        internal static uint ClampValueToMultipleOf16(uint value)
        {
            const int multiple = 16;
            const int max = 2048 - 16;

            // Clamp between multiple and max
            value = Math.Min(Math.Max(value, multiple), max);

            // Round up to multiple
            return (uint)(value + 0xF & -0x10);
        }
    }

    /// <summary>
    /// Lists the possible export formats for textures.
    /// </summary>
    public enum TextureFormatType
    {
        Source,
        PNG,
        JPG,
        WebP
    }

    /// <summary>
    /// Stores the texture settings used for export.
    /// </summary>
    public class TextureSettings
    {
        /// <summary>
        /// Returns the default texture settings.
        /// </summary>
        public static TextureSettings Default => new TextureSettings
        {
            FormatType = TextureFormatType.PNG,
            JpgCompressionQuality = 100,
            WebPCompressionQuality = 100
        };

        /// <summary>
        /// The texture format.
        /// </summary>
        public TextureFormatType FormatType;

        /// <summary>
        /// The image quality setting for JPEG compression. This value can range from 1 to 100.
        /// </summary>
        public uint JpgCompressionQuality;

        /// <summary>
        /// The image quality setting for WebP compression. This value can range from 1 to 100.
        /// </summary>
        public uint WebPCompressionQuality;
    }
}
