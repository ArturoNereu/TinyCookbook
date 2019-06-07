using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Authoring.Core;
using Unity.Authoring.Hashing;
using Unity.Editor.Bindings;
using Unity.Editor.Extensions;
using Unity.Editor.MenuItems;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Tiny.Scenes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.Editor.Hierarchy
{
    [InitializeOnLoad]
    internal class EntityHierarchyWindow : EditorWindow
    {
        private const string k_AuthoringRoot = "AuthoringContext";
        private const string k_NonAuthoringRoot = "NonAuthoringContext";
        private const string k_ContentRoot = "Content";
        private const string k_CreateButton = "CreateButton";
        private const string k_FooterRoot = "Footer";
        private const string k_FlexibleSpace = "FlexibleSpace";
        private const int k_FooterHeight = 24;

        private static readonly List<EntityHierarchyWindow> s_ActiveHierarchies;
        private static Project Project { get; set; }
        private static Session Session { get; set; }
        private static IEditorSceneManagerInternal SceneManager { get; set; }

        private UnityComponentCacheManager m_ComponentCache;
        private VisualElement m_AuthoringRoot;
        private VisualElement m_NonAuthoringRoot;
        private VisualElement m_ContentRoot;
        private IMGUIContainer m_TreeViewRoot;
        private EntityHierarchyTree m_TreeView;
        private ToolbarMenu m_CreateButton;
        private UnityEngine.Rect m_LastHoverRect;
        private VisualElement m_FooterRoot;
        private IMGUIContainer m_ConfigRoot;

        [SerializeField] private TreeViewState m_ContentState;
        private List<Guid> TransferSelection { get; } = new List<Guid>();
        private List<SceneGuid> TransferSceneSelection { get; } = new List<SceneGuid>();

        public static void ReloadAll()
        {
            foreach (var hierarchy in s_ActiveHierarchies)
            {
                if (hierarchy && null != hierarchy)
                {
                    hierarchy.m_TreeView?.Invalidate();
                }
            }
        }

        public static void RepaintAll()
        {
            foreach (var hierarchy in s_ActiveHierarchies)
            {
                if (hierarchy && null != hierarchy)
                {
                    hierarchy.Repaint();
                }
            }
        }

        static EntityHierarchyWindow()
        {
            s_ActiveHierarchies = new List<EntityHierarchyWindow>();
            Application.BeginAuthoringProject += EnterAuthoringContext;
            Application.EndAuthoringProject += ExitAuthoringContext;
        }

        private static void EnterAuthoringContext(Project project)
        {
            Project = project;
            Session = Project.Session;
            SceneManager = Session.GetManager<IEditorSceneManagerInternal>();
            foreach (var hierarchy in s_ActiveHierarchies)
            {
                hierarchy.EnterSession();
            }
        }

        private static void ExitAuthoringContext(Project project)
        {
            foreach (var hierarchy in s_ActiveHierarchies)
            {
                hierarchy.ExitSession();
            }

            Project = null;
            Session = null;
            SceneManager = null;
        }

        public void OnSelectionChange()
        {
            if (null == m_TreeView)
            {
                return;
            }

            var shouldRepaint = m_TreeView.GetSelection()
                .Concat(Selection.instanceIDs)
                .Select(EditorUtility.InstanceIDToObject)
                .OfType<GameObject>()
                .Any();

            m_TreeView.SetSelection(Selection.instanceIDs);

            for (var i = Selection.instanceIDs.Length - 1; i >= 0; i--)
            {
                if (null == m_TreeView.FindItem(Selection.instanceIDs[i]))
                {
                    continue;
                }

                m_TreeView.FrameItem(Selection.instanceIDs[i]);
            }

            if (shouldRepaint)
            {
                Repaint();
            }
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            titleContent.text = "Hierarchy";
            s_ActiveHierarchies.Add(this);
            CreateWindow();
            EnterSession();
        }

        private void OnDisable()
        {
            s_ActiveHierarchies.Remove(this);
            ExitSession();
        }

        private void EnterSession()
        {
            if (null == Session)
            {
                return;
            }

            // TODO: Track state
            if (null == m_ContentState)
            {
                m_ContentState = new TreeViewState();
            }

            m_TreeView = new EntityHierarchyTree(Session, m_ContentState);
            m_AuthoringRoot.style.display = DisplayStyle.Flex;
            m_NonAuthoringRoot.style.display = DisplayStyle.None;
            m_ComponentCache = Session.GetManager<UnityComponentCacheManager>();
            Session.GetManager<IChangeManager>().RegisterChangeCallback(HandleChanges);
        }

        private void ExitSession()
        {
            if (null == Session)
            {
                return;
            }

            Session.GetManager<IChangeManager>().UnregisterChangeCallback(HandleChanges);
            m_AuthoringRoot.style.display = DisplayStyle.None;
            m_NonAuthoringRoot.style.display = DisplayStyle.Flex;
            m_TreeView?.Dispose();
            m_TreeView = null;
        }

        private void CreateWindow()
        {
            var template = StyleSheets.Hierarchy;
            template.Template.CloneTree(rootVisualElement);
            rootVisualElement.AddStyleSheetSkinVariant(template.StyleSheet);

            m_AuthoringRoot = rootVisualElement.Q<VisualElement>(k_AuthoringRoot);
            m_NonAuthoringRoot = rootVisualElement.Q<VisualElement>(k_NonAuthoringRoot);
            m_ContentRoot = rootVisualElement.Q<VisualElement>(k_ContentRoot);
            m_CreateButton = rootVisualElement.Q<ToolbarMenu>(k_CreateButton);
            m_FooterRoot = rootVisualElement.Q<VisualElement>(k_FooterRoot);

            var search = rootVisualElement.Q<ToolbarSearchField>("SearchField");
            search.RegisterValueChangedCallback(
                evt =>
                {
                    if (null == m_TreeView)
                    {
                        return;
                    }

                    m_TreeView.FilterString = evt.newValue;
                });

            m_TreeViewRoot = new IMGUIContainer(UpdateTreeView);
            m_ContentRoot.Add(m_TreeViewRoot);
            var flexibleSpace = rootVisualElement.Q<VisualElement>(k_FlexibleSpace);
            flexibleSpace.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            });
            flexibleSpace.RegisterCallback<DragPerformEvent>(evt =>
            {
                m_TreeView.TryHandleDragAndDropFromExternalSource();
            });
            flexibleSpace.RegisterCallback<MouseUpEvent>(evt =>
            {
                switch (evt.button)
                {
                    case 0:
                        Selection.instanceIDs = new int[0];
                        break;
                    case 1:
                        m_TreeView?.ShowEntityContextMenu(null);
                        break;
                }
            });

            m_ConfigRoot = new IMGUIContainer(UpdateConfigView);
            m_FooterRoot.Add(m_ConfigRoot);

            m_AuthoringRoot.style.display = DisplayStyle.None;
            m_NonAuthoringRoot.style.display = DisplayStyle.Flex;
            SetupCreateToolbar();
        }

        private void UpdateTreeView()
        {
            if (null == m_TreeView)
            {
                return;
            }

            if (TransferSelection.Count > 0)
            {
                m_TreeView.TransferSelection(TransferSelection);
                TransferSelection.Clear();
            }

            if (TransferSceneSelection.Count > 0)
            {
                m_TreeView.TransferSelection(TransferSceneSelection);
                TransferSceneSelection.Clear();
            }

            if (m_TreeView.ShouldReload)
            {
                m_TreeView.Reload();    
            }
            
            var height = Mathf.Min(position.height - 24.0f - k_FooterHeight, m_TreeView.totalHeight); 
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.width = Screen.width + 1;
            rect.x = 0;
            m_TreeView.OnGUI(rect);
            var currentHoverRect = m_TreeView.CurrentHoverRect;
            if (m_LastHoverRect != currentHoverRect)
            {
                m_LastHoverRect = currentHoverRect;
                Repaint();
            }

            ExecuteCommands();
        }

        private void ExecuteCommands()
        {
            Event evt = Event.current;

            var execute = evt.type == EventType.ExecuteCommand;
            if (!execute && evt.type != EventType.ValidateCommand)
                return;

            if (evt.commandName == "DeselectAll")
            {
                if (execute)
                {
                    Selection.instanceIDs = Array.Empty<int>();
                    OnSelectionChange();
                }

                evt.Use();
                GUIUtility.ExitGUI();
            }

            if (evt.commandName == "InvertSelection")
            {
                if (execute)
                {
                    int[] instanceIDs = m_TreeView.GetRows()
                        .Select(tvi => tvi.id)
                        .Except(m_TreeView.GetSelection())
                        .Where(id => EditorUtility.InstanceIDToObject(id))
                        .ToArray();

                    Selection.instanceIDs = instanceIDs;
                    OnSelectionChange();
                }

                evt.Use();
                GUIUtility.ExitGUI();
            }
        }

        private void UpdateConfigView()
        {
            if (Project == null)
            {
                return;
            }

            var configEntity = Project.WorldManager.GetConfigEntity();
            if (configEntity == Entity.Null)
            {
                return;
            }

            var configEntityRef = m_ComponentCache.GetEntityReference(configEntity);
            var rect = EditorGUILayout.GetControlRect(false, k_FooterHeight);

            // Background
            if (Selection.activeInstanceID == configEntityRef.GetInstanceID())
            {
                HierarchyGui.BackgroundColor(rect, HierarchyColors.Hierarchy.Selection);
            }
            else if (rect.Contains(Event.current.mousePosition))
            {
                HierarchyGui.BackgroundColor(rect, HierarchyColors.Hierarchy.Hover);
            }

            // Draw top line
            var topLine = rect;
            topLine.width += 1;
            topLine.height = 1;
            HierarchyGui.BackgroundColor(topLine, HierarchyColors.Hierarchy.SceneSeparator);

            // Draw configuration entity
            var entityRect = rect;
            entityRect.x += 2;
            entityRect.y += 2;

            // Draw entity icon
            var iconRect = entityRect;
            iconRect.y += 1;
            iconRect.width = 18;
            EditorGUI.LabelField(iconRect, new GUIContent { image = Icons.Entity });

            // Draw entity label
            var labelRect = entityRect;
            labelRect.x += 18;
            labelRect.y += 2;
            EditorGUI.LabelField(labelRect, "Configuration", GUI.skin.label);

            if (GUI.Button(rect, new GUIContent(), GUI.skin.label))
            {
                Selection.activeInstanceID = configEntityRef.GetInstanceID();
            }
        }

        internal static void SelectScene(SceneGuid guid)
        {
            if (Session == null)
            {
                return;
            }

            foreach (var window in s_ActiveHierarchies)
            {
                window.TransferSceneSelection.Add(guid);
                window.Repaint();
            }
        }
        
        internal static void SelectOnNextPaint(Guid entity)
        {
            if (Session == null)
            {
                return;
            }

            foreach (var window in s_ActiveHierarchies)
            {
                window.TransferSelection.Add(entity);
                window.Repaint();
            }
        }

        internal static void SelectOnNextPaint(List<Guid> entities)
        {
            if (Session == null)
            {
                return;
            }

            foreach (var window in s_ActiveHierarchies)
            {
                window.TransferSelection.AddRange(entities);
                window.Repaint();
            }
        }

        private void SetupCreateToolbar()
        {
            var menu = m_CreateButton.menu;
            menu.AppendAction("Scene", _ => AssetMenuItems.CreateAndOpenScene());
            menu.AppendSeparator();
            menu.AppendAction("Empty Entity", _ => EntityMenuItems.CreateEmpty(), GetDisabledStatusWhenNoSceneSelected);
            menu.AppendAction("Empty Child Entity", _ => EntityMenuItems.CreateEmptyChild(), GetDisabledStatusWhenNoSceneSelected);
            menu.AppendSeparator();
            menu.AppendAction("Audio Source", _ => EntityMenuItems.AudioSource(null), GetDisabledStatusWhenNoSceneSelected);
            menu.AppendAction("Camera", _ => EntityMenuItems.Camera(null), GetDisabledStatusWhenNoSceneSelected);
            menu.AppendAction("Sprite", _ => EntityMenuItems.Sprite(null), GetDisabledStatusWhenNoSceneSelected);
            menu.AppendAction("Canvas", _ => EntityMenuItems.UICanvas(null), GetDisabledStatusWhenNoSceneSelected);
        }

        private static DropdownMenuAction.Status GetDisabledStatusWhenNoSceneSelected(DropdownMenuAction _)
        {
            return SceneManager.GetActiveScene() == Scene.Null
                ? DropdownMenuAction.Status.Disabled
                : DropdownMenuAction.Status.Normal;
        }

        private void HandleChanges(Changes changes)
        {
            if (changes.EntitiesWereCreated ||
                changes.EntitiesWereDeleted ||
                changes.ComponentsWereRemoved ||
                changes.ComponentsWereAdded ||
                changes.ChangedEntitiesWithSetComponent<Parent>().Any() ||
                changes.ChangedEntitiesWithSetSharedComponent<SceneGuid>().Any() ||
                changes.ChangedEntitiesWithSetComponent<SiblingIndex>().Any() ||
                changes.ReparentedEntities().Any() ||
                changes.ChangedEntitiesWithSetComponent<WorkspaceScenes>().Any() || 
                changes.ChangedEntitiesWithSetComponent<ActiveScene>().Any())
            {
                m_TreeView?.Invalidate();
                Repaint();
            }
        }
    }
}
