
namespace Unity.Editor.Bridge
{
#if UNITY_EDITOR
    internal static class GUIUtility
    {
        public static double pixelsPerPoint => UnityEngine.GUIUtility.pixelsPerPoint;
    }
#endif
}
