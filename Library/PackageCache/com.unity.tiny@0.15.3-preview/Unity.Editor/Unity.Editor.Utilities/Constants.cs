namespace Unity.Editor
{
    internal static class Constants
    {
        private const string Sep = "/";

        public const string ApplicationName = "Tiny";
        public const string PackageName = "com.unity.tiny";
        public const string PackagePath = "Packages" + Sep + PackageName;

        private const string EditorDefaultResourcesPath = PackagePath + "/Editor Default Resources/";
        public const string UssPath = EditorDefaultResourcesPath + "uss/";
    }
}
