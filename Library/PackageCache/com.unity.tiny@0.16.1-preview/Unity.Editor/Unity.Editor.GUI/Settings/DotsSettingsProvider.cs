using JetBrains.Annotations;
using System;
using Unity.Editor.Build;
using Unity.Editor.Utilities;
using Unity.Tiny.Core2D;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    [UsedImplicitly]
    internal class DotsSettingsProvider : BaseSettingsProvider
    {
        private DotsSettingsProvider() : base("Project/DOTS", SettingsScope.Project)
        {
        }

        [SettingsProvider, UsedImplicitly]
        private static SettingsProvider Provider()
        { 
            return new DotsSettingsProvider { label = "DOTS" };
        }
        
        protected override void OnBeginAuthoring(Project project) {}

        protected override void OnEndAuthoring(Project project) {}
        
        protected override VisualElement CreateSettingsGUI(string searchContext)
        {
            var settingsUI =  new VisualElement();
            settingsUI.Add(new IMGUIContainer(() =>
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                Draw();
                EditorGUILayout.EndHorizontal();
            }));
            
            return settingsUI;
        }

        private void Draw()
        {
            EditorGUIUtility.labelWidth = 400;
            EditorGUILayout.BeginVertical();
            try
            {
                EditorGUILayout.Space();
                DrawProjectSettings();

                GUILayout.Space(20);
                GUILayout.Label("Web Settings", DotsStyles.SettingsSection);
                EditorGUILayout.Space();
                Project.Settings.WebSettings.MemorySizeInMB = WebSettings.ClampValueToMultipleOf16((uint)EditorGUILayout.DelayedIntField(
                    new GUIContent("Memory Size in MB", "Total memory size pre-allocated for the entire project."), (int)Project.Settings.WebSettings.MemorySizeInMB));
#if false
                project.Settings.WebSettings.SingleFileOutput = EditorGUILayout.Toggle(
                    new GUIContent("Single file output", "Output build in a single file and assets will be embedded as base64 (this will increase asset size by approx 35%)."), project.Settings.WebSettings.SingleFileOutput);
#endif

                GUILayout.Space(20);
                GUILayout.Label("Build Settings", DotsStyles.SettingsSection);
                EditorGUILayout.Space();

                var workspaceManager = Project.Session.GetManager<WorkspaceManager>();
                workspaceManager.ActiveConfiguration = (Configuration)EditorGUILayout.EnumPopup("Build Configuration", workspaceManager.ActiveConfiguration);
                workspaceManager.ActiveBuildTarget = BuildTargetSettings.DrawBuildTargetPopup(workspaceManager.ActiveBuildTarget, label: "Build Target");

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

        private void DrawProjectSettings()
        {
            var entityManager = Project.EntityManager;
            var configEntity = Project.WorldManager.GetConfigEntity();
            var newDisplayInfo = new DisplayInfo();
            var originalDisplayInfo = entityManager.GetComponentData<DisplayInfo>(configEntity);

            GUILayout.Label("Project Settings", DotsStyles.SettingsSection);
            EditorGUILayout.Space();
            newDisplayInfo.autoSizeToFrame = EditorGUILayout.Toggle("Auto-Resize", originalDisplayInfo.autoSizeToFrame);
            newDisplayInfo.width = Math.Max(1, EditorGUILayout.DelayedIntField("Viewport Width", originalDisplayInfo.width));
            newDisplayInfo.height = Math.Max(1, EditorGUILayout.DelayedIntField("Viewport Height", originalDisplayInfo.height));
            newDisplayInfo.renderMode = (Tiny.Core2D.RenderMode)EditorGUILayout.EnumPopup("Render Mode", originalDisplayInfo.renderMode);
            TextureSettingsField(Project.Settings.DefaultTextureSettings);
            
            // If any of our settings changes, persist the change to ECS storage
            if (!originalDisplayInfo.Equals(newDisplayInfo))
            {
                entityManager.SetComponentData(configEntity, newDisplayInfo);
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
    }
}
