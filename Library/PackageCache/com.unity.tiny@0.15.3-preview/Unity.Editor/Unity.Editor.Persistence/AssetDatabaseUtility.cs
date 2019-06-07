using System;
using System.IO;
using Unity.Editor.Extensions;
using UnityEditor;

namespace Unity.Editor.Persistence
{
    internal static class FileInfoAssetDatabaseExtensions
    {
        public static string ToAssetPath(this FileInfo fileInfo)
        {
            return AssetDatabaseUtility.GetPathRelativeToProjectPath(fileInfo.FullName);
        }
        
        public static string ToAssetGuid(this FileInfo fileInfo)
        {
            return AssetDatabaseUtility.GetAssetGuid(fileInfo);
        }
    }
    
    internal static class AssetDatabaseUtility
    {
        /// <summary>
        /// Returns the `Unity` asset path for the given file.
        /// </summary>
        public static string GetAssetGuid(FileInfo fileInfo)
        {
            return AssetDatabase.AssetPathToGUID(GetPathRelativeToProjectPath(fileInfo.FullName));
        }
        
        /// <summary>
        /// Returns the `Unity` asset path for the given full path.
        /// </summary>
        public static string GetPathRelativeToProjectPath(string path)
        {
            // @TODO FIXME
            var packageName = "Packages/com.unity.tiny";
            var assetPath = Path.GetFullPath(path).ToForwardSlash();

            // check if the given path is a package path (relative or installed)
            // assumption: true if the path contains the package name
            var packagePartIndex = assetPath.LastIndexOf(packageName, StringComparison.Ordinal);
            if (packagePartIndex >= 0)
            {
                var localPath = packageName + assetPath.Substring(assetPath.IndexOf('/', packagePartIndex));
                return localPath;
            }

            // otherwise, we assume it can be any path, and attempt normalization
            var projectPath = new DirectoryInfo(".").FullName.ToForwardSlash() + "/";
            assetPath = assetPath.Replace(projectPath, string.Empty);
            return assetPath;
        }
    }
}