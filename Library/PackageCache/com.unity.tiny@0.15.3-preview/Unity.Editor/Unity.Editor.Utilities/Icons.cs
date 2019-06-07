using UnityEditor;
using UnityEngine;
using Unity.Editor.Bridge;

namespace Unity.Editor
{
    internal static class Icons
    {
        private const string KIconsDirectory = Constants.PackagePath + "/Editor Default Resources/icons";

        public static Texture2D Project { get; private set; }
        public static Texture2D Scene { get; private set; }
        public static Texture2D ActiveScene { get; private set; }
        public static Texture2D Entity { get; private set; }
        public static Texture2D Settings { get; private set; }

        static Icons()
        {
            LoadIcons();
        }

        private static void LoadIcons()
        {
            Project = LoadIcon("project");
            Scene = LoadIcon("scene");
            ActiveScene = LoadIcon("activeScene");
            Entity = LoadIcon("entity");
            Settings = LoadIcon("settings");
        }

        /// <summary>
        /// Workaround for `EditorGUIUtility.LoadIcon` not working with packages. This can be removed once it does
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static Texture2D LoadIcon(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (EditorGUIUtility.isProSkin)
            {
                name = "d_" + name;
            }

            // Try to use high DPI if possible
            if (Bridge.GUIUtility.pixelsPerPoint > 1.0)
            {
                var texture = LoadIconTexture($"{KIconsDirectory}/{name}@2x.png");
                if (null != texture)
                {
                    return texture;
                }
            }

            // Fallback to low DPI if we couldn't find the high res or we are on a low res screen
            return LoadIconTexture($"{KIconsDirectory}/{name}.png");
        }

        private static Texture2D LoadIconTexture(string path)
        {
            var texture = (Texture2D) AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));

            if (texture != null &&
                !Mathf.Approximately(texture.GetPixelsPerPoint(), (float) Bridge.GUIUtility.pixelsPerPoint) &&
                !Mathf.Approximately((float) Bridge.GUIUtility.pixelsPerPoint % 1f, 0.0f))
            {
                texture.filterMode = FilterMode.Bilinear;
            }

            return texture;
        }
    }
}
