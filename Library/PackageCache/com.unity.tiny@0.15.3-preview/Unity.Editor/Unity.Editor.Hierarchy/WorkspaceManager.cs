using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Editor.Build;
using Unity.Editor.Persistence;
using Unity.Serialization.Json;
using Unity.Tiny.Scenes;
using UnityEditor;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Editor
{
    [InitializeOnLoad]
    internal class WorkspaceManager : SessionManager
    {
        private EditorSceneManager m_SceneManager;
        private Workspace m_CurrentWorkspace;
        private string m_CurrentWorkspaceKey;
        private bool m_SuspendEvents;

        public Platform ActivePlatform
        {
            get
            {
                if (m_CurrentWorkspace == null)
                    return PlatformSettings.GetDefaultPlatform();

                return PlatformSettings.GetPlatformFromName(m_CurrentWorkspace.Platform);
            }
            set
            {
                if (m_CurrentWorkspace != null)
                {
                    var platformName = PlatformSettings.GetPlatformName(value);
                    if (m_CurrentWorkspace.Platform != platformName)
                    {
                        m_CurrentWorkspace.Platform = platformName;
                        SaveWorkspace();
                    }
                }
            }
        }

        public Configuration ActiveConfiguration
        {
            get
            {
                if (m_CurrentWorkspace == null)
                    return Configuration.Develop;

                return m_CurrentWorkspace.Configuration;
            }
            set
            {
                if (m_CurrentWorkspace != null && m_CurrentWorkspace.Configuration != value)
                {
                    m_CurrentWorkspace.Configuration = value;
                    SaveWorkspace();
                }
            }
        }

        public WorkspaceManager(Session session) : base(session) { }

        static WorkspaceManager()
        {
            Project.ProjectCreated += OnProjectCreated;
        }

        private static void OnProjectCreated(Project project)
        {
            var ws = new Workspace();
            var scenes = project.GetScenes();
            Assert.IsTrue(scenes.Length > 0);
            ws.ActiveScene = scenes[0].SceneGuid;
            ws.Scenes.Add(ws.ActiveScene);
            SaveWorkspace(ws, GetWorkspaceKey(project));
        }

        public override void Load()
        {
            Application.BeginAuthoringProject += OnBeginAuthoringProject;
            Session.GetManager<IChangeManager>().RegisterChangeCallback(HandleChanges);

            m_SceneManager = Session.GetManager<EditorSceneManager>();
        }

        public override void Unload()
        {
            Application.BeginAuthoringProject -= OnBeginAuthoringProject;
            Session.GetManager<IChangeManager>().UnregisterChangeCallback(HandleChanges);
        }

        private void OnBeginAuthoringProject(Project project)
        {
            LoadWorkspace(project);

            if (DomainReload.IsDomainReloading)
                return;

            ReopenScenes();
        }

        private void LoadWorkspace(Project project)
        {
            m_CurrentWorkspaceKey = GetWorkspaceKey(project);
            if (EditorPrefs.HasKey(m_CurrentWorkspaceKey))
            {
                try
                {
                    var serializedWorkspace = EditorPrefs.GetString(m_CurrentWorkspaceKey);
                    if (!string.IsNullOrEmpty(serializedWorkspace))
                    {
                        m_CurrentWorkspace = JsonSerialization.DeserializeFromString<Workspace>(serializedWorkspace);
                        m_CurrentWorkspace.Initialize();
                    }
                    else
                    {
                        CreateNewWorkspace();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Unable to deserialize workspace for project {project.Name}");
                    Debug.LogException(e);

                    CreateNewWorkspace();
                }
            }
            else
            {
                CreateNewWorkspace();
            }
        }

        private static string GetWorkspaceKey(Project project) => $"DOTS.Editor.Workspace.{project.Guid:N}";

        private void CreateNewWorkspace()
        {
            m_CurrentWorkspace = new Workspace();
            m_CurrentWorkspace.Initialize();
            SaveWorkspace();
        }

        private void ReopenScenes()
        {
            var persistenceManager = Session.GetManager<IPersistenceManager>();
            var activeScene = Scene.Null;

            try
            {
                m_SuspendEvents = true;
                foreach (var sceneGuid in m_CurrentWorkspace.Scenes)
                {
                    var scenePath = persistenceManager.GetSceneAssetPath(sceneGuid);
                    if (string.IsNullOrEmpty(scenePath))
                    {
                        Debug.LogWarning($"Unable to find scene path associated to asset id {sceneGuid}");
                        continue;
                    }

                    var scene = m_SceneManager.LoadScene(scenePath);
                    if (sceneGuid == m_CurrentWorkspace.ActiveScene)
                    {
                        activeScene = scene;
                    }
                }

                if (activeScene != Scene.Null)
                {
                    m_SceneManager.SetActiveScene(activeScene);
                }
            }
            finally
            {
                m_SuspendEvents = false;
            }
        }
        
        private void HandleChanges(Changes changes)
        {
            if (m_SuspendEvents || null == m_CurrentWorkspace)
                return;

            if (changes.ChangedEntitiesWithSetComponent<WorkspaceScenes>().Any() || 
                changes.ChangedEntitiesWithSetComponent<ActiveScene>().Any())
            {
                m_CurrentWorkspace.ClearScenes();
                for (var i = 0; i < m_SceneManager.LoadedSceneCount; ++i)
                {
                    m_CurrentWorkspace.AddScene(m_SceneManager.GetLoadedSceneAtIndex(i));
                }

                m_CurrentWorkspace.SetActiveScene(m_SceneManager.GetActiveScene());
                SaveWorkspace();
            }
        }

        private void SaveWorkspace() => SaveWorkspace(m_CurrentWorkspace, m_CurrentWorkspaceKey);

        private static void SaveWorkspace(Workspace workspace, string workspaceKey)
        {
            var serializedWorkspace = JsonSerialization.Serialize(workspace);
            EditorPrefs.SetString(workspaceKey, serializedWorkspace);
        }

        class Workspace
        {
            private HashSet<Guid> m_Scenes;

            public readonly List<Guid> Scenes = new List<Guid>();
            public Guid ActiveScene;

            public string Platform;
            public Configuration Configuration;

            public void Initialize()
            {
                m_Scenes = new HashSet<Guid>(Scenes);
            }

            public bool AddScene(Scene scene)
            {
                var sceneId = scene.SceneGuid.Guid;
                if (m_Scenes.Add(sceneId))
                {
                    Scenes.Add(sceneId);
                    return true;
                }
                return false;
            }

            public bool RemoveScene(Scene scene)
            {
                var sceneId = scene.SceneGuid.Guid;
                if (m_Scenes.Remove(sceneId))
                {
                    Scenes.Remove(sceneId);
                    return true;
                }
                return false;
            }

            public bool ClearScenes()
            {
                if (m_Scenes.Count == 0)
                {
                    return false;
                }
                m_Scenes.Clear();
                Scenes.Clear();
                return true;
            }

            public bool SetActiveScene(Scene scene)
            {
                var sceneGuid = scene.SceneGuid.Guid;
                if (sceneGuid != ActiveScene)
                {
                    ActiveScene = sceneGuid;
                    return true;
                }
                return false;
            }
        }
    }
}
