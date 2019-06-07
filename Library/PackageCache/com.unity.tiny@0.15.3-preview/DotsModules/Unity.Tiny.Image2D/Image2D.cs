using System;
using Unity.Authoring.Core;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;

namespace Unity.Tiny.Core2D
{
    public enum ImageStatus
    {
        Invalid,
        Loaded,
        Loading,
        LoadError
    }

    public enum Image2DSaveStatus
    {
        Invalid,
        Written,
        Writing,
        WriteErrorBadInput,
        WriteErrorUnsuportedFormat,
        WriteError
    }

    public enum Image2DMemoryFormat
    {
        RGBA8Premultiplied,
        RGBA8,
        A8
    }

    /// <summary>
    /// Initialize an image from an asset file.
    /// </summary>
    /// <remarks>
    /// Once loading has completed, the asset loading system will remove this component from the image.
    /// Inspect the Image2D component status field for loading results.
    ///
    /// You need to provide an <see cref="Image2DLoadFromFileImageFile"/> and/or <see cref="Image2DLoadFromFileMaskFile"/>
    /// next to this component to specify the file to load.
    /// </remarks>
    public struct Image2DLoadFromFile : IComponentData
    {
        public int dummy;
    }

    /// <summary>
    /// The image file/URI to load.
    /// This can be a data URI.
    /// </summary>
    public struct Image2DLoadFromFileImageFile : IBufferElementData
    {
        public char s;
    }

    /// <summary>
    /// An image to use as the mask. This can be a data URI.
    /// </summary>
    /// <remarks>
    /// The red channel will be used as
    /// the mask (the alpha channel is ignored);
    /// efficient compression can be used (e.g. a single channel PNG or palleted PNG8).
    /// </remarks>
    public struct Image2DLoadFromFileMaskFile : IBufferElementData
    {
        public char s;
    }

#if false
    /// <summary>
    /// Initialize an image from a memory buffer
    /// Once loading has completed, the asset loading system will remove this component from the image
    /// </summary>
    public struct Image2DLoadFromMemory : IComponentData
    {
        /// <summary>
        /// Width of image in memory buffer
        /// </summary>
        public int width;

        /// <summary>
        /// Height of image in memory buffer
        /// </summary>
        public int height;

        /// <summary>
        /// Pixel data, packed. Use format to specify what format the pixel data is in.
        /// </summary>
        public DynamicArray<byte> pixelData;

        /// <summary>
        /// Format of the pixel data.
        /// The most efficient format may vary by platform.
        /// </summary>
        public Image2DMemoryFormat format;
    }

    /// <summary>
    /// Store the image to a .png file on disk (native), or to POST it to a server (web)
    /// This can be useful to create screen shots or caches when combined with Image2DRenderToTexture
    /// After writing is done (asynchronously) an Image2DSaveToFileResult component is added with a result.
    /// </summary>
    public struct Image2DSaveToFile : IComponentData
    {
        /// <summary>
        /// The image file to write to.
        /// The applications must have write access.
        /// In HTML mode, this will POST a png file to the given
        /// target file. For native code, this will write a file to disk.
        /// Image files are always written as png files.
        /// </summary>
        public string imageFile;
    }

    /// <summary>
    /// Result of an asynchronous Image2DSaveToFile operation.
    /// This component should not be added directly, it will be added by a system when and Image2DSaveToFile
    /// operation completed.
    /// </summary>
    public struct Image2DSaveToFileResult : IComponentData
    {
        /// <summary>
        /// The result of the write operation. On success it is Written
        /// </summary>
        public /*readonly*/ Image2DSaveStatus status;

        /// <summary>
        /// The image file target that was written to.
        /// The same name as passed in via Image2DSaveToFile.
        /// </summary>
        public /*readonly*/ string imageFile;
    }

    /// <summary>
    /// Store the image to a .png file data uri
    /// This can be useful to create screen shots or caches when combined with Image2DRenderToTexture
    /// After writing is done (asynchronously) the Image2DSaveToDataURI component is removed and
    /// an Image2DSaveToDataURIResult component is added with a result.
    /// Image files are always encoded as png files.
    /// </summary>
    public struct Image2DSaveToDataURI : IComponentData
    {
    }

    /// <summary>
    /// Result of an asynchronous Image2DSaveToDataURI operation.
    /// This component should not be added directly, it will be added by a system when an Image2DSaveToDataURI
    /// operation completed.
    /// </summary>
    public struct Image2DSaveToDataURIResult : IComponentData
    {
        /// <summary>
        /// The result data URI, one very large string.
        /// When the URI is ready, a Image2DSaveResult will be added to the entity.
        /// </summary>
        public /*readonly*/ string dataURI;

        /// <summary>
        /// The result of the write operation. On success it is Written
        /// </summary>
        public /*readonly*/ Image2DSaveStatus status;
    }

    /// <summary>
    /// Store the image as raw memory
    /// This can be useful to roundtrip for CPU modification of images by re-uploading
    /// via Image2DLoadFromMemory
    /// After writing is done (asynchronously) the Image2DSaveToMemory component is removed and
    /// an Image2DSaveToMemory component is added with a result.
    /// </summary>
    public struct Image2DSaveToMemory : IComponentData
    {
        /// <summary>
        /// The format of the image pixels.
        /// </summary>
        public Image2DMemoryFormat format;
    }

    /// <summary>
    /// Result of an asynchronous Image2DSaveToMemory operation.
    /// This component should not be added directly, it will be added by a system when an Image2DSaveToMemory
    /// operation completed.
    /// </summary>
    public struct Image2DSaveToMemoryResult : IComponentData
    {
        /// <summary>
        /// The result pixel data in format RGBA8Premultiplied
        /// </summary>
        public /*readonly*/ DynamicArray<byte> pixelData;

        /// <summary>
        /// The width of the image in pixelData
        /// </summary>
        public /*readonly*/ int width;

        /// <summary>
        /// The height of the image in pixelData
        /// </summary>
        public /*readonly*/ int height;

        /// <summary>
        /// The format of the image in pixelData, the same value that was passed
        /// in via Image2DSaveToMemory.format
        /// </summary>
        public /*readonly*/ Image2DMemoryFormat format;

        /// <summary>
        /// The result of the write operation. On success it is set to Written.
        /// </summary>
        public /*readonly*/ Image2DSaveStatus status;
    }
#endif

    /// <summary>
    /// Tag component that needs to be next to be placed next to an <see cref="Image2D"/> component
    /// if it is intended to be used as a render to texture target.
    /// </summary>
    public struct Image2DRenderToTexture : IComponentData
    {
    }

    public struct Image2D : IComponentData
    {
        /// <summary>
        /// Disable image bilinear filtering.
        /// This is useful for pixel art assets.
        /// Defaults to false.
        /// </summary>
        public bool disableSmoothing;

        /// <summary>
        /// Image size in pixels.
        /// Set only after loading (status must be ImageStatus::Loaded).
        /// This is written to by the image loading system and should be treated as read only by user code.
        /// </summary>
        public float2 imagePixelSize;

        /// <summary>
        /// Image contains any alpha values != 1.
        /// Set only after loading (status must be ImageStatus::Loaded).
        /// This is written to by the image loading system and should be treated as read only by user code.
        /// </summary>
        public bool hasAlpha;

        /// <summary>
        /// Load status of the image.
        /// This is written to by the image loading system and should be treated as read only by user code.
        /// </summary>
        public ImageStatus status;
    }

    /// <summary>
    /// A component that keeps a read only alpha mask of an <see cref="Image2D"/> for use in hit
    /// testing, when pixelAccurate hit testing is enabled.
    /// </summary>
    /// <remarks>
    /// Add the Image2DAlphaMask component next to
    /// an Image2D component before loading the image.
    /// The result is written to a Image2DAlphaMaskData component that is placed next to the Image2DAlphaMask.
    /// </remarks>
    public struct Image2DAlphaMask : IComponentData
    {
        public static Image2DAlphaMask Default { get; } = new Image2DAlphaMask
        {
            threshold = 0.5f
        };

        /// <summary>
        /// Threshold value for when a bit is set or not, depending on the alpha value of the adjacent Image2D.
        /// Default value is .5.
        /// </summary>
        public float threshold;
    }

    [HideInInspector]
    public struct Image2DAlphaMaskData : IBufferElementData
    {
        public byte c;
    }

    
    /// <summary>
    /// A system that makes sure that Image2DAlphaMaskData is added to images that have the Image2DAlphaMask 
    /// component. Any image loading systems should be scheduled after this system.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class Image2DMaskInit : ComponentSystem
    {
        protected override void OnUpdate()
        {
            UpdateMasks();
        }

        private void UpdateMasks() {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities.WithAll<Image2DAlphaMask>().WithNone<Image2DAlphaMaskData>().ForEach((Entity e) => {
                    ecb.AddBuffer<Image2DAlphaMaskData>(e);
            });
            Entities.WithAll<Image2DAlphaMaskData>().WithNone<Image2DAlphaMask>().ForEach((Entity e) => {
                    ecb.RemoveComponent<Image2DAlphaMaskData>(e);
            });
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
