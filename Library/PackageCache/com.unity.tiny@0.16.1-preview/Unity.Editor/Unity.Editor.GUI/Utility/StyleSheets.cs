using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal static class StyleSheets
    {
        public readonly struct UxmlTemplate
        {
            private readonly string UxmlPath;
            private readonly string UssPath;

            public UxmlTemplate(string name)
            {
                UxmlPath = k_UxmlBasePath + name + ".uxml";
                UssPath = k_UssBasePath + name + ".uss";
            }

            public VisualTreeAsset Template => EditorGUIUtility.Load(UxmlPath) as VisualTreeAsset;
            public StyleSheet StyleSheet => AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
        }

        private const string k_BasePath = "Packages/com.unity.tiny/Editor Default Resources/";
        private const string k_UssBasePath = k_BasePath + "uss/";
        private const string k_UxmlBasePath = k_BasePath + "uxml/";

        public static readonly UxmlTemplate Inspector = new UxmlTemplate("entity-inspector");
        public static readonly UxmlTemplate EntityHeader = new UxmlTemplate("entity-inspector-header");
        public static readonly UxmlTemplate BindingsDebugger = new UxmlTemplate("bindings-debugger-window");
        public static readonly UxmlTemplate ListItem = new UxmlTemplate("entity-inspector-list-item");

        public static readonly UxmlTemplate Hierarchy = new UxmlTemplate("entity-hierarchy");
    }
}
