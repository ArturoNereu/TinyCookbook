using System;
using Unity.Editor.Modes;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Unity.Editor
{
    [System.Serializable]
    internal class SceneAsset : ScriptableObject
    {
        [SerializeField] private string m_Guid;
        [SerializeField] private uint m_SerializedVersion;
        [SerializeField] private Texture2D m_Icon;

        internal string Guid
        {
            get => m_Guid;
            set => m_Guid = value;
        }

        internal uint SerializedVersion
        {
            get => m_SerializedVersion;
            set => m_SerializedVersion = value;
        }

        public Texture2D Icon
        {
            get => m_Icon;
            set => m_Icon = value;
        }

        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = Selection.activeObject;

            if (obj is SceneAsset && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long id))
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    Debug.LogWarning("Cannot open Scene while in Play Mode");
                    return true;
                }

                if (!EditorModes.IsDotsModeActive || Application.AuthoringProject == null)
                {
                    Debug.LogWarning("Open a Project before opening a Scene");
                    return true;
                }

                try
                {
                    var authoringSession = Application.AuthoringProject.Session;

                    // @TODO Validation that this scene is part of the AuthoringProject

                    var sceneManager = authoringSession.GetManager<IEditorSceneManager>();
                    sceneManager.LoadScene(AssetDatabase.GetAssetPath(instanceId));
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
