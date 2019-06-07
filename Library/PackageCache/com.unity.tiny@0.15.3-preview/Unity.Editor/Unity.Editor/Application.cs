using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Authoring.Undo;
using Unity.Editor.Build;
using Unity.Editor.Extensions;
using Unity.Editor.Modes;
using Unity.Editor.Tools;
using Unity.Properties;
using Unity.Properties.Reflection;
using Unity.Serialization;
using UnityEditor;
using BuildPipeline = Unity.Editor.Build.BuildPipeline;

namespace Unity.Editor
{
    internal static class Application
    {
        public static event Action<Project> BeginAuthoringProject = delegate { };
        public static event Action<Project> EndAuthoringProject = delegate { };

        public static Project AuthoringProject { get; private set; }
        public static DirectoryInfo RootDirectory => new DirectoryInfo(".");
        public static DirectoryInfo LibraryDirectory => RootDirectory.Combine("Library");
        public const string PackageId = "com.unity.tiny";
        public static DirectoryInfo PackageDirectory => new DirectoryInfo(Path.GetFullPath("Packages/" + PackageId));
        public static DirectoryInfo ToolsDirectory => LibraryDirectory.Combine("DotsEditorTools");
        public static DirectoryInfo MonoDirectory => new DirectoryInfo(Path.Combine(EditorApplication.applicationContentsPath, "MonoBleedingEdge", "bin"));
        public static DirectoryInfo DataDirectory => new DirectoryInfo(UnityEngine.Application.dataPath);
        public static DirectoryInfo OutputDirectory { get; set; } = LibraryDirectory.Combine("DotsRuntimeBuild");
        public static BuildSettings LastBuildSettings { get; internal set; }
        public static BuildResult LastBuildResult { get; internal set; }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            var provider = new ReflectedPropertyBagProvider();
            provider.AddGenerator(new CharPropertyGenerator());

            PropertyBagResolver.RegisterProvider(provider);
            PropertyBagResolver.Register(new SerializedObjectViewPropertyBag());

            AssemblyReloadEvents.beforeAssemblyReload += HandleDomainWillUnload;
            EditorApplication.update += HandleUpdate;
            Bridge.EditorApplication.UpdateMainWindowTitleHandler += MainWindowTitleHandler;
        }

        private static void MainWindowTitleHandler(Bridge.EditorApplication.TitleDescriptor desc)
        {
            if (null == AuthoringProject)
            {
                return;
            }

            var project = AuthoringProject;
            if (null == project.GetProjectFile())
            {
                return;
            }
            var session = project.Session;
            desc.projectName = project.Name + " (DOTS mode)";
            var sceneManager = session.GetManager<IEditorSceneManagerInternal>();
            var scene = sceneManager.GetActiveScene();
            var sceneName = sceneManager.GetSceneName(scene);
            desc.activeSceneName = string.IsNullOrEmpty(sceneName) ? "No loaded scene" : sceneName;
            desc.targetName = string.Empty;
        }

        private static void HandleDomainWillUnload()
        {
            AuthoringProject?.Save();
            Project.CloseAll();
        }

        private static void HandleUpdate()
        {
            if (null != AuthoringProject)
            {
                SessionRunner.Update(AuthoringProject.Session);
            }
        }

        public static void SetAuthoringProject(Project project)
        {
            var previous = AuthoringProject;
            AuthoringProject = project;
            
            if (null != previous)
            {
                EndAuthoringProject(previous);
                previous.Dispose();
            }

            if (null != AuthoringProject)
            {
                EditorModes.SetDotsMode();
                BeginAuthoringProject(AuthoringProject);
                
                project.Session.GetManager<IChangeManager>().Update();
                project.Session.GetManager<IUndoManager>().Flush();
            }
            else
            {
                EditorModes.SetDefaultMode();
            }

            Bridge.EditorApplication.UpdateMainWindowTitle();
        }

        public static bool Build()
        {
            if (AuthoringProject == null)
            {
                Debug.LogError("No project open, cannot build.");
                return false;
            }

            var workspaceManager = AuthoringProject.Session.GetManager<WorkspaceManager>();

            LastBuildSettings = new BuildSettings
            {
                Project = AuthoringProject,
                Platform = workspaceManager.ActivePlatform,
                Configuration = workspaceManager.ActiveConfiguration,
                OutputDirectory = OutputDirectory
            };

            LastBuildResult = BuildPipeline.Build(LastBuildSettings);
            if (LastBuildResult.Success)
            {
                Debug.Log(LastBuildResult.ToString());
            }

            return LastBuildResult.Success;
        }

        public static bool BuildAndRun()
        {
            if (!Build())
            {
                return false;
            }

            return LastBuildSettings.Platform.Run(LastBuildResult.Target);
        }
    }
}
