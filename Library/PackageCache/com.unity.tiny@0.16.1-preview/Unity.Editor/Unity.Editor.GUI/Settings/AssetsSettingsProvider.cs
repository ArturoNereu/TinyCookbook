using JetBrains.Annotations;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Authoring.ChangeTracking;
using Unity.Editor.Assets;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal class AssetsSettingsProvider : BaseSettingsProvider
    {
        private AssetTreeView m_TreeView;
        private readonly SearchField m_SearchField = new SearchField();
        private IMGUIContainer m_TreeViewContainer;
        private string m_Filter;

        private AssetsSettingsProvider() : base("Project/DOTS/Assets", SettingsScope.Project)
        {
        }

        [SettingsProvider, UsedImplicitly]
        private static SettingsProvider Provider()
        {
            return new AssetsSettingsProvider { label = "Assets" };
        }

        protected override void OnBeginAuthoring(Project project)
        {
            m_TreeView = new AssetTreeView(Project, new TreeViewState());
            m_TreeView.Reload();
        }

        protected override void OnEndAuthoring(Project project)
        {
            m_TreeView = null;
        }

        protected override VisualElement CreateSettingsGUI(string searchContext)
        {
            m_TreeViewContainer = new IMGUIContainer(DrawTreeView);
            
            var settingsUI =  new VisualElement();
            settingsUI.Add(new IMGUIContainer(DrawFilter));
            
            settingsUI.Add(m_TreeViewContainer);
            return settingsUI;
        }

        private void DrawFilter()
        {
            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5);
                m_Filter = m_SearchField.OnToolbarGUI(m_Filter);
                GUILayout.Space(4);
            }

            if (EditorGUI.EndChangeCheck())
            {
                OnFilterChanged();
            }
        }

        private void OnFilterChanged() => m_TreeView.Filter(m_Filter);

        private void DrawTreeView()
        {
            const int space = 5;
            GUILayout.Space(space);
            var startPosition = m_TreeViewContainer.localBound.y + m_TreeViewContainer.parent.localBound.y + space;
            var height = Mathf.Min(LocalBound.height - startPosition, m_TreeView.totalHeight); 
            m_TreeView.OnGUI(GUILayoutUtility.GetRect(0, height));
        }

        private class AssetTreeView : TreeView
        {
            private readonly Project m_Project;
            private readonly IChangeManager m_ChangeManager;
            private string m_Filter;

            public AssetTreeView(Project project, TreeViewState state) : base(state)
            {
                m_Project = project;
                showAlternatingRowBackgrounds = true;

                m_ChangeManager = project.Session.GetManager<IChangeManager>();
                m_ChangeManager.RegisterChangeCallback(OnChangeDetected);
            }

            private void OnChangeDetected(Changes changes)
            {
                if(changes.EntitiesWereCreated 
                    || changes.EntitiesWereDeleted
                    || changes.ComponentsWereAdded 
                    || changes.ComponentsWereModified 
                    || changes.ComponentsWereRemoved )
                {
                    Reload();
                }
            }

            protected override TreeViewItem BuildRoot()
            {
                var root = new TreeViewItem(0, -1, "Root");

                foreach (var asset in AssetEnumerator.GetAllReferencedAssets(m_Project)
                                                     .Where(x => x.Parent == null && MatchFilter(x))
                                                     .OrderBy(x => x.Name))
                {
                    root.AddChild(new AssetItem(asset));
                }

                if (!root.hasChildren)
                {
                    if (!string.IsNullOrEmpty(m_Filter))
                    {
                        root.AddChild(new TreeViewItem { displayName = "no match for : " + m_Filter });
                    }
                    else
                    {
                        root.AddChild(new TreeViewItem { displayName = "no assets to display" });
                    }
                }

                return root;
            }

            private bool MatchFilter(AssetInfo assetInfo)
                => string.IsNullOrEmpty(m_Filter)
                || CultureInfo.InvariantCulture.CompareInfo.IndexOf(assetInfo.Name, m_Filter, CompareOptions.OrdinalIgnoreCase) >= 0;

            protected override void SelectionChanged(IList<int> selectedIds)
            {
                Selection.instanceIDs = selectedIds.ToArray();

                base.SelectionChanged(selectedIds);
            }

            internal void Filter(string filter)
            {
                m_Filter = filter;
                Reload();
            }

            internal class AssetItem : TreeViewItem
            {
                private readonly AssetInfo m_AssetInfo;

                public AssetItem(AssetInfo assetInfo)
                {
                    m_AssetInfo = assetInfo;
                    foreach (var child in assetInfo.Children)
                    {
                        AddChild(new AssetItem(child));
                    }
                }

                public override int depth => parent?.depth + 1 ?? -1;

                public override int id => m_AssetInfo.Object.GetInstanceID();

                public override Texture2D icon => (Texture2D)EditorGUIUtility.ObjectContent(m_AssetInfo.Object, m_AssetInfo.Object.GetType()).image;

                public override string displayName => m_AssetInfo.Name;
            }
        }

    }
}
