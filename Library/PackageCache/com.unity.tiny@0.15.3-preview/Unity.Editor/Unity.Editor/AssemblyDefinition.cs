using System;
using System.IO;
using Unity.Editor.Extensions;

namespace Unity.Editor
{
    [Serializable]
    internal class AssemblyDefinition
    {
        [Serializable]
        internal class VersionDefine
        {
            public string name;
            public string expression;
            public string define;
        }

        public string name;
        public string[] references;
        public string[] optionalUnityReferences;
        public string[] includePlatforms;
        public string[] excludePlatforms;
        public bool allowUnsafeCode;
        public bool overrideReferences;
        public string[] precompiledReferences;
        public bool autoReferenced;
        public string[] defineConstraints;
        public VersionDefine[] versionDefines;

        public static AssemblyDefinition Deserialize(FileInfo file)
        {
            var asmdef = new AssemblyDefinition
            {
                autoReferenced = true
            };
            UnityEngine.JsonUtility.FromJsonOverwrite(file.ReadAllText(), asmdef);

            if (asmdef == null)
            {
                throw new Exception($"File '{file.FullName}' does not contain valid asmdef data.");
            }


            if (string.IsNullOrEmpty(asmdef.name))
            {
                throw new Exception("Required property 'name' not set.");
            }

            if ((asmdef.excludePlatforms != null && asmdef.excludePlatforms.Length > 0) &&
                (asmdef.includePlatforms != null && asmdef.includePlatforms.Length > 0))
            {
                throw new Exception("Both 'excludePlatforms' and 'includePlatforms' are set.");
            }

            return asmdef;
        }

        public void Serialize(FileInfo file)
        {
            file.UpdateAllText(UnityEngine.JsonUtility.ToJson(this, true));
        }
    }
}
