using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Authoring.Hashing;
using Unity.Editor.Bridge;
using Unity.Editor.Extensions;
using Unity.Editor.Tools;
using Unity.Entities;
using Unity.Tiny.Core2D;

namespace Unity.Editor.Assets
{
    [EntityWithComponentsBinding(typeof(Image2D))]
    internal class Texture2DAsset : UnityObjectAsset<UnityEngine.Texture2D>
    {
        public override AssetInfo GetAssetInfo(IAssetEnumerator ctx, UnityEngine.Texture2D texture)
        {
            var atlas = texture.GetAtlas();
            if (atlas != null)
            {
                return new AssetInfo(texture, atlas.name, ctx.GetAssetInfo(atlas));
            }
            else
            {
                return new AssetInfo(texture, texture.name);
            }
        }

        public static TextureSettings GetSettings(UnityEngine.Texture2D texture)
        {
            // TODO: have per texture settings
            return Application.AuthoringProject.Settings.DefaultTextureSettings;
        }

        internal static bool HasColor(UnityEngine.Texture2D texture)
        {
            return texture.format != UnityEngine.TextureFormat.Alpha8;
        }

        internal static bool HasAlpha(UnityEngine.Texture2D texture)
        {
            if (!HasAlpha(texture.format))
            {
                return false;
            }

            if (texture.format == UnityEngine.TextureFormat.ARGB4444 ||
                texture.format == UnityEngine.TextureFormat.ARGB32 ||
                texture.format == UnityEngine.TextureFormat.RGBA32)
            {
                var tmp = BlitTexture(texture, UnityEngine.TextureFormat.ARGB32);
                UnityEngine.Color32[] pix = tmp.GetPixels32();
                for (int i = 0; i < pix.Length; ++i)
                {
                    if (pix[i].a != byte.MaxValue)
                    {
                        return true;
                    }
                }

                // image has alpha channel, but every alpha value is opaque
                return false;
            }

            return true;
        }

        private static bool HasAlpha(UnityEngine.TextureFormat format)
        {
            return format == UnityEngine.TextureFormat.Alpha8 ||
                   format == UnityEngine.TextureFormat.ARGB4444 ||
                   format == UnityEngine.TextureFormat.ARGB32 ||
                   format == UnityEngine.TextureFormat.RGBA32 ||
                   format == UnityEngine.TextureFormat.DXT5 ||
                   format == UnityEngine.TextureFormat.PVRTC_RGBA2 ||
                   format == UnityEngine.TextureFormat.PVRTC_RGBA4 ||
                   format == UnityEngine.TextureFormat.ETC2_RGBA8;
        }

        internal static TextureFormatType RealFormatType(UnityEngine.Texture2D texture, TextureSettings settings)
        {
            var format = settings.FormatType;
            if (format != TextureFormatType.Source)
            {
                return format;
            }

            // If the texture doesn't exist in asset database, we can't use "Source" format type, default to PNG.
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(assetPath))
            {
                return TextureFormatType.PNG;
            }

            // If the main asset loaded from the texture asset path is not the texture, default to PNG.
            var mainAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (mainAsset != texture)
            {
                return TextureFormatType.PNG;
            }

            return format;
        }

        internal static UnityEngine.Texture2D BlitTexture(UnityEngine.Texture2D texture, UnityEngine.TextureFormat format, bool alphaOnly = false)
        {
            // Create a temporary RenderTexture of the same size as the texture
            var tmp = UnityEngine.RenderTexture.GetTemporary(
                texture.width,
                texture.height,
                0,
                UnityEngine.RenderTextureFormat.Default,
                UnityEngine.RenderTextureReadWrite.sRGB);

            // Blit the pixels on texture to the RenderTexture
            UnityEngine.Graphics.Blit(texture, tmp);

            // Backup the currently set RenderTexture
            var previous = UnityEngine.RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            UnityEngine.RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            var result = new UnityEngine.Texture2D(texture.width, texture.height, format, false);

            // Copy the pixels from the RenderTexture to the new Texture
            result.ReadPixels(new UnityEngine.Rect(0, 0, tmp.width, tmp.height), 0, 0);
            result.Apply();

            // Broadcast alpha to color
            if (alphaOnly || !HasColor(texture))
            {
                var pixels = result.GetPixels();
                for (var i = 0; i < pixels.Length; i++)
                {
                    pixels[i].r = pixels[i].a;
                    pixels[i].g = pixels[i].a;
                    pixels[i].b = pixels[i].a;
                }
                result.SetPixels(pixels);
                result.Apply();
            }

            // Reset the active RenderTexture
            UnityEngine.RenderTexture.active = previous;

            // Release the temporary RenderTexture
            UnityEngine.RenderTexture.ReleaseTemporary(tmp);
            return result;
        }
    }

    internal class Texture2DAssetImporter : UnityObjectAssetImporter<UnityEngine.Texture2D>
    {
        public override Entity Import(IAssetImporter ctx, UnityEngine.Texture2D texture)
        {
            var entity = ctx.CreateEntity(typeof(Image2D), typeof(Image2DLoadFromFile), typeof(Image2DLoadFromFileImageFile));

            ctx.SetComponentData(entity, new Image2D()
            {
                disableSmoothing = texture.filterMode == UnityEngine.FilterMode.Point,
                imagePixelSize = new Mathematics.float2(texture.width, texture.height),
                hasAlpha = Texture2DAsset.HasAlpha(texture)
            });

            ctx.SetBufferFromString<Image2DLoadFromFileImageFile>(entity, "Data/" + texture.GetGuid().ToString("N"));

            return entity;
        }
    }

    internal class Texture2DAssetExporter : UnityObjectAssetExporter<UnityEngine.Texture2D>
    {
        public override uint ExportVersion => 1;

        public override IEnumerable<FileInfo> Export(FileInfo outputFile, UnityEngine.Texture2D texture)
        {
            var settings = Texture2DAsset.GetSettings(texture);
            var formatType = Texture2DAsset.RealFormatType(texture, settings);
            switch (formatType)
            {
                case TextureFormatType.Source:
                    return AssetExporter.ExportSource(outputFile, texture);
                case TextureFormatType.PNG:
                    return ExportPng(outputFile, texture);
                case TextureFormatType.JPG:
                    //return ExportJpg(outputFile, Object, settings.JpgCompressionQuality);
                    return ExportJpgOptimized(outputFile, texture, settings.JpgCompressionQuality);
                case TextureFormatType.WebP:
                    return ExportWebP(outputFile, texture, settings.WebPCompressionQuality);
                default:
                    throw new InvalidEnumArgumentException(nameof(formatType), (int)formatType, formatType.GetType());
            }
        }

        public override Guid GetExportHash(UnityEngine.Texture2D texture)
        {
            var bytes = new List<byte>();

            // Add tiny texture settings bytes
            var settings = Texture2DAsset.GetSettings(texture);
            var settingsJson = UnityEditor.EditorJsonUtility.ToJson(settings);
            bytes.AddRange(Encoding.ASCII.GetBytes(settingsJson));

            // Add texture importer settings bytes
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(texture);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var importer = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
                if (importer != null)
                {
                    var importerJson = UnityEditor.EditorJsonUtility.ToJson(importer);
                    bytes.AddRange(Encoding.ASCII.GetBytes(importerJson));
                }
            }

            // Add image content hash bytes
            var contentHash = texture.imageContentsHash.ToString();
            bytes.AddRange(Encoding.ASCII.GetBytes(contentHash));

            // New guid from bytes
            return GuidUtility.NewGuid(bytes.ToArray());
        }

        private static IEnumerable<FileInfo> ExportPng(FileInfo outputFile, UnityEngine.Texture2D texture)
        {
            var format = Texture2DAsset.HasAlpha(texture) ? UnityEngine.TextureFormat.RGBA32 : UnityEngine.TextureFormat.RGB24;
            var outputTexture = Texture2DAsset.BlitTexture(texture, format);
            return WritePng(outputFile, outputTexture).AsEnumerable();
        }

        private static IEnumerable<FileInfo> ExportJpg(FileInfo outputFile, UnityEngine.Texture2D texture, uint quality)
        {
            var outputFiles = new List<FileInfo>();

            // Export color as jpg
            var outputColorTexture = Texture2DAsset.BlitTexture(texture, UnityEngine.TextureFormat.RGB24);
            outputFiles.Add(WriteJpg(outputFile, outputColorTexture, quality));

            // Export alpha as png
            if (Texture2DAsset.HasAlpha(texture))
            {
                // Bake alpha into color channels, then kill alpha channel
                var outputAlphaTexture = Texture2DAsset.BlitTexture(texture, UnityEngine.TextureFormat.RGBA32, alphaOnly: true);
                outputAlphaTexture = Texture2DAsset.BlitTexture(outputAlphaTexture, UnityEngine.TextureFormat.RGB24);

                // Export alpha as png
                var outputAlphaFile = new FileInfo(Path.ChangeExtension(GetAlphaFilePath(outputFile), "png"));
                outputFiles.Add(WritePng(outputAlphaFile, outputAlphaTexture));
            }

            return outputFiles;
        }

        private static IEnumerable<FileInfo> ExportJpgOptimized(FileInfo outputFile, UnityEngine.Texture2D texture, uint quality)
        {
            var outputFiles = new List<FileInfo>();

            // Export color as temporary png
            var tmpColorFile = new FileInfo(Path.ChangeExtension(GetTemporaryFilePath(outputFile), "png"));
            var tmpColorTexture = Texture2DAsset.BlitTexture(texture, UnityEngine.TextureFormat.RGB24);
            WritePng(tmpColorFile, tmpColorTexture);

            // Export color as jpg
            outputFiles.Add(ConvertPngToJpgOptimized(outputFile, tmpColorFile, quality));

            // Export jpg alpha as png
            if (Texture2DAsset.HasAlpha(texture))
            {
                // Bake alpha into color channels
                var tmpAlphaFile = new FileInfo(Path.ChangeExtension(GetTemporaryFilePath(outputFile), "png"));
                var tmpAlphaTexture = Texture2DAsset.BlitTexture(texture, UnityEngine.TextureFormat.RGBA32, alphaOnly: true);

                // Export alpha as temporary png
                WritePng(tmpAlphaFile, tmpAlphaTexture);

                // Convert to 8-bit grayscale png
                var outputAlphaFile = new FileInfo(Path.ChangeExtension(GetAlphaFilePath(outputFile), "png"));
                outputFiles.Add(ConvertPngTo8BitGrayscale(outputAlphaFile, tmpAlphaFile));
            }

            return outputFiles;
        }

        internal static IEnumerable<FileInfo> ExportWebP(FileInfo outputFile, UnityEngine.Texture2D texture, uint quality)
        {
            // Export as temporary png
            var tmpFile = new FileInfo(Path.ChangeExtension(GetTemporaryFilePath(outputFile), "png"));
            ExportPng(tmpFile, texture);

            // Export as webp
            return ConvertPngToWebP(outputFile, tmpFile, quality).AsEnumerable();
        }

        private static string GetTemporaryFilePath(FileInfo outputFile)
        {
            var fileName = Path.GetFileName(outputFile.FullName);
            return Path.Combine(UnityEngine.Application.temporaryCachePath, fileName);
        }

        private static string GetAlphaFilePath(FileInfo outputFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(outputFile.FullName);
            return outputFile.FullName.Replace(fileName, fileName + "_alpha");
        }

        private static FileInfo WritePng(FileInfo outputFile, UnityEngine.Texture2D texture)
        {
            var bytes = UnityEngine.ImageConversion.EncodeToPNG(texture);
            outputFile.Directory.Create();
            File.WriteAllBytes(outputFile.FullName, bytes);
            return outputFile;
        }

        private static FileInfo WriteJpg(FileInfo outputFile, UnityEngine.Texture2D texture, uint quality)
        {
            var bytes = UnityEngine.ImageConversion.EncodeToJPG(texture, (int)quality);
            outputFile.Directory.Create();
            File.WriteAllBytes(outputFile.FullName, bytes);
            return outputFile;
        }

        private static FileInfo ConvertPngToJpgOptimized(FileInfo outputFile, FileInfo inputFile, uint quality)
        {
            var input = inputFile.FullName.DoubleQuoted();
            var output = outputFile.FullName.DoubleQuoted();
            quality = Math.Max(0, Math.Min(100, quality));
            outputFile.Directory.Create();

            // This will build progressive jpegs by default; -baseline stops this. Progressive results in better compression.
            ImageTools.Run("moz-cjpeg", $"-quality {quality}", $"-outfile {output}", input);

            inputFile.Delete();
            return outputFile;
        }

        private static FileInfo ConvertPngTo8BitGrayscale(FileInfo outputFile, FileInfo inputFile)
        {
            var input = inputFile.FullName.DoubleQuoted();
            var output = outputFile.FullName.DoubleQuoted();
            outputFile.Directory.Create();

            ImageTools.Run("pngcrush", "-s", "-c 0", input, output);

            inputFile.Delete();
            return outputFile;
        }

        private static FileInfo ConvertPngToWebP(FileInfo outputFile, FileInfo inputFile, uint quality)
        {
            var input = inputFile.FullName.DoubleQuoted();
            var output = outputFile.FullName.DoubleQuoted();
            quality = Math.Max(0, Math.Min(100, quality));
            outputFile.Directory.Create();

            ImageTools.Run("cwebp", "-quiet", $"-q {quality}", input, $"-o {output}");

            inputFile.Delete();
            return outputFile;
        }
    }
}
