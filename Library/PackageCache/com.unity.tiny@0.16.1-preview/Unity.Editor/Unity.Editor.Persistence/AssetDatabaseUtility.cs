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
        /// Returns the `Unity` asset guid for the given file.
        /// </summary>
        public static string GetAssetGuid(FileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                var assetPath = GetPathRelativeToProjectPath(fileInfo.FullName);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    return AssetDatabase.AssetPathToGUID(assetPath);
                }
            }
            return null;
        }
        
        /// <summary>
        /// Returns the `Unity` asset path for the given full path.
        /// </summary>
        public static string GetPathRelativeToProjectPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var packagePath = Application.PackageDirectory.FullName.ToForwardSlash();
            var assetPath = Path.GetFullPath(path).ToForwardSlash();

            // check if the given path is a package path (relative or installed)
            // assumption: true if the path contains the package name
            var packagePartIndex = assetPath.LastIndexOf(packagePath, StringComparison.Ordinal);
            if (packagePartIndex >= 0)
            {
                var localPath = packagePath + assetPath.Substring(assetPath.IndexOf('/', packagePartIndex));
                return localPath;
            }

            // otherwise, we assume it can be any path, and attempt normalization
            var projectPath = Application.RootDirectory.FullName.ToForwardSlash() + "/";
            assetPath = assetPath.Replace(projectPath, string.Empty);
            return assetPath;
        }
    }
}