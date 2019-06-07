using UnityEditor;
using UnityEngine;

namespace Unity.Editor.Bridge
{
    internal static class PopupWindow
    {
        public static void Show(Rect activatorRect, PopupWindowContent content)
        {
            UnityEditor.PopupWindow.Show(activatorRect, content, null, ShowMode.PopupMenu);
        }
    }
}
