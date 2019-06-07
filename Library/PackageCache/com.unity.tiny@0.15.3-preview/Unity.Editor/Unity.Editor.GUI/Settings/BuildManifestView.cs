using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Editor.Extensions;
using Unity.Entities;
using Unity.Tiny.Scenes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Unity.Editor.Hierarchy;
using Unity.Editor.Persistence;
using Unity.Authoring.ChangeTracking;

namespace Unity.Editor
{
    internal class BuildManifestView : TreeView, IDisposable
    {
        enum Column
        {
            Icon,
            Scene,
            StartupToggle
        }

        internal class BuildManifestViewItem : TreeViewItem
        {

        }

        internal static class BuildManifestColors
        {
            private static bool ProSkin => EditorGUIUtility.isProSkin;

            internal static class Table
            {
                public static Color Hover { get; } = ProSkin ? new Color(1.0f, 1.0f, 1.0f, 0.1f) : new Color(0.0f, 0.0f, 0.0f, 0.1f);
            }
        }

        private class SceneReferenceItem : BuildManifestViewItem
        {
            private readonly IPersistenceManager m_PersistenceManager;

            public SceneReferenceItem(IPersistenceManager persistenceManager, Guid sceneGuid, bool isStartup)
            {
                m_PersistenceManager = persistenceManager;

                IsStartup = isStartup;
                SceneGuid = sceneGuid;
                ScenePath = m_PersistenceManager.GetSceneAssetPath(SceneGuid);
            }

            public bool IsStartup { get; set; }
            public Guid SceneGuid { get; }
            public string ScenePath { get; }

            public override string displayName => null;
            public override int id => SceneGuid.GetHashCode();
            public override int depth => parent?.depth + 1 ?? 0;
        }

        private readonly IPersistenceManager m_PersistenceManager;
        private readonly IChangeManager m_ChangeManager;
        private readonly EntityManager m_EntityManager;
        private readonly MultiColumnHeaderState m_MultiColumnHeaderState;
        private Rect m_CurrentHoveredRect;

        internal Rect CurrentHoverRect => m_CurrentHoveredRect;
        private bool ShouldReload { get; set; }

        public void Invalidate()
        {
            ShouldReload = true;
        }

        public BuildManifestView(TreeViewState state) : base(state, new MultiColumnHeader(CreateMultiColumnHeaderState()))
        {
            var session = Application.AuthoringProject.Session;
            m_PersistenceManager = session.GetManager<IPersistenceManager>();
            m_ChangeManager = session.GetManager<IChangeManager>();
            m_ChangeManager.RegisterChangeCallback(HandleChanges);
            AssetPostprocessorCallbacks.RegisterAssetMovedHandlerForType<SceneAsset>(HandleMovedAsset);
            
            multiColumnHeader.sortingChanged += OnSortingChanged;
            showAlternatingRowBackgrounds = true;

            Reload();
        }

        private void HandleChanges(Changes changes)
        {
            if (changes.ComponentsWereModified ||
                changes.ComponentsWereRemoved ||
                changes.ComponentsWereAdded ||
                changes.EntitiesWereDeleted)
            {
                Invalidate();
                Repaint();
            }
        }

        private void HandleMovedAsset(SceneAsset scene, PostprocessEventArgs args)
        {
            Invalidate();
            Repaint();
        }

        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
            {
                return; // No column to sort for (just use the order the data are in)
            }

            SortColumn();
            TreeToList(root, rows);
            Repaint();
        }

        void SortColumn()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            var items = rootItem.children.Cast<SceneReferenceItem>();
            int columnIndex = multiColumnHeader.sortedColumnIndex;
            Column column = (Column)columnIndex;
            bool ascending = multiColumnHeader.IsSortedAscending(columnIndex);
            switch (column)
            {
                case Column.StartupToggle:
                    if (ascending)
                    {
                        items = items.OrderBy(item => item.IsStartup);
                    }
                    else
                    {
                        items = items.OrderByDescending(item => item.IsStartup);
                    }
                    break;
                default:
                case Column.Scene:
                    if (ascending)
                    {
                        items = items.OrderBy(item => item.ScenePath);
                    }
                    else
                    {
                        items = items.OrderByDescending(item => item.ScenePath);
                    }
                    break;
            }

            rootItem.children = items.Cast<TreeViewItem>().ToList();
        }

        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
        {
            if (root == null)
                throw new NullReferenceException("root");
            if (result == null)
                throw new NullReferenceException("result");

            result.Clear();

            if (root.children == null)
                return;

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for (int i = root.children.Count - 1; i >= 0; i--)
                stack.Push(root.children[i]);

            while (stack.Count > 0)
            {
                TreeViewItem current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null)
                {
                    for (int i = current.children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current.children[i]);
                    }
                }
            }
        }

        public static MultiColumnHeaderState CreateMultiColumnHeaderState()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    canSort = false,
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByType")),
                    contextMenuText = "Icon",
                    headerTextAlignment = TextAlignment.Center,
                    width = 30,
                    minWidth = 30,
                    maxWidth = 30,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Scene", $"Scenes belonging to project '{Application.AuthoringProject.Name}'"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 350,
                    minWidth = 60,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Load at Startup", "These scenes will be automatically loaded when the game boots"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 100,
                    minWidth = 100,
                    autoResize = true
                }
            };

            // Number of columns should match number of enum values: You probably forgot to update one of them
            Assertions.Assert.AreEqual(columns.Length, Enum.GetValues(typeof(Column)).Length);
            var state = new MultiColumnHeaderState(columns);

            return state;
        }

        public override void OnGUI(Rect rect)
        {
            if (ShouldReload)
            {
                Reload();
                ShouldReload = false;
            }

            base.OnGUI(rect);
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            if (item is SceneItem)
            {
                return 22.0f;
            }
            return 18.0f;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            // Draw background
            if (!args.selected)
            {
                var headerRect = args.rowRect;
                if (headerRect.Contains(Event.current.mousePosition))
                {
                    HierarchyGui.BackgroundColor(headerRect, BuildManifestColors.Table.Hover);
                    m_CurrentHoveredRect = headerRect;
                }
            }

            switch (args.item)
            {
                case SceneReferenceItem sceneRefItem:
                    for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                    {
                        DrawRowCell(args.GetCellRect(i), (Column)args.GetColumn(i), sceneRefItem, args);
                    }
                    break;
                case TreeViewItem treeItem:
                    for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                        DrawRowCell(args.GetCellRect(i), (Column)args.GetColumn(i), treeItem, args);
                    break;
            }

            base.RowGUI(args);
        }

        private void DrawRowCell(UnityEngine.Rect rect, Column column, TreeViewItem item, RowGUIArgs args)
        {
            if (null == item)
            {
                return;
            }

            CenterRectUsingSingleLineHeight(ref rect);
        }

        private void DrawRowCell(UnityEngine.Rect rect, Column column, SceneReferenceItem item, RowGUIArgs args)
        {
            if (null == item)
            {
                return;
            }

            CenterRectUsingSingleLineHeight(ref rect);

            switch (column)
            {
                case Column.Icon:
                {
                    GUI.DrawTexture(rect, Icons.Scene, ScaleMode.ScaleToFit);
                    break;
                }
                case Column.Scene:
                {
                    DefaultGUI.Label(rect, item.ScenePath, args.selected, args.focused);
                    break;
                }
                case Column.StartupToggle:
                {
                    var toggleRect = rect;
                    int toggleWidth = 20;
                    toggleRect.x += rect.width * 0.5f - toggleWidth * 0.5f;

                    bool toggleVal = EditorGUI.Toggle(toggleRect, item.IsStartup);
                    if (toggleVal != item.IsStartup)
                    {
                        item.IsStartup = toggleVal;

                        var project = Application.AuthoringProject;
                        var sceneReference = new SceneReference { SceneGuid = item.SceneGuid };
                        if (toggleVal)
                        {
                            project.AddStartupScene(sceneReference);
                        }
                        else
                        {
                            project.RemoveStartupScene(sceneReference);
                        }
                        Repaint();
                    }
                    break;
                }
            }
        }

        public TreeViewItem FindItem(int id)
        {
            return Bridge.TreeView.FindItem(id, rootItem);
        }

        protected override TreeViewItem BuildRoot()
        {
            var project = Application.AuthoringProject;
            var scenesArray = project.GetScenes();
            var startupScenes = project.GetStartupScenes();

            var root = new BuildManifestViewItem { id = int.MaxValue, depth = -1, displayName = "Root" };
            var persistenceManager = project.PersistenceManager;

            if (scenesArray.Length > 0)
            {
                for (int i = 0; i < scenesArray.Length; ++i)
                {
                    var sceneReference = scenesArray[i];
                    var sceneGuid = sceneReference.SceneGuid;
                    var scenePath = persistenceManager.GetSceneAssetPath(sceneGuid);
                    if (scenePath == null)
                    {
                        Debug.LogWarning($"Cannot find scene path for guid '{sceneGuid}' found in configuration.");
                        continue;
                    }

                    root.AddChild(new SceneReferenceItem(m_PersistenceManager, sceneGuid, startupScenes.Contains(sceneReference)));
                }
            }

            if (!root.hasChildren)
            {
                root.AddChild(new TreeViewItem(0, 0, "Project has no scenes. Drag scenes here to add them to the project"));
            }

            return root;
        }

        public void Dispose()
        {
            m_ChangeManager.UnregisterChangeCallback(HandleChanges);
            AssetPostprocessorCallbacks.UnregisterAssetMovedHandlerForType<SceneAsset>(HandleMovedAsset);
        }

        protected override void KeyEvent()
        {
            base.KeyEvent();
            if (UnityEngine.Event.current.type == UnityEngine.EventType.KeyDown && UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.Delete)
            {
                DeleteSelection();
                UnityEngine.Event.current.Use();
            }
        }

        public void DeleteSelection()
        {
            var selection = GetSceneSelection();

            Selection.instanceIDs = new int[0];

            var project = Application.AuthoringProject;
            foreach (var sceneGuid in selection)
            {
                project.RemoveScene(new SceneReference { SceneGuid = sceneGuid });
            }

            Invalidate();
        }

        private IEnumerable<Guid> GetSceneSelection()
        {
            return new List<Guid>(GetSelection()
                    .Select(id => FindItem(id, rootItem))
                    .OfType<SceneReferenceItem>()
                    .Select(i => i.SceneGuid));
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return args.draggedItem is EntityItem;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            var sortedDraggedIDs = SortItemIDsInRowOrder(args.draggedItemIDs);
            var objList = new List<UnityEngine.GameObject>(sortedDraggedIDs.Count);


            DragAndDrop.paths = new string[0];
            DragAndDrop.objectReferences = objList.Cast<UnityEngine.Object>().ToArray();
            DragAndDrop.StartDrag("Multiple");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (!args.performDrop)
            {
                return DragAndDropVisualMode.Move;
            }

            var draggedObjects = DragAndDrop.objectReferences;
            var sceneAssets = draggedObjects.Select(obj => obj as SceneAsset).Where(obj => obj != null).ToList();

            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                case DragAndDropPosition.BetweenItems:
                case DragAndDropPosition.OutsideItems:
                    return HandleDropScenes(sceneAssets);
                default: return DragAndDropVisualMode.Rejected;
            }
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            switch (item)
            {
                case SceneReferenceItem sceneItem:
                    ShowSceneReferenceContextMenu(sceneItem);
                    break;
            }
        }

        private void ShowSceneReferenceContextMenu(SceneReferenceItem item)
        {
            if (item.SceneGuid == SceneReference.Null.SceneGuid)
            {
                return;
            }

            var project = Application.AuthoringProject;
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent($"Remove Scene from {project.Name}"), false, () =>
            {
                project.RemoveScene(new SceneReference { SceneGuid = item.SceneGuid });
                Invalidate();
            });

            menu.ShowAsContext();
        }

        private DragAndDropVisualMode HandleDropScenes(List<SceneAsset> sceneAssets)
        {
            if (sceneAssets.Count <= 0)
            {
                return DragAndDropVisualMode.Rejected;
            }

            var project = Application.AuthoringProject;
            var sceneGuids = sceneAssets.Select(scene => Guid.Parse(scene.Guid)).ToList();
            foreach (var sceneGuid in sceneGuids)
            {
                project.AddScene(new SceneReference { SceneGuid = sceneGuid });
            }

            Invalidate();
            return DragAndDropVisualMode.Link;
        }
    }
}
