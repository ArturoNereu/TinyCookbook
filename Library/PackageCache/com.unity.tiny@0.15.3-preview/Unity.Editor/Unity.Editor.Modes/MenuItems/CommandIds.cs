namespace Unity.Editor.MenuItems
{
    internal static class CommandIds
    {
        private const string Prefix = "DOTS/";

        public static class File
        {
            public const string NewProject = Prefix + "New Project...";
            public const string OpenProject = Prefix + "Open Project...";
            
            public const string SaveProject = Prefix + "SaveProject";
            public const string CloseProject = Prefix + "CloseProject";
            public const string BuildProject = Prefix + "BuildProject";
            public const string BuildAndRun = Prefix + "BuildAndRun";
            public const string ImportSamples = Prefix + "ImportSamples";
        }

        public static class Edit
        {
            public const string SelectionBasedValidation = Prefix + "SelectionBasedValidation";
            public const string DuplicateSelection = Prefix + "DuplicateSelection";
            public const string DeleteSelection = Prefix + "DeleteSelection";
            public const string Play = Prefix + "Play";
            public const string Pause = Prefix + "Pause";
        }

        public static class Assets
        {
            public const string CreateScene = Prefix + "CreateScene";
            public const string CreateSystem = Prefix + "CreateSystem";
            public const string OpenCSharpProject = Prefix + "OpenCSharpProject";
        }

        public static class Entity
        {
            public const string EntityCreationValidation = Prefix + "EntityCreationValidation";
            public const string CreateEmpty = Prefix + "CreateEntity";
            public const string CreateEmptyChild = Prefix + "CreateChildEntity";
            public const string CreateAudioSource = Prefix + "CreateAudioSource";
            public const string CreateCamera = Prefix + "CreateCamera";
            public const string CreateSprite = Prefix + "CreateSprite";
            public const string CreateCanvas = Prefix + "CreateCanvas";
        }

        public static class Window
        {
            public const string Hierarchy = Prefix + "HierarchyWindow";
            public const string Context = Prefix + "ContextWindow";
            public const string Configuration = Prefix + "ConfigurationWindow";
            public const string BindingsDebugger = Prefix + "BindingsDebuggerWindow";
        }

        public static class Help
        {
            public const string Forums = Prefix + "Forums";
        }

        public static class Validation
        {
            public const string OpenedProjectValidation = Prefix + "OpenedProjectValidation";
        }

        public static class Tools
        {
            public const string EnableCompileEditorTools = Prefix + "EnableCompileEditorTools";
        }
    }
}
