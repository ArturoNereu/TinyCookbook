using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Tiny.Scenes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Unity.Editor.Persistence;
using Unity.Authoring.ChangeTracking;
using Object = UnityEngine.Object;

namespace Unity.Editor
{
    internal class BuildManifestView : TreeView, IDisposable
    {
        private enum Column
        {
            Icon,
            SceneName,
            ScenePath,
            StartupToggle
        }

        private class SceneReferenceItem : TreeViewItem
        {
            public SceneReferenceItem(IPersistenceManager persistenceManager, Guid sceneGuid, bool isStartup)
            {
                IsStartup = isStartup;
                SceneGuid = sceneGuid;
                ScenePath = persistenceManager.GetSceneAssetPath(SceneGuid);
                SceneName = persistenceManager.GetSceneAssetName(SceneGuid);
                InstanceID = AssetDatabase.LoadAssetAtPath<Object>(ScenePath).GetInstanceID();
            }

            public bool IsStartup { get; set; }
            public Guid SceneGuid { get; }
            public string ScenePath { get; }
            public string SceneName { get; }
            public int InstanceID { get; }

            //displayName value will be used by default DoesItemMatchSearch method implementation.
            public override string displayName => ScenePath;
            public override int id => SceneGuid.GetHashCode();
        }

        private readonly IPersistenceManager m_PersistenceManager;
        private readonly IChangeManager m_ChangeManager;
        private readonly EntityManager m_EntityManager;
        private readonly IEditorSceneManagerInternal m_SceneManager;
        private readonly MultiColumnHeaderState m_MultiColumnHeaderState;
        private bool ShouldReload { get; set; }

        public void Invalidate()
        {
            ShouldReload = true;
        }

        public BuildManifestView(TreeViewState state, Project project) : base(state, new MultiColumnHeader(CreateMultiColumnHeaderState()))
        {
            useScrollView = true;
            m_PersistenceManager = project.Session.GetManager<IPersistenceManager>();
            m_SceneManager = project.Session.GetManager<IEditorSceneManagerInternal>();
            m_ChangeManager = project.Session.GetManager<IChangeManager>();
            m_ChangeManager.RegisterChangeCallback(HandleChanges);
            AssetPostprocessorCallbacks.RegisterAssetMovedHandlerForType<SceneAsset>(HandleMovedAsset);
            
            multiColumnHeader.sortingChanged += OnSortingChanged;
            multiColumnHeader.sortedColumnIndex = 1;
            showAlternatingRowBackgrounds = true;

            Reload();
            SortIfNeeded();
        }

        private void HandleChanges(Changes changes)
        {
            if (changes.ComponentsWereModified ||
                changes.ComponentsWereRemoved ||
                changes.ComponentsWereAdded ||
                changes.EntitiesWereDeleted)
            {
                Invalidate();
            }
        }

        private void HandleMovedAsset(SceneAsset scene, PostprocessEventArgs args)
        {
            Invalidate();
        }

        private void OnSortingChanged(MultiColumnHeader columnHeader)
        {
            SortIfNeeded();
        }

        void SortIfNeeded()
        {
            var rows = GetRows();
            if (rows == null || rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
            {
                return; // No column to sort for (just use the order the data are in)
            }

            SortColumn();
            TreeToList(rootItem, rows);
            Repaint();
        }

        void SortColumn()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            var items = rootItem.children.Cast<SceneReferenceItem>();
            var columnIndex = multiColumnHeader.sortedColumnIndex;
            var column = (Column)columnIndex;
            var ascending = multiColumnHeader.IsSortedAscending(columnIndex);
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

            var stack = new Stack<TreeViewItem>();
            for (var i = root.children.Count - 1; i >= 0; i--)
                stack.Push(root.children[i]);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null)
                {
                    for (var i = current.children.Count - 1; i >= 0; i--)
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
                    autoResize = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Scene", $"Scenes belonging to project '{Application.AuthoringProject.Name}'"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 120,
                    minWidth = 100,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Asset Path", $"Scenes belonging to project '{Application.AuthoringProject.Name}'"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 350,
                    minWidth = 100,
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
                    maxWidth = 100,
                    autoResize = false
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
                SortIfNeeded();
                ShouldReload = false;
            }

            multiColumnHeader.ResizeToFit();

            if (hasSearch && GetRows().Count == 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("No match for : " + searchString);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            base.OnGUI(rect);
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            return 18.0f;
        }

        protected override void RowGUI(RowGUIArgs args)
        {            
            if (null == args.item)
            {
                return;
            }

            switch (args.item)
            {
                case SceneReferenceItem sceneRefItem:
                    for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
                    {
                        DrawRowCell(args.GetCellRect(i), (Column)args.GetColumn(i), sceneRefItem, args);
                    }
                    return;
            }

            base.RowGUI(args);
        }

        private void DrawRowCell(Rect rect, Column column, SceneReferenceItem item, RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref rect);
            switch (column)
            {
                case Column.Icon:
                {
                    GUI.DrawTexture(rect, Icons.Scene, ScaleMode.ScaleToFit);
                    break;
                }
                case Column.SceneName:
                {
                    DefaultGUI.Label(rect, item.SceneName, args.selected, args.focused);
                    break;
                }
                
                case Column.ScenePath:
                {
                    DefaultGUI.Label(rect, item.ScenePath, args.selected, args.focused);
                    break;
                }
                case Column.StartupToggle:
                {
                    var toggleRect = rect;
                    toggleRect.width = 20;
                    toggleRect.x += rect.width * 0.5f - toggleRect.width * 0.5f;

                    var toggleVal = EditorGUI.Toggle(toggleRect, item.IsStartup);
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

        protected override TreeViewItem BuildRoot()
        {
            var project = Application.AuthoringProject;
            var scenesArray = project.GetScenes();
            var startupScenes = project.GetStartupScenes();

            var root = new TreeViewItem { id = int.MaxValue, depth = -1, displayName = "Root" };
            var persistenceManager = project.PersistenceManager;

            foreach (var sceneReference in scenesArray)
            {
                var sceneGuid = sceneReference.SceneGuid;
                var scenePath = persistenceManager.GetSceneAssetPath(sceneGuid);
                if (scenePath == null)
                {
                    Debug.LogWarning($"Cannot find scene path for guid '{sceneGuid}' found in configuration.");
                    continue;
                }

                root.AddChild(new SceneReferenceItem(m_PersistenceManager, sceneGuid, startupScenes.Contains(sceneReference)));
            }

            if (!root.hasChildren)
            {
                const string emptyTreeMessage = "Project has no scenes. Drag scenes here to add them to the project";
                root.AddChild(new TreeViewItem(0, 0, emptyTreeMessage));
            }

            return root;
        }

        public void Dispose()
        {
            m_ChangeManager.UnregisterChangeCallback(HandleChanges);
            AssetPostprocessorCallbacks.UnregisterAssetMovedHandlerForType<SceneAsset>(HandleMovedAsset);
        }
        
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var selectedInstanceIDs = selectedIds.Select(id => FindItem(id, rootItem))
                .OfType<SceneReferenceItem>()
                .Select(item => item.InstanceID)
                .ToArray();
           
            Selection.instanceIDs = selectedInstanceIDs;
            base.SelectionChanged(selectedIds);
        }

        protected override void DoubleClickedItem(int id)
        {
            if (FindItem(id, rootItem) is SceneReferenceItem sceneReferenceItem)
            {
                m_SceneManager.LoadScene(sceneReferenceItem.ScenePath);
            }
            
            base.DoubleClickedItem(id);
        }

        protected override void KeyEvent()
        {
            base.KeyEvent();
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
            {
                DeleteSelection();
                Event.current.Use();
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
                    .Where(item => item.SceneGuid != SceneReference.Null.SceneGuid)
                    .Select(i => i.SceneGuid));
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
                case SceneReferenceItem _:
                    ShowSceneReferenceContextMenu();
                    break;
            }
        }

        private void ShowSceneReferenceContextMenu()
        {

            var project = Application.AuthoringProject;
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent($"Remove Scene from {project.Name}"), false, () =>
            {
                DeleteSelection();
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
