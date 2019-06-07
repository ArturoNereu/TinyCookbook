using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Unity.Editor
{
    [Serializable]
    internal class ProjectAsset : ScriptableObject
    {
        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = Selection.activeObject;

            if (obj is ProjectAsset &&
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long id))
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    Debug.LogWarning("Cannot open Project while in Play Mode");
                    return true;
                }

                try
                {
                    var path = AssetDatabase.GetAssetPath(instanceId);
                    if (string.IsNullOrEmpty(path))
                    {
                        Debug.LogWarning("Cannot open select Project: asset not found in the Asset Database");
                        return true;
                    }

                    if (Application.AuthoringProject != null)
                    {
                        Application.SetAuthoringProject(null);
                    }

                    var project = Project.Open(new FileInfo(path));
                    Application.SetAuthoringProject(project);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return true;
                }
            }

            return false;
        }
    }
}
