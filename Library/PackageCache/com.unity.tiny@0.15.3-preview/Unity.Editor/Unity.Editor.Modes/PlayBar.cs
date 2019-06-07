using System;
using JetBrains.Annotations;
using Unity.Editor.Build;
using Unity.Editor.MenuItems;
using UnityEditor;
using UnityEngine;

namespace Unity.Editor.Modes
{
    internal static class PlayBar
    {
        [UsedImplicitly, CommandHandler(CommandIds.Edit.Play, CommandHint.UserDefined)]
        internal static void PlayCommandHandler(CommandExecuteContext context)
        {
            Application.BuildAndRun();
        }

        private class Styles
        {
            public GUIStyle OffsetStyle;
            public GUIContent PlayIcon;
            public GUIStyle CommandStyle;
            public GUIStyle DropdownStyle;

            public Styles()
            {
                OffsetStyle = new GUIStyle {margin = new RectOffset(0, 0, -3, 0)};
            
                PlayIcon = EditorGUIUtility.TrIconContent("PlayButton", "Play");
            
                CommandStyle = new GUIStyle("Command");
                var margin = CommandStyle.margin;
                margin.top = 0;
                CommandStyle.margin = margin;
                
                DropdownStyle = new GUIStyle("Dropdown");
                margin = DropdownStyle.margin;
                margin.top = 2;
                DropdownStyle.margin = margin;
            }
        }

        private static Styles PlayBarStyles;
        
        [CommandHandler("DOTS/GUI/PlayBar", CommandHint.UI)]
        [UsedImplicitly]
        internal static void DrawPlayBar(CommandExecuteContext context)
        {
            if (PlayBarStyles == null)
            {
                PlayBarStyles = new Styles();
            }
            EditorGUILayout.BeginHorizontal(PlayBarStyles.OffsetStyle);
            var settings = Application.AuthoringProject?.Settings;
            if (settings == null)
            {
                if (GUILayout.Button("Open a DOTS project"))
                {
                    CommandService.Execute(CommandIds.File.OpenProject);
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.BeginVertical();

            var workspaceManager = Application.AuthoringProject.Session.GetManager<WorkspaceManager>();
            workspaceManager.ActivePlatform = PlatformSettings.DrawPlatformPopup(workspaceManager.ActivePlatform, style: PlayBarStyles.DropdownStyle);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical();
            workspaceManager.ActiveConfiguration = (Configuration)EditorGUILayout.EnumPopup(workspaceManager.ActiveConfiguration, PlayBarStyles.DropdownStyle, GUILayout.MinWidth(60));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            if (GUILayout.Button(PlayBarStyles.PlayIcon, PlayBarStyles.CommandStyle))
            {
                CommandService.Execute(CommandIds.Edit.Play);
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}
