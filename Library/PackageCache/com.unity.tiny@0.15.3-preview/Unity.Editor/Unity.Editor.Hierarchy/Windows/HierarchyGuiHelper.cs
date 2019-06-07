using System.Linq;
using UnityEditor;
using UnityEngine;
using GUI = UnityEngine.GUI;

namespace Unity.Editor.Hierarchy
{
    internal static class HierarchyColors
    {
        private static bool ProSkin => EditorGUIUtility.isProSkin;

        internal static class Hierarchy
        {
            public static Color SceneItem { get; } = ProSkin ? new Color32(0x3D, 0x3D, 0x3D, 0xFF) : new Color32(0xDA, 0xDA, 0xDA, 0xFF);
            public static Color Hover { get; } = ProSkin ? new Color(1.0f, 1.0f, 1.0f, 0.06f) : new Color(0.0f, 0.0f, 0.0f, 0.06f);
            public static Color SceneSeparator { get; } = ProSkin ? new Color32(0x21, 0x21, 0x21, 0xFF) : new Color32(0x96, 0x96, 0x96, 0xFF);
            public static Color Disabled { get; } =  new Color32(0xFF, 0xFF, 0xFF, 0x93);
            public static Color Prefab { get; } = new Color32(0x6C, 0xB6, 0xFF, 0xFF);
            public static Color Selection { get; } = new Color32(0x3E, 0x5F, 0x96, 0xFF);
        }
    }

    internal static class HierarchyGui
    {
        public static void BackgroundColor(Rect rect, Color color)
        {
            var oldColor = GUI.color;
            GUI.color = color;

            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            GUI.color = oldColor;
        }

        private static readonly int s_FolderPickerHash = "TinyFolderPicker".GetHashCode();
        public static DefaultAsset FolderField(Rect rect, string label, DefaultAsset folder)
        {
            var folderAsset = folder;
            if (!AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(folder)))
            {
                folderAsset = null;
            }

            folderAsset = (DefaultAsset)EditorGUI.ObjectField(rect, label, folderAsset, typeof(DefaultAsset), false);
            // By default, it will output None (Default Asset), we want to display None (Folder)
            if (null == folderAsset)
            {
                var id = GUIUtility.GetControlID(s_FolderPickerHash, FocusType.Keyboard, rect);
                rect.x += EditorGUIUtility.labelWidth;
                rect.width -= EditorGUIUtility.labelWidth;
                if (Event.current.type == EventType.Repaint)
                {
                    var highlighted = false;
                    if (rect.Contains(Event.current.mousePosition) && GUI.enabled)
                    {
                        if (null != DragAndDrop.objectReferences.FirstOrDefault(obj => obj is DefaultAsset))
                        {
                            highlighted = true;
                        }
                    }
                    EditorStyles.objectField.Draw(rect, new GUIContent("None (Folder)"), id, highlighted);
                }
            }

            if (null != folderAsset)
            {
                if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(folderAsset)))
                {
                    return folderAsset;
                }
            }

            return null;
        }
    }

    internal class GuiColorScope : GUI.Scope
    {
        private readonly Color m_Color;

        public GuiColorScope(Color color)
        {
            m_Color = GUI.color;
            GUI.color = color;
        }

        protected override void CloseScope()
        {
            GUI.color = m_Color;
        }
    }
}
