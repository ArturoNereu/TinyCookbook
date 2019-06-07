using System;
using Unity.Authoring.Core;
using Object = UnityEngine.Object;

namespace Unity.Editor.Extensions
{
    internal static class AssetReferenceExtensions
    {
        internal struct UnityObjectHandle
        {
            public string guid;
            public long fileID;
            public int type;
        }

#if UNITY_EDITOR
        private static readonly string s_EmptyJsonObject = UnityEditor.EditorJsonUtility.ToJson(new Container { o = null });
#endif

        private class Container
        {
            public Object o;
            public static readonly Container Instance = new Container();
        }

        public static AssetReference ToAssetReference(this Object obj)
        {
            var reference = new AssetReference();

            if (!obj || null == obj)
            {
                return reference;
            }

#if UNITY_EDITOR
            var json = UnityEditor.EditorJsonUtility.ToJson(new Container { o = obj });
            if (string.IsNullOrEmpty(json) || json == s_EmptyJsonObject)
            {
                return reference;
            }
            json = json.Substring(5, json.Length - 6);
            object handleObject = new UnityObjectHandle();
            UnityEditor.EditorJsonUtility.FromJsonOverwrite(json, handleObject);
            var handle = (UnityObjectHandle) handleObject;
            reference.Guid = new Guid(handle.guid);
            reference.FileId = handle.fileID;
            reference.Type = handle.type;
#endif
            return reference;
        }

        public static Object ToUnityObject(this AssetReference assetReference)
        {
#if UNITY_EDITOR
            var handle = new UnityObjectHandle
            {
                fileID = assetReference.FileId,
                guid = assetReference.Guid.ToString("N"),
                type = assetReference.Type
            };
            var json = "{\"o\":" + UnityEditor.EditorJsonUtility.ToJson(handle) + "}";

            Container.Instance.o = null;
            UnityEditor.EditorJsonUtility.FromJsonOverwrite(json, Container.Instance);
            return Container.Instance.o;
#else
            return null;
#endif
        }
    }
}
