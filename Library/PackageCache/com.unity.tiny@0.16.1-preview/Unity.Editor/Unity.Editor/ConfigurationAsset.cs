using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Unity.Editor
{
    [Serializable]
    internal class ConfigurationAsset : ScriptableObject
    {
        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = Selection.activeObject;
            if (obj is ConfigurationAsset && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long id))
            {
                return true;
            }
            return false;
        }
    }
}
