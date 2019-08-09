using UnityEditor;

namespace Unity.Editor.Modes
{
    internal static class EditorModes
    {
        private const string k_DotsId = "dots";
        private const string k_DefaultId = "default";

        public static bool IsDotsModeActive => ModeService.currentId == k_DotsId;

        public static void SetDotsMode()
        {
            if (!IsDotsModeActive)
            {
                ModeService.ChangeModeById(k_DotsId);
            }
        }

        public static void SetDefaultMode()
        {
            if (IsDotsModeActive)
            {
                ModeService.ChangeModeById(k_DefaultId);
            }
        }
    }
}
