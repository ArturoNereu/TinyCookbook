using System;
using UnityEditor;
using UnityEngine;
using TMPro;
using Unity.Editor.Extensions;

namespace Unity.Tiny.Text.Editor
{
    internal static class Fonts
    {
        private static readonly Vector2 LowerLeft = new Vector2(0.0f, 0.0f);
        private static readonly Vector2 LowerCenter = new Vector2(0.5f, 0.0f);
        private static readonly Vector2 LowerRight = new Vector2(1.0f, 0.0f);
        
        private static readonly Vector2 MiddleLeft = new Vector2(0.0f, 0.5f);
        private static readonly Vector2 MiddleCenter = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 MiddleRight = new Vector2(1.0f, 0.5f);
        
        private static readonly Vector2 UpperLeft = new Vector2(0.0f, 1.0f);
        private static readonly Vector2 UpperCenter = new Vector2(0.5f, 1.0f);
        private static readonly Vector2 UpperRight = new Vector2(1.0f, 1.0f);
        
        private static TMP_FontAsset SansSerifRegular { get; }
        private static TMP_FontAsset SansSerifBold { get; }
        private static TMP_FontAsset SansSerifItalic { get; }
        private static TMP_FontAsset SansSerifBoldItalic { get; }

        private static TMP_FontAsset SerifRegular { get; }
        private static TMP_FontAsset SerifBold { get; }
        private static TMP_FontAsset SerifItalic { get; }
        private static TMP_FontAsset SerifBoldItalic { get; }

        private static TMP_FontAsset MonoSpaceRegular { get; }
        private static TMP_FontAsset MonoSpaceBold { get; }
        private static TMP_FontAsset MonoSpaceItalic { get; }
        private static TMP_FontAsset MonoSpaceBoldItalic { get; }

        private static TMP_FontAsset Load(string name)
        {
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Packages/com.unity.tiny/Editor Default Resources/fonts/" + name);
        }

        static Fonts()
        {
            const string sansSerifRoot = "liberation-sans/";
            SansSerifRegular = Load(sansSerifRoot + "LiberationSans-Regular SDF.asset");
            SansSerifItalic = Load(sansSerifRoot + "LiberationSans-Italic SDF.asset");

            const string serifRoot = "liberation-serif/";
            SerifRegular = Load(serifRoot + "LiberationSerif-Regular SDF.asset");
            SerifItalic = Load(serifRoot + "LiberationSerif-Italic SDF.asset");

            const string monospaceRoot = "liberation-mono/";
            MonoSpaceRegular = Load(monospaceRoot + "LiberationMono-Regular SDF.asset");
            MonoSpaceItalic = Load(monospaceRoot + "LiberationMono-Italic SDF.asset");
        }

        public static TMP_FontAsset GetSansSerifFont(bool italic)
        {
            if (italic)
            {
                return SansSerifItalic;
            }

            return SansSerifRegular;
        }

        public static TMP_FontAsset GetSerifFont(bool italic)
        {
            if (italic)
            {
                return SerifItalic;
            }

            return SerifRegular;
        }

        public static TMPro.TMP_FontAsset GetMonoSpaceFont(bool italic)
        {
            if (italic)
            {
                return MonoSpaceItalic;
            }

            return MonoSpaceRegular;
        }

        public static TextAlignmentOptions GetTextAlignmentFromPivot(Vector2 pivot)
        {
            if (pivot == LowerLeft)
            {
                return TextAlignmentOptions.BottomLeft;
            }
            if (pivot == LowerCenter)
            {
                return TextAlignmentOptions.Bottom;
            }
            if (pivot == LowerRight)
            {
                return TextAlignmentOptions.BottomRight;
            }
            if (pivot == MiddleLeft)
            {
                return TextAlignmentOptions.Left;
            }
            if (pivot == MiddleCenter)
            {
                return TextAlignmentOptions.Center;
            }
            if (pivot == MiddleRight)
            {
                return TextAlignmentOptions.Right;
            }
            if (pivot == UpperLeft)
            {
                return TextAlignmentOptions.TopLeft;
            }
            if (pivot == UpperCenter)
            {
                return TextAlignmentOptions.Top;
            }
            if (pivot == UpperRight)
            {
                return TextAlignmentOptions.TopRight;
            }
            throw new ArgumentOutOfRangeException(nameof(pivot), pivot, null);
        }
    }

    internal static class TtfFonts
    {
        private const string SansfontFolderPath = "/Editor Default Resources/fonts/liberation-sans/";
        private const string SeriffontFolderPath = "/Editor Default Resources/fonts/liberation-serif/";
        private const string MonofontFolderPath = "/Editor Default Resources/fonts/liberation-mono/";
        
        public static string GetSansTtfFontPath(string packagePath, bool italic, bool bold)
        {
             if(bold && italic)
             {
                 return packagePath + SansfontFolderPath + "LiberationSans-BoldItalic.ttf";
             }
             if(bold)
             {
                 return packagePath + SansfontFolderPath + "LiberationSans-Bold.ttf";
             }
             if (italic)
             {
                 return packagePath + SansfontFolderPath + "LiberationSans-Italic.ttf";
             }
            return packagePath + SansfontFolderPath +  "LiberationSans-Regular.ttf";
        }

        public static string GetSerifTtfFontPath(string packagePath, bool italic, bool bold)
        {
            if (bold && italic)
            {
                return packagePath + SeriffontFolderPath + "LiberationSerif-BoldItalic.ttf";
            }
            if (bold)
            {
                return packagePath + SeriffontFolderPath + "LiberationSerif-Bold.ttf";
            }
            if (italic)
            {
                return packagePath + SeriffontFolderPath + "LiberationSerif-Italic.ttf";
            }
            return packagePath + SeriffontFolderPath + "LiberationSerif-Regular.ttf";
        }

        public static string GetMonoTtfFontPath(string packagePath, bool italic, bool bold)
        {
            if (bold && italic)
            {
                return packagePath + MonofontFolderPath + "LiberationMono-BoldItalic.ttf";
            }
            if (bold)
            {
                return packagePath + MonofontFolderPath + "LiberationMono-Bold.ttf";
            }
            if (italic)
            {
                return packagePath + MonofontFolderPath + "LiberationMono-Italic.ttf";
            }
            return packagePath + MonofontFolderPath + "LiberationMono-Regular.ttf";
        }

    }
}
