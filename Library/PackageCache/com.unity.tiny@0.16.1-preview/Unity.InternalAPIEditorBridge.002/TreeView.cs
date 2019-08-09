using UnityEditor.IMGUI.Controls;

namespace Unity.Editor.Bridge
{
    internal class TreeView
    {
        internal static TreeViewItem FindItem(int id, TreeViewItem searchFromThisItem)
        {
            return TreeViewUtility.FindItem(id, searchFromThisItem);
        }
    }
}
