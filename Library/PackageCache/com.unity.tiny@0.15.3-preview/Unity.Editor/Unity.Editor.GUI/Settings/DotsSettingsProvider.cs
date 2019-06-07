using JetBrains.Annotations;
using System;
using Unity.Editor.Build;
using Unity.Editor.Utilities;
using Unity.Tiny.Core2D;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    [UsedImplicitly]
    internal class DotsSettingsProvider : SettingsProvider
    {
        private BuildManifestView m_BuildManifestView;
        private TreeViewState m_TreeViewState;

        public DotsSettingsProvider() : base("Project/DOTS", SettingsScope.Project)
        {
        }

        [SettingsProvider]
        [UsedImplicitly]
        public static SettingsProvider Provider()
        { 
            return new DotsSettingsProvider() { label = "DOTS" };
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            Application.BeginAuthoringProject += OnActivateOrBeginAuthoring;
            Application.EndAuthoringProject += OnDeactivateOrEndAuthoring;
            OnActivateOrBeginAuthoring(Application.AuthoringProject);
            base.OnActivate(searchContext, rootElement);
        }

        private void OnActivateOrBeginAuthoring(Project obj)
        {
            if (obj == null)
                return;
            
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            m_BuildManifestView = new BuildManifestView(m_TreeViewState);
        }
        
        private void OnDeactivateOrEndAuthoring(Project obj)
        {
            if (m_BuildManifestView != null)
            {
                m_BuildManifestView.Dispose();
                m_BuildManifestView = null;
            }
        }

        public override void OnDeactivate()
        {
            Application.BeginAuthoringProject -= OnActivateOrBeginAuthoring;
            Application.EndAuthoringProject -= OnDeactivateOrEndAuthoring;
            OnDeactivateOrEndAuthoring(Application.AuthoringProject);
            base.OnDeactivate();
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.BeginHorizontal();
            try
            {
                GUILayout.Space(10.0f);
                using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                {
                    if (Application.AuthoringProject != null && m_BuildManifestView != null)
                    {
                        var project = Application.AuthoringProject;
                        label = $"DOTS Project settings for {project.Name}";
                        Draw(project);
                    }
                    else
                    {
                        label = "DOTS Settings";
                        EditorGUILayout.LabelField("No DOTS project is currently opened.");
                    }
                }
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawProjectSettings(Project project)
        {
            var em = project.WorldManager.EntityManager;
            var configEntity = project.WorldManager.GetConfigEntity();
            DisplayInfo newDisplayInfo = new DisplayInfo();
            DisplayInfo originalDisplayInfo = em.GetComponentData<DisplayInfo>(configEntity);

            GUILayout.Label("Project Settings", DotsStyles.SettingsSection);
            EditorGUILayout.Space();
            newDisplayInfo.autoSizeToFrame = EditorGUILayout.Toggle("Auto-Resize", originalDisplayInfo.autoSizeToFrame);
            newDisplayInfo.width = Math.Max(1, EditorGUILayout.DelayedIntField("Viewport Width", originalDisplayInfo.width));
            newDisplayInfo.height = Math.Max(1, EditorGUILayout.DelayedIntField("Viewport Height", originalDisplayInfo.height));
            newDisplayInfo.renderMode = (Tiny.Core2D.RenderMode)EditorGUILayout.EnumPopup("Render Mode", originalDisplayInfo.renderMode);
            TextureSettingsField(project.Settings.DefaultTextureSettings);
            
            // If any of our settings changes, persist the change to ECS storage
            if (!originalDisplayInfo.Equals(newDisplayInfo))
            {
                em.SetComponentData(project.WorldManager.GetConfigEntity(), newDisplayInfo);
            }
        }

        private void Draw(Project project)
        {
            EditorGUIUtility.labelWidth = 400;
            EditorGUILayout.BeginVertical();
            try
            {
                EditorGUILayout.Space();
                DrawProjectSettings(project);

                GUILayout.Space(20);
                GUILayout.Label("Build Manifest", DotsStyles.SettingsSection);
                var bmRect = EditorGUILayout.GetControlRect(false, 150);
                m_BuildManifestView.OnGUI(bmRect);

                GUILayout.Space(20);
                GUILayout.Label("Web Settings", DotsStyles.SettingsSection);
                EditorGUILayout.Space();
                project.Settings.WebSettings.MemorySizeInMB = WebSettings.ClampValueToMultipleOf16((uint)EditorGUILayout.DelayedIntField(
                    new GUIContent("Memory Size in MB", "Total memory size pre-allocated for the entire project."), (int)project.Settings.WebSettings.MemorySizeInMB));
#if false
                project.Settings.WebSettings.SingleFileOutput = EditorGUILayout.Toggle(
                    new GUIContent("Single file output", "Output build in a single file and assets will be embedded as base64 (this will increase asset size by approx 35%)."), project.Settings.WebSettings.SingleFileOutput);
#endif

                GUILayout.Space(20);
                GUILayout.Label("Build Settings", DotsStyles.SettingsSection);
                EditorGUILayout.Space();

                var workspaceManager = project.Session.GetManager<WorkspaceManager>();
                workspaceManager.ActiveConfiguration = (Configuration)EditorGUILayout.EnumPopup("Build Configuration", workspaceManager.ActiveConfiguration);
                workspaceManager.ActivePlatform = PlatformSettings.DrawPlatformPopup(workspaceManager.ActivePlatform, label: "Build Target");

                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                try
                {
                    GUILayout.FlexibleSpace();

                    var content = new GUIContent("Build and Run");

                    var rect = GUILayoutUtility.GetRect(content, DotsStyles.AddComponentStyle);
                    if (EditorGUI.DropdownButton(rect, content, FocusType.Passive, DotsStyles.AddComponentStyle))
                    {
                        Application.BuildAndRun();
                        GUIUtility.ExitGUI();
                    }

                    GUILayout.FlexibleSpace();
                }
                finally
                {
                    EditorGUILayout.EndHorizontal();
                }
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }
        }

        private static void TextureSettingsField(TextureSettings textureSettings)
        {
            textureSettings.FormatType =
                (TextureFormatType)EditorGUILayout.EnumPopup("Default Texture Format", textureSettings.FormatType);

            switch (textureSettings.FormatType)
            {
                case TextureFormatType.JPG:
                    textureSettings.JpgCompressionQuality = (uint)EditorGUILayout.IntSlider("Compression Quality", (int)textureSettings.JpgCompressionQuality, 1, 100);
                    break;
                case TextureFormatType.WebP:
                    textureSettings.WebPCompressionQuality = (uint)EditorGUILayout.IntSlider("Compression Quality", (int)textureSettings.WebPCompressionQuality, 1, 100);
                    break;
                case TextureFormatType.Source:
                    break;
                case TextureFormatType.PNG:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal static class ProjectSettingsStyles
        {
            public static GUIStyle SettingsSection { get; } = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
            };
        }
    }
}
