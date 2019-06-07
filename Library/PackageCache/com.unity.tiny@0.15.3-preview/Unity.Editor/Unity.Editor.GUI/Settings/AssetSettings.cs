using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Authoring.ChangeTracking;
using Unity.Editor.Assets;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unity.Editor
{
    [InitializeOnLoad]
    internal class AssetSettings : SettingsProvider
    {
        private static AssetTreeView m_TreeView;
        private static Project m_Project;

        private readonly SearchField m_SearchField = new SearchField();
        private Vector2 m_ScrollPosition = Vector2.zero;
        private string m_Filter;

        static AssetSettings()
        {
            Application.BeginAuthoringProject += OnBeginAuthoringProject;
            Application.EndAuthoringProject += OnEndAuthoringProject;
        }

        private static void OnBeginAuthoringProject(Project project)
        {
            m_Project = project;
            m_TreeView = new AssetTreeView(m_Project, new TreeViewState());
            m_TreeView.Reload();
        }

        private static void OnEndAuthoringProject(Project _)
        {
            m_Project = null;
            m_TreeView = null;
        }

        public AssetSettings() : base("Project/DOTS/Assets", SettingsScope.Project)
        {
        }

        [SettingsProvider]
        [UsedImplicitly]
        public static SettingsProvider Provider()
        {
            return new AssetSettings() { label = "Assets" };
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.BeginHorizontal();
            try
            {
                using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                {
                    if (m_Project != null)
                    {
                        EditorGUILayout.BeginVertical();

                        DrawFilter();
                        DrawTreeView();

                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No DOTS project is currently opened.");
                    }
                }
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }
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
            if (m_TreeView == null)
            {
                return;
            }

            GUILayout.Space(5);
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            try
            {
                m_TreeView.OnGUI(GUILayoutUtility.GetRect(0, m_TreeView.totalHeight));
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
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
