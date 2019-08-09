using UnityEditor;
using UnityEngine;

namespace Unity.Editor.Utilities
{
    internal static class DotsStyles
    {
        public static GUIStyle SettingsSection { get; } = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold,
        };

        public static GUIStyle RightAlignedLabel { get; } = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.UpperRight
        };

        public static GUIStyle AddComponentStyle { get; } = new GUIStyle("AC Button")
        {
        };

        private const float KIconSize = 16.0f;

        public static GUIStyle PaneOptionStyle { get; } = new GUIStyle("PaneOptions")
        {
            fixedHeight = KIconSize,
            fixedWidth = KIconSize
        };
    }
}
