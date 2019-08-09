using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Authoring;
using Unity.Collections;
using Unity.Editor.Bindings;
using Unity.Editor.MenuItems;
using Unity.Entities;
using Unity.Tiny.Scenes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

using DropOnItemAction = System.Action<Unity.Editor.Hierarchy.HierarchyItem, System.Collections.Generic.List<Unity.Editor.ISceneGraphNode>>;
using DropBetweenAction = System.Action<Unity.Editor.Hierarchy.HierarchyItem, System.Collections.Generic.List<Unity.Editor.ISceneGraphNode>, int>;

namespace Unity.Editor.Hierarchy
{
    internal class EntityHierarchyTree : TreeView, IDisposable
    {
        internal struct Key{}
        
        private readonly Session m_Session;
        private readonly IEditorSceneManagerInternal m_SceneManager;
        private readonly EntityManager m_EntityManager;
        private readonly Dictionary<SceneGuid, TreeViewItem> m_CachedSceneItems;
        private readonly UnityComponentCacheManager m_ComponentCache;
        private string m_FilterString = string.Empty;
        private readonly Dictionary<System.Type, DropOnItemAction> m_DroppedOnMethod;
        private readonly Dictionary<System.Type, DropBetweenAction> m_DroppedBetweenMethod;

        private Rect m_CurrentHoveredRect;
        private Scene ActiveScene => m_SceneManager?.GetActiveScene() ?? Scene.Null;
        private List<int> IdsToExpand { get; }

        internal Rect CurrentHoverRect => m_CurrentHoveredRect;
        private HierarchySearchFilter m_HierarchySearchFilter;
        internal bool ShouldReload { get; private set; }

        public string FilterString
        {
            get
            {
                return m_FilterString;
            }
            set
            {
                if (m_FilterString != value)
                {
                    m_FilterString = value;
                    m_HierarchySearchFilter.Dispose();
                    m_HierarchySearchFilter = FilterUtility.CreateHierarchyFilter(m_EntityManager, m_FilterString);
                    Invalidate();
                }
            }
        }

        private bool IsSearching => !string.IsNullOrEmpty(m_FilterString);

        public void Invalidate()
        {
            ShouldReload = true;
        }

        public EntityHierarchyTree(Session session, TreeViewState state) : base(state)
        {
            useScrollView = true;
            m_Session = session;
            m_EntityManager = session.GetManager<IWorldManager>().EntityManager;
            m_CachedSceneItems = new Dictionary<SceneGuid, TreeViewItem>();
            m_SceneManager = m_Session.GetManager<IEditorSceneManagerInternal>();
            m_ComponentCache = m_Session.GetManager<UnityComponentCacheManager>();
            m_HierarchySearchFilter = FilterUtility.CreateHierarchyFilter(m_EntityManager, string.Empty);
            foldoutOverride = HandleFoldout;

            m_DroppedOnMethod = new Dictionary<System.Type, DropOnItemAction>
            {
                { typeof(SceneItem), DropUponSceneItem },
                { typeof(EntityItem), DropUponEntityItem },
            };

            m_DroppedBetweenMethod = new Dictionary<System.Type, DropBetweenAction>
            {
                { typeof(HierarchyItem), DropBetweenSceneItems },
                { typeof(SceneItem), DropBetweenRootEntities },
                { typeof(EntityItem), DropBetweenChildrenEntities },
            };

            IdsToExpand = new List<int>();
            Reload();
        }

        private float CurrentFoldoutHeight { get; set; }

        private bool HandleFoldout(Rect position, bool expandedState, GUIStyle style)
        {
            position.height = CurrentFoldoutHeight;
            CenterRectUsingSingleLineHeight(ref position);
            return EditorGUI.Foldout(position, expandedState, GUIContent.none);
        }

        public override void OnGUI(Rect rect)
        {
            if (ShouldReload)
            {
                Reload();
            }

            if (IdsToExpand.Count > 0)
            {
                ForceExpanded(IdsToExpand);
                IdsToExpand.Clear();
            }
            base.OnGUI(rect);
        }

        protected override void KeyEvent()
        {
            base.KeyEvent();
            if (UnityEngine.Event.current.type == UnityEngine.EventType.KeyDown)
            {
                if (UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.Delete)
                {
                    DeleteSelection();
                    UnityEngine.Event.current.Use();
                }
            }
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
            var itemRect = args.rowRect;
            CurrentFoldoutHeight = itemRect.height;
            switch (args.item)
            {
                case SceneItem sceneItem:
                    DrawItem(itemRect, sceneItem, args);
                    return;
                case EntityItem treeItem:
                    DrawItem(itemRect, treeItem, args);
                    return;
            }

            base.RowGUI(args);
        }

        private void DrawItem(UnityEngine.Rect rect, SceneItem item, RowGUIArgs args)
        {
            if (null == item)
            {
                return;
            }

            // Draw scene separators and background
            var indent = GetContentIndent(item);
            if (!args.selected)
            {
                var headerRect = rect;
                headerRect.width += 1;

                var topLine = headerRect;
                topLine.height = 1;
                HierarchyGui.BackgroundColor(topLine, HierarchyColors.Hierarchy.SceneSeparator);

                headerRect.y += 2;
                headerRect.height -= 2;
                HierarchyGui.BackgroundColor(headerRect, HierarchyColors.Hierarchy.SceneItem);
                if (rect.Contains(Event.current.mousePosition))
                {
                    HierarchyGui.BackgroundColor(headerRect, HierarchyColors.Hierarchy.Hover);
                    if (Event.current.type == EventType.Layout)
                    {
                        m_CurrentHoveredRect = rect;
                    }
                }

                var bottomLine = headerRect;
                bottomLine.y += bottomLine.height - 2;
                bottomLine.height = 1;
                HierarchyGui.BackgroundColor(bottomLine, HierarchyColors.Hierarchy.SceneSeparator);
            }

            // Draw scene icon
            rect.y += 2;
            rect.x = indent;
            rect.width -= indent;

            var iconRect = rect;
            iconRect.width = 20;

            var image = ActiveScene == item.Scene ? Icons.ActiveScene : Icons.Scene;
            EditorGUI.LabelField(iconRect, new UnityEngine.GUIContent { image = image });

            // Draw scene label
            rect.x += 20;
            rect.width -= 40;

            var style = ActiveScene == item.Scene ? EditorStyles.boldLabel : UnityEngine.GUI.skin.label;
            var label = item.displayName;
            var project = Application.AuthoringProject;
            if (!project.GetScenes().Contains(new SceneReference { SceneGuid = item.Scene.SceneGuid.Guid }))
            {
                label += $" (Not in {project.Name})";
            }
            EditorGUI.LabelField(rect, label, style);

            // Draw scene context menu button
            rect.x += rect.width;
            rect.width = 16;
            if (UnityEngine.GUI.Button(rect, Icons.Settings, GUI.skin.label))
            {
                ShowSceneContextMenu(item);
            }
        }

        private void DrawItem(UnityEngine.Rect rect, EntityItem item, RowGUIArgs args)
        {
            using (new GuiColorScope(item.Node.EnabledInHierarchy ? UnityEngine.Color.white : HierarchyColors.Hierarchy.Disabled))
            {
                if (!args.selected && rect.Contains(Event.current.mousePosition))
                {
                    HierarchyGui.BackgroundColor(rect, HierarchyColors.Hierarchy.Hover);
                    if (Event.current.type == EventType.Layout)
                    {
                        m_CurrentHoveredRect = rect;
                    }
                }

                CenterRectUsingSingleLineHeight(ref args.rowRect);
                base.RowGUI(args);
            }
        }

        private void ForceExpanded(IEnumerable<int> ids)
        {
            foreach (var id in ids)
            {
                foreach (var ancestorId in GetAncestors(id))
                {
                    SetExpanded(ancestorId, true);
                }
                SetExpanded(id, true);
            }
        }

        public TreeViewItem FindItem(int id)
        {
            return Bridge.TreeView.FindItem(id, rootItem);
        }

        public void TransferSelection(List<Guid> guids)
        {
            var references = guids.Select(g => m_ComponentCache.GetEntityReference(g)).Where(r => r && null != r).ToList();
            Selection.instanceIDs = references.Select(r => r.gameObject.GetInstanceID()).ToArray();
            IdsToExpand.AddRange(references
                .Select(e => e.transform.parent)
                .Where(p => p && null != p)
                .Select(p => p.gameObject.GetInstanceID()));
        }

        public void TransferSelection(List<SceneGuid> guids)
        {
            SetSelection(guids.Select(g => g.Guid.GetHashCode()).ToList());
        }

        public struct SearchingScope : IDisposable
        {
            public bool IsSearching;
            public NativeHashMap<Entity, int> Map;

            public SearchingScope(bool searching, HierarchySearchFilter filter)
            {
                IsSearching = searching;
                if (IsSearching)
                {
                    Map = filter.ToResult(Allocator.TempJob);
                }
                else
                {
                    Map = new NativeHashMap<Entity, int>(0, Allocator.TempJob);
                }
            }

            public void Dispose()
            {
                Map.Dispose();
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            // TODO:Remove
            m_CachedSceneItems.Clear();
            using (var searching = new SearchingScope(IsSearching, m_HierarchySearchFilter))
            {
                var root = new HierarchyItem {id = int.MaxValue, depth = -1, displayName = "Root"};
                for (var i = 0; i < m_SceneManager.LoadedSceneCount; ++i)
                {
                    var scene = m_SceneManager.GetLoadedSceneAtIndex(i);
                    if (m_CachedSceneItems.TryGetValue(scene.SceneGuid, out var item))
                    {
                        root.AddChild(item);
                    }
                    else
                    {
                        root.AddChild(LoadScene(scene, searching));
                    }
                }

                if (m_SceneManager.LoadedSceneCount == 0)
                {
                    root.AddChild(new TreeViewItem(0, 0, "No scene loaded"));
                }

                ShouldReload = false;
                return root;
            }
        }

        public void Dispose()
        {
            m_HierarchySearchFilter.Dispose();
        }

        private IEnumerable<EntityNode> GetEntitySelection()
        {
            return new List<EntityNode>(GetSelection()
                    .Select(id => FindItem(id, rootItem))
                    .OfType<EntityItem>()
                    .Select(i => i.Node));
        }

        public void DeleteSelection()
        {
            var selection = GetEntitySelection();

            Selection.instanceIDs = new int[0];

            foreach (var node in selection)
            {
                node.Graph.Delete(node);
            }

            Repaint();
        }

        public void DuplicateSelection()
        {
            using (var pooled = ListPool<ISceneGraphNode>.GetDisposable())
            {
                var toSelect = pooled.List;
                var selection = GetEntitySelection();
                foreach (var group in selection.GroupBy(
                    e => m_EntityManager.GetSharedComponentData<SceneGuid>(e.Entity)))
                {
                    var graph = m_SceneManager.GetGraphForScene(group.Key);
                    var list = group.Cast<ISceneGraphNode>().ToList();
                    toSelect.AddRange(graph.Duplicate(list));
                }
                EntityHierarchyWindow.SelectOnNextPaint(toSelect.OfType<EntityNode>().Select(e => e.Guid).ToList());
            }
        }

        public void Rename(Guid guid)
        {
            var reference = m_ComponentCache.GetEntityReference(guid);
            if (null == reference || !reference)
            {
                return;
            }

            var item = FindItem(reference.gameObject.GetInstanceID(), rootItem);
            BeginRename(item);
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return item is EntityItem;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (!args.acceptedRename || string.IsNullOrEmpty(args.newName))
            {
                return;
            }

            var item = FindItem(args.itemID, rootItem);

            switch (item)
            {
                case EntityItem entityItem:
                {
                    entityItem.displayName = args.newName;
                }
                break;
            }
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
            foreach (var id in sortedDraggedIDs)
            {
                if (FindItem(id, rootItem) is EntityItem item)
                {
                    objList.Add(m_ComponentCache.GetEntityReference(item.Guid).gameObject);
                }
            }

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

            var dragAndDropResult = DragAndDropVisualMode.Rejected;
            foreach (var draggedObject in draggedObjects)
            {
                if (draggedObject is SceneAsset sceneAsset)
                {
                    dragAndDropResult = HandleDropSceneOutsideOfItems(sceneAsset);
                }
            }
            
            if (dragAndDropResult == DragAndDropVisualMode.Rejected)
            {
                var offset = 0;
                foreach (var t in draggedObjects)
                {
                    var created = HandleSingleObjectDrop(args, m_Session, t, offset);
                    if (created == DragAndDropVisualMode.Link)
                    {
                        ++offset;
                        dragAndDropResult = DragAndDropVisualMode.Link;
                    }
                }
            }
            
            if (dragAndDropResult == DragAndDropVisualMode.Rejected)
            {
                var nodes = draggedObjects
                    .Select(d => FindItem(d.GetInstanceID(), rootItem))
                    .OfType<EntityItem>()
                    .Select(item => item.Node as ISceneGraphNode)
                    .ToList();

                switch (args.dragAndDropPosition)
                {
                    case DragAndDropPosition.UponItem:
                        dragAndDropResult = HandleDropUponItem(nodes, (HierarchyItem)args.parentItem);
                        break;
                    case DragAndDropPosition.BetweenItems:
                        dragAndDropResult = HandleDropBetweenItems(nodes, (HierarchyItem)args.parentItem, args.insertAtIndex);
                        break;
                    case DragAndDropPosition.OutsideItems:
                        dragAndDropResult = HandleDropOutsideOfItems(nodes);
                        break;
                    default:
                        break;
                }
            }

            return dragAndDropResult;
        }
        
        private DragAndDropVisualMode HandleSingleObjectDrop(DragAndDropArgs args, Session session, UnityEngine.Object o, int offset)
        {
            return (DragAndDropVisualMode) typeof(EntityHierarchyTree)
                .GetMethod(nameof(DoHandleSingleObjectDrop), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(o.GetType()).Invoke(this, new object[] {args, o, offset});
        }

        private DragAndDropVisualMode DoHandleSingleObjectDrop<TObject>(DragAndDropArgs args, TObject o, int offset)
        {
            var handler = HierarchyDragAndDrop<TObject, Key>.SingleObjectDrop;
            if (null == handler)
            {
                return DragAndDropVisualMode.Rejected;
            }

            var item = GetDragAndDropItem(args);

            var result = DragAndDropVisualMode.Rejected;
            ISceneGraphNode resultNode = null;
            if (item is SceneItem sceneItem)
            {
                if (null != (resultNode = handler(m_Session, o, sceneItem.Graph, null,
                        args.dragAndDropPosition == DragAndDropPosition.BetweenItems ? args.insertAtIndex : -1)))
                {
                    IdsToExpand.Add(sceneItem.id);
                    result = DragAndDropVisualMode.Link;
                }
            }

            if (item is EntityItem entityItem)
            {
                var node = entityItem.Node;
                if (null != (resultNode = handler(m_Session, o, node.Graph as SceneGraph, node,
                    args.dragAndDropPosition == DragAndDropPosition.BetweenItems ? args.insertAtIndex : -1)))
                {
                    IdsToExpand.Add(entityItem.id);
                    result = DragAndDropVisualMode.Link;
                }
            }
            
            if (null == resultNode)
            {
                return DragAndDropVisualMode.Rejected;
            }
            
            if (result == DragAndDropVisualMode.Link)
            {
                if (resultNode is EntityNode entityNode)
                {
                    EntityHierarchyWindow.SelectOnNextPaint(entityNode.Guid);
                }
            }
            
            return result;
        }

        private HierarchyItem GetDragAndDropItem(DragAndDropArgs args)
        {
            var parent = args.parentItem as HierarchyItem;
            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                    return parent;
                case DragAndDropPosition.BetweenItems:
                    return parent;
                case DragAndDropPosition.OutsideItems:
                    return rootItem.children[rootItem.children.Count - 1] as HierarchyItem;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem);

            switch (item)
            {
                case SceneItem sceneItem:
                    ShowSceneContextMenu(sceneItem);
                    break;
                case EntityItem entityItem:
                    ShowEntityContextMenu(entityItem);
                    break;
            }

            //ContextClickedWithId = true;
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id); 
            if (item is EntityItem && null != SceneView.lastActiveSceneView)
            {
                SceneView.lastActiveSceneView.FrameSelected();
                return;
            }

            if (item is SceneItem sceneItem)
            {
                m_SceneManager.SetActiveScene(sceneItem.Scene);
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            Selection.instanceIDs = selectedIds.ToArray();
        }

        private TreeViewItem LoadScene(Scene scene, SearchingScope search)
        {
            var graph = m_SceneManager.GetGraphForScene(scene);
            var item = new SceneItem(m_Session, graph);

            foreach (var node in graph.Roots)
            {
                BuildFromNode(node, item, search);
            }

            m_CachedSceneItems[scene.SceneGuid] = item;
            return item;
        }

        private void BuildFromNode(ISceneGraphNode node, TreeViewItem parentItem, SearchingScope search)
        {
            TreeViewItem item = null;
            switch (node)
            {
                case EntityNode entityNode:
                {
                    item = new EntityItem(m_Session, entityNode.Entity, entityNode.Guid, entityNode);

                    if (search.IsSearching)
                    {
                        if (search.Map.TryGetValue(entityNode.Entity, out _))
                        {
                            parentItem.AddChild(item);
                        }
                    }
                    else
                    {
                        parentItem.AddChild(item);
                    }
                }
                break;
            }

            if (null == item)
            {
                return;
            }

            foreach (var child in node.Children)
            {
                BuildFromNode(child, string.IsNullOrEmpty(m_FilterString) ? item : parentItem, search);
            }
        }

        public void ShowSceneContextMenu(SceneItem item)
        {
            if (item.Scene == Scene.Null)
            {
                return;
            }

            var project = Application.AuthoringProject;
            var scenes = project.GetScenes();

            var menu = new GenericMenu();
            if (m_SceneManager.GetActiveScene() == item.Scene)
            {
                menu.AddDisabledItem(new GUIContent("Set as Active Scene"), true);
            }
            else
            {
                menu.AddItem(new GUIContent("Set as Active Scene"), false, () =>
                {
                    m_SceneManager.SetActiveScene(item.Scene);
                });
            }
            
            menu.AddItem(new GUIContent("Unload Scene"), false, () =>
            {
                m_SceneManager.UnloadSceneWithDialog(item.Scene);
            });
            
            menu.AddItem(new GUIContent("Unload Other Scenes"), false, () =>
            {
                using (var scenesToUnload = ListPool<Scene>.GetDisposable())
                {
                    for (var i = 0; i < m_SceneManager.LoadedSceneCount; ++i)
                    {
                        var scene = m_SceneManager.GetLoadedSceneAtIndex(i);
                        if (scene != item.Scene)
                        {
                            scenesToUnload.List.Add(scene);
                        }
                    }
                    foreach (var scene in scenesToUnload.List)
                    {
                        m_SceneManager.UnloadSceneWithDialog(scene);
                    }
                }
            });
            
            menu.AddSeparator("");

            if (m_SceneManager.GetLoadedSceneAtIndex(0) == item.Scene)
            {
                menu.AddDisabledItem(new GUIContent("Move Up"));
            }
            else
            {
                menu.AddItem(new GUIContent("Move Up"), false, () => { m_SceneManager.MoveSceneUp(item.Scene); });
            }
            
            if (m_SceneManager.GetLoadedSceneAtIndex(m_SceneManager.LoadedSceneCount - 1) == item.Scene)
            {
                menu.AddDisabledItem(new GUIContent("Move Down"));
            }
            else
            {
                menu.AddItem(new GUIContent("Move Down"), false, () => { m_SceneManager.MoveSceneDown(item.Scene); });
            }
            
            menu.AddSeparator("");

            var sceneReference = new SceneReference { SceneGuid = item.Scene.SceneGuid.Guid };
            if (scenes.Contains(sceneReference))
            {
                var startupScenes = project.GetStartupScenes();
                menu.AddItem(new GUIContent($"Remove Scene from {project.Name}"), false, () =>
                {
                    project.RemoveScene(sceneReference);
                });

                if (startupScenes.Contains(sceneReference))
                {
                    menu.AddItem(new GUIContent($"Remove Scene from {project.Name} Startup Scenes"), false, () =>
                    {
                        project.RemoveStartupScene(sceneReference);
                    });
                }
                else
                {
                    menu.AddItem(new GUIContent($"Add Scene to {project.Name} Startup Scenes"), false, () =>
                    {
                        project.AddStartupScene(sceneReference);
                    });
                }
            }
            else
            {
                menu.AddItem(new GUIContent($"Add Scene to {project.Name}"), false, () =>
                {
                    project.AddScene(sceneReference);
                });

                menu.AddDisabledItem(new GUIContent($"Add Scene to {project.Name} Startup Scenes"));
            }

            menu.ShowAsContext();
        }

        public void ShowEntityContextMenu(EntityItem item)
        {
            var menu = new GenericMenu();

            var selection = GetEntitySelection().ToList();
            var hasSelection = selection.Count > 0;

            if (null != item && item.Entity != Entity.Null)
            {
                menu.AddItem(new GUIContent("Rename"), false, () =>
                {
                    Rename(item.Guid);
                });
            }

            if (hasSelection)
            {
                menu.AddItem(new GUIContent("Duplicate"), false, DuplicateSelection);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Duplicate"));
            }

            if (hasSelection)
            {
                menu.AddItem(new GUIContent("Delete"), false, DeleteSelection);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Delete"));
            }

            menu.AddSeparator("");

            //            PopulateEntityTemplate(menu, tree.GetRegistryObjectSelection());
            menu.AddItem(new GUIContent("Empty Entity"), false, () => EntityMenuItems.CreateEmpty());
            menu.AddItem(new GUIContent("Empty Child Entity"), false, () => EntityMenuItems.CreateEmptyChild());
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Audio Source"), false, () => EntityMenuItems.AudioSource(null));
            menu.AddItem(new GUIContent("Camera"), false, () => EntityMenuItems.Camera(null));
            menu.AddItem(new GUIContent("Sprite"), false, () => EntityMenuItems.Sprite(null));
            menu.AddItem(new GUIContent("Canvas"), false, () => EntityMenuItems.UICanvas(null));


            menu.ShowAsContext();
        }


        private IEnumerable<int> AsInstanceIds(IEnumerable<ISceneGraphNode> nodes)
        {
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case EntityNode entityNode:
                    {
                        var reference = m_ComponentCache.GetEntityReference(entityNode.Guid);
                        if (!reference || null == reference)
                        {
                            continue;
                        }

                        yield return reference.gameObject.GetInstanceID();
                    }
                    break;
                }
            }
        }

        private void SetSelectionAndExpandedState(List<ISceneGraphNode> nodes)
        {
            var ids = AsInstanceIds(nodes.Select(n => n.Parent))
                .Concat(nodes.Where(n => null == n.Parent).Select(n => n.Graph).OfType<SceneGraph>().Select(g => g.Scene.SceneGuid.GetHashCode()))
                .Distinct()
                .ToArray();
            IdsToExpand.AddRange(ids);
            Selection.instanceIDs = AsInstanceIds(nodes).ToArray();
        }

        private DragAndDropVisualMode HandleDropUponItem(List<ISceneGraphNode> nodes, HierarchyItem parentItem)
        {
            if (m_DroppedOnMethod.TryGetValue(parentItem.GetType(), out var method))
            {
                method(parentItem, nodes);
                SetSelectionAndExpandedState(nodes);
                return DragAndDropVisualMode.Link;
            }
            return DragAndDropVisualMode.Rejected;
        }

        private DragAndDropVisualMode HandleDropBetweenItems(List<ISceneGraphNode> nodes, HierarchyItem parentItem, int insertAtIndex)
        {
            if (m_DroppedBetweenMethod.TryGetValue(parentItem.GetType(), out var method))
            {
                method(parentItem, nodes, insertAtIndex);
                SetSelectionAndExpandedState(nodes);
                return DragAndDropVisualMode.Link;
            }
            return DragAndDropVisualMode.Rejected;
        }

        internal void TryHandleDragAndDropFromExternalSource()
        {
            var dragAndDropResult = DragAndDropVisualMode.Rejected;
            
            using (var pooled = ListPool<SceneAsset>.GetDisposable())
            {
                pooled.List.AddRange(DragAndDrop.objectReferences.OfType<SceneAsset>());
                foreach (var scene in pooled.List)
                {
                    dragAndDropResult = HandleDropSceneOutsideOfItems(scene);
                }

                if (pooled.List.Count > 0)
                {
                    return;
                }
            }
            
            if (dragAndDropResult == DragAndDropVisualMode.Rejected)
            {

                var offset = 0;
                foreach (var t in DragAndDrop.objectReferences)
                {
                    var created = HandleSingleObjectDrop(new DragAndDropArgs
                    {
                        dragAndDropPosition = DragAndDropPosition.OutsideItems,
                    }, m_Session, t, offset);
                    if (created == DragAndDropVisualMode.Link)
                    {
                        ++offset;
                        dragAndDropResult = DragAndDropVisualMode.Link;
                    }
                }
            }

            if (dragAndDropResult == DragAndDropVisualMode.Rejected)
            {
                var nodes = DragAndDrop.objectReferences
                    .Select(d => FindItem(d.GetInstanceID(), rootItem))
                    .OfType<EntityItem>()
                    .Select(item => item.Node as ISceneGraphNode)
                    .ToList();

                if (nodes.Count > 0)
                {
                    HandleDropOutsideOfItems(nodes);
                }
            }
        }
        
        private DragAndDropVisualMode HandleDropOutsideOfItems(List<ISceneGraphNode> nodes)
        {
            var graph = m_SceneManager.GetGraphForScene(m_SceneManager.GetLoadedSceneAtIndex(m_SceneManager.LoadedSceneCount - 1).SceneGuid);
            graph.Add(nodes);
            SetSelectionAndExpandedState(nodes);
            return DragAndDropVisualMode.Link;
        }

        private void DropUponSceneItem(HierarchyItem parent, List<ISceneGraphNode> nodes)
        {
            SceneGuid guid;
            switch (parent)
            {
                case EntityItem entityItem:
                {
                    guid = m_EntityManager.GetSharedComponentData<SceneGuid>(entityItem.Entity);
                    break;
                }
                case SceneItem sceneItem:
                {
                    guid = sceneItem.Scene.SceneGuid;
                    break;
                }
            }

            var graph = m_SceneManager.GetGraphForScene(guid);
            Assert.IsNotNull(graph);

            foreach (var node in nodes)
            {
                graph.Add(node);
            }
            SetSelectionAndExpandedState(nodes);
        }

        private void DropUponEntityItem(HierarchyItem parent, List<ISceneGraphNode> nodes)
        {
            var item = parent as EntityItem;
            var parentNode = item.Node;
            var graph = m_SceneManager.GetGraphForScene(m_EntityManager.GetSharedComponentData<SceneGuid>(item.Entity));
            graph.Insert(-1, nodes, parentNode);
            SetSelectionAndExpandedState(nodes);
        }

        private void DropBetweenSceneItems(HierarchyItem parent, List<ISceneGraphNode> nodes, int insertAtIndex)
        {
            // Can't add entities before the first group.
            if (insertAtIndex <= 0)
            {
                return;
            }

            var graph = m_SceneManager.GetGraphForScene((parent.children[insertAtIndex - 1] as SceneItem).Scene.SceneGuid);
            graph.Add(nodes);
            SetSelectionAndExpandedState(nodes);
        }

        private void DropBetweenRootEntities(HierarchyItem parent, List<ISceneGraphNode> nodes, int insertAtIndex)
        {
            var item = parent as SceneItem;
            var graph = m_SceneManager.GetGraphForScene(item.Scene.SceneGuid);

            var firstIndex = insertAtIndex;
            foreach (var node in nodes)
            {
                if (graph.IsRoot(node) && node.SiblingIndex() < firstIndex)
                {
                    firstIndex -= 1;
                }

                graph.Insert(firstIndex++, node);
            }
            SetSelectionAndExpandedState(nodes);
        }

        private void DropBetweenChildrenEntities(HierarchyItem parent, List<ISceneGraphNode> nodes, int insertAtIndex)
        {
            var entityNode = (parent as EntityItem).Node;
            var firstIndex = insertAtIndex;
            foreach (var node in nodes)
            {
                if (node.IsChildOf(entityNode) && node.SiblingIndex() < firstIndex)
                {
                    firstIndex -= 1;
                }

                entityNode.Insert(firstIndex++, node);
            }
            SetSelectionAndExpandedState(nodes);
        }

        private DragAndDropVisualMode HandleDropSceneOutsideOfItems(SceneAsset sceneAsset)
        {
            var assetPath = AssetDatabase.GetAssetPath(sceneAsset);
            m_SceneManager.LoadScene(assetPath);
            return DragAndDropVisualMode.Link;
        }
    }
}
