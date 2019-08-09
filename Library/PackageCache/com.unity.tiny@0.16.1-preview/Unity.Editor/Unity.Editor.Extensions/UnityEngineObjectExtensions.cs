using System;
using System.Text;
using Unity.Authoring.Hashing;

namespace Unity.Editor.Extensions
{
    internal static class UnityEngineObjectExtensions
    {
        public static Guid GetGuid(this UnityEngine.Object obj)
        {
            if (!UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long fileId))
            {
                return Guid.Empty;
            }

            if (string.IsNullOrEmpty(guid) || guid == "00000000000000000000000000000000")
            {
                // Special case for memory textures
                if (obj is UnityEngine.Texture texture)
                {
                    return texture.imageContentsHash.ToGuid();
                }

                Debug.LogWarning($"Could not get {nameof(Guid)} for object type '{obj.GetType().FullName}'.");
                return Guid.Empty;
            }

            // Merge asset database guid and file identifier
            var bytes = new byte[guid.Length + sizeof(long)];
            Encoding.ASCII.GetBytes(guid).CopyTo(bytes, 0);
            BitConverter.GetBytes(fileId).CopyTo(bytes, guid.Length);
            return GuidUtility.NewGuid(bytes);
        }
    }
}
