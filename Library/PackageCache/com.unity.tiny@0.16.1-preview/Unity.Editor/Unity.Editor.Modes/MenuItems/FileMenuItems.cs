using JetBrains.Annotations;
using System;
using System.IO;
using Unity.Editor.Modes;
using Unity.Editor.Utilities;
using UnityEditor;

namespace Unity.Editor.MenuItems
{
    internal static class FileMenuItems
    {
        [MenuItem(CommandIds.File.NewProject)]
        private static void NewProjectMenu()
        {
            NewProject();
        }

        [MenuItem(CommandIds.File.OpenProject)]
        private static void OpenProjectMenu()
        {
            OpenProject();
        }

        [UsedImplicitly, CommandHandler(CommandIds.File.SaveProject, CommandHint.Menu)]
        public static void SaveProject(CommandExecuteContext context)
        {
            SaveProject();
        }

        [UsedImplicitly, CommandHandler(CommandIds.File.CloseProject, CommandHint.Menu)]
        public static void CloseProject(CommandExecuteContext context)
        {
            CloseProject();
        }

        [UsedImplicitly, CommandHandler(CommandIds.File.BuildProject, CommandHint.Menu)]
        public static void BuildProject(CommandExecuteContext context)
        {
            BuildProject();
        }

        [UsedImplicitly, CommandHandler(CommandIds.File.BuildAndRun, CommandHint.Menu)]
        public static void BuildAndRun(CommandExecuteContext context)
        {
            BuildAndRun();
        }

        internal static void NewProject()
        {
            var defaultProjectSettings = ProjectSettings.Default;
            var path = EditorUtility.SaveFilePanelInProject(title: "New Project", defaultName: "NewProject", extension: string.Empty, message: "Select a new project directory");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (Application.AuthoringProject != null)
            {
                Application.SetAuthoringProject(null);
            }

            var file = new FileInfo(path);
            var project = Project.Create(file.Directory, file.Name);
            SessionState.SetString(Project.k_OpenNewlyCreatedProjectSessionKey, project.GetProjectFile().FullName);

            EditorModes.SetDotsMode();
        }

        internal static void OpenProject()
        {
            var path = EditorUtility.OpenFilePanel("Open Project", Application.DataDirectory.FullName, "project");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (Application.AuthoringProject != null)
            {
                Application.SetAuthoringProject(null);
            }

            var project = Project.Open(new FileInfo(path));
            Application.SetAuthoringProject(project);
        }

        internal static void SaveProject()
        {
            Application.AuthoringProject?.Save();
        }

        internal static void CloseProject()
        {
            Application.SetAuthoringProject(null);
        }

        internal static void BuildProject()
        {
            Application.Build();
        }

        internal static void BuildAndRun()
        {
            Application.BuildAndRun();
        }
    }
}
