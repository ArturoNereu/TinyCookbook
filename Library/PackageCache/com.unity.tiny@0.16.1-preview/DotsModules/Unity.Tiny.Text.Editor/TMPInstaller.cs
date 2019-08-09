using UnityEditor;
using UnityEngine;

namespace Unity.Tiny.Text.Editor
{
    internal static class TMPInstaller
    {
        [InitializeOnLoadMethod]
        private static void InstallEssentialResources()
        {
            Unity.Editor.Application.BeginAuthoringProject += project =>
            {
                // TODO: move this utility in TMP
                if (!AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
                {
                    var packageFullPath = System.IO.Path.GetFullPath("Packages/com.unity.textmeshpro");
                    AssetDatabase.ImportPackage(
                        packageFullPath + "/Package Resources/TMP Essential Resources.unitypackage",
                        false);
                    
                    Debug.Log("Installed TextMesh Pro essential resources");
                }
            };
        }
    }
}