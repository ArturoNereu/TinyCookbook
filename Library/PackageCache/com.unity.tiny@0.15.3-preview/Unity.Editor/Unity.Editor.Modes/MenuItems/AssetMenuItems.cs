using JetBrains.Annotations;
using System;
using NiceIO;
using Unity.Authoring;
using Unity.Editor.Build;
using Unity.Editor.Extensions;
using Unity.Editor.Hierarchy;
using Unity.Editor.Persistence;
using Unity.Editor.Utilities;
using Unity.Tiny.Scenes;
using UnityEditor.ProjectWindowCallback;
using CommandExecuteContext = UnityEditor.CommandExecuteContext;
using CommandHandler = UnityEditor.CommandHandlerAttribute;
using CommandHint = UnityEditor.CommandHint;
using UnityEngine;
using System.IO;

namespace Unity.Editor.MenuItems
{
    internal static class AssetMenuItems
    {
        //Menu item to be used outside of DOTS mode
        [UnityEditor.MenuItem("Assets/Create/Tiny Project", false, 900)]
        public static void CreateTinyProject()
        {
            FileMenuItems.NewProject();
        }

        [UsedImplicitly, CommandHandler(CommandIds.Validation.OpenedProjectValidation, CommandHint.Menu | CommandHint.Validate)]
        public static void ValidateCreateScene(CommandExecuteContext context)
        {
            context.result = Application.AuthoringProject != null;
        }

        [UsedImplicitly, CommandHandler(CommandIds.Assets.CreateSystem, CommandHint.Menu)]
        public static void CreateSystem(CommandExecuteContext context = null)
        {
            CreateScript("NewSystem.cs", name =>
            {
                var newSystemTemplate = Application.PackageDirectory.Combine("Unity.Editor", "Unity.Editor").GetFile("NewSystemTemplate.cs.in").ReadAllText();
                return newSystemTemplate.Replace("{{SystemName}}", name);
            });
        }

        [UsedImplicitly, CommandHandler(CommandIds.Assets.CreateScene, CommandHint.Menu)]
        public static void CreateScene(CommandExecuteContext context = null)
        {
            CreateAsset(CreateScene);
        }

        public static void CreateAndOpenScene()
        {
            CreateAsset(CreateScene, true);
        }

        private static void CreateAsset(Action<Session, Project, Action<SceneAsset>> creator, bool open = false)
        {
            var project = Application.AuthoringProject;
            var session = project.Session;
            creator.Invoke(session, project, sceneAsset =>
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);
                UnityEditor.Selection.activeInstanceID = UnityEditor.AssetDatabase.LoadAssetAtPath<SceneAsset>(path).GetInstanceID();
                if (open)
                {
                    session.GetManager<IEditorSceneManager>().LoadScene(path);
                    UnityEditor.EditorWindow.FocusWindowIfItsOpen<EntityHierarchyWindow>();
                    EntityHierarchyWindow.SelectScene(new SceneGuid { Guid = Guid.Parse(sceneAsset.Guid) });
                }
            });
        }

        private static void CreateScene(Session session, Project project, Action<SceneAsset> onComplete = null)
        {
            // Build our `CreateAsset` request object
            var create = UnityEngine.ScriptableObject.CreateInstance<DoCreateScene>();

            // Initialize the "create asset request"
            create.Session = session;
            create.WorldManager = session.GetManager<IWorldManager>();
            create.PersistenceManager = session.GetManager<IPersistenceManager>();
            create.Project = project;
            create.OnComplete = onComplete;
            var path = "NewScene.scene";

            // This will prompt the user in the `Asset` window to name the asset using a 'Unity' like flow
            UnityEditor.ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                create,
                path,
                Icons.Scene,
                null);
        }

        public static void CreateScript(string defaultPath, Func<string, string> nameToContents)
        {
            // Build our `CreateAsset` request object
            var create = ScriptableObject.CreateInstance<DoCreateScript>();

            // Initialize the "create asset request"
            create.NameToContents = nameToContents;

            // This will prompt the user in the `Asset` window to name the asset using a 'Unity' like flow
            UnityEditor.ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                create,
                defaultPath,
                null,
                null);
        }

        internal abstract class CreateAssetRequest<T> : EndNameEditAction
        {
            public Session Session { protected get; set; }
            public IWorldManager WorldManager { protected get; set; }
            public IPersistenceManager PersistenceManager { protected get; set; }
            public Project Project { protected get; set; }
            public Action<T> OnComplete { protected get; set; }
        }

        internal class DoCreateScene : CreateAssetRequest<SceneAsset>
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var scene = SceneManager.Create(Guid.NewGuid());
                PersistenceManager.SaveScene(WorldManager.EntityManager, scene, pathName);
                Project.AddScene(new SceneReference { SceneGuid = scene.SceneGuid.Guid });
                var path = PersistenceManager.GetSceneAssetPath(scene);
                var sceneAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                OnComplete?.Invoke(sceneAsset);
            }
        }
        internal class DoCreateScript : EndNameEditAction
        {
            public Func<string, string> NameToContents { protected get; set; }

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var contents = NameToContents.Invoke(Path.GetFileNameWithoutExtension(pathName));
                File.WriteAllText(pathName, contents);
                UnityEditor.AssetDatabase.ImportAsset(pathName, UnityEditor.ImportAssetOptions.ForceSynchronousImport | UnityEditor.ImportAssetOptions.ForceUncompressedImport);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(pathName);

                if (asset)
                {
                    UnityEditor.Selection.activeInstanceID = asset.GetInstanceID();
                }
            }
        }

        [UsedImplicitly, CommandHandler(CommandIds.Assets.OpenCSharpProject, CommandHint.Menu)]
        public static void OpenCSharpProject(CommandExecuteContext context)
        {
            OpenCSharpProject();
        }

        internal static void OpenCSharpProject()
        {
            using (var progress = new ProgressBarScope("Generating DOTS C# Project", "Please wait..."))
            {
                var project = Application.AuthoringProject;
                var context = new BuildPipeline.BuildContext(new BuildSettings
                {
                    Project = project,
                    Platform = new DesktopDotNetPlatform(),
                    Configuration = Configuration.Debug,
                    OutputDirectory = Application.OutputDirectory
                }, progress);

                try
                {
                    BuildProgramDataFileWriter.WriteAll(context.OutputDirectory.FullName);
                    if (BuildStep.GenerateProjectFiles.Run(context))
                    {
                        var dotsSolutionFile = Application.RootDirectory.GetFile($"{new NPath(Application.DataDirectory).Parent.FileName}-Dots.sln");
                        if (dotsSolutionFile.Exists)
                        {
                            Bridge.EditorApplication.OpenCSharpSolution(dotsSolutionFile);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to generate DOTS C# project files.\n{e}");
                }
            }
        }
    }
}
