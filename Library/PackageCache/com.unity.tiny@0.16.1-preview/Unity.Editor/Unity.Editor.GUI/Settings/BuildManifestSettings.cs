using System;
using JetBrains.Annotations;
using Unity.Editor.Persistence;
using Unity.Tiny.Scenes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal class BuildManifestSettings : BaseSettingsProvider
    {
        private BuildManifestView m_BuildManifestView;
        private TreeViewState m_TreeViewState;
        private readonly SearchField m_SearchField = new SearchField();
        private VisualElement m_Root;
        private IMGUIContainer m_TreeViewContainer;
        
        private BuildManifestSettings() : base("Project/DOTS/Build Manifest", SettingsScope.Project) {}

        [SettingsProvider, UsedImplicitly]
        private static SettingsProvider Provider()
        {
            return new BuildManifestSettings { label = "Build Manifest" };
        }
        
        protected override void OnBeginAuthoring(Project project)
        {
            if (m_TreeViewState == null)
            {
                m_TreeViewState = new TreeViewState();
            }

            m_BuildManifestView = new BuildManifestView(m_TreeViewState, project);
        }
        
        protected override void OnEndAuthoring(Project project)
        {
            if (m_BuildManifestView != null)
            {
                m_BuildManifestView.Dispose();
                m_BuildManifestView = null;
            }
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
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5);
                m_BuildManifestView.searchString = m_SearchField.OnToolbarGUI(m_BuildManifestView.searchString);
                GUILayout.Space(4);
            }
        }

        private void DrawTreeView()
        {
           const int space = 5;
           const int footerHeight = 30;
           GUILayout.Space(space);
           var startPosition = m_TreeViewContainer.localBound.y  + m_TreeViewContainer.parent.localBound.y + space;
           var height = Mathf.Min(LocalBound.height - startPosition - footerHeight, m_BuildManifestView.totalHeight); 
           m_BuildManifestView.OnGUI(GUILayoutUtility.GetRect(0, height));

           DrawFooter();
        }
        
        private void DrawFooter()
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Scene", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                var scenePath = EditorUtility.OpenFilePanel($"Add Scene to {Project.Name}", string.Empty, "scene");
                scenePath = AssetDatabaseUtility.GetPathRelativeToProjectPath(scenePath);
                
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (sceneAsset != null)
                {
                    var sceneReference = new SceneReference {SceneGuid = new Guid(sceneAsset.Guid)};
                    Project.AddScene(sceneReference);
                    m_BuildManifestView.Invalidate();
                }
            }
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
        }
    }
}