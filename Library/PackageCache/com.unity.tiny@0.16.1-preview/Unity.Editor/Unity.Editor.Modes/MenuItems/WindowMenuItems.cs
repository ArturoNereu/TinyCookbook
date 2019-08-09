using JetBrains.Annotations;
using System;
using Unity.Editor.Hierarchy;
using UnityEditor;
using UnityEngine;

namespace Unity.Editor.MenuItems
{
    internal static class WindowMenuItems
    {
        [UsedImplicitly, CommandHandler(CommandIds.Window.Hierarchy, CommandHint.Menu)]
        public static void ShowHierarchyWindow(CommandExecuteContext context)
        {
            ScriptableObject.CreateInstance<EntityHierarchyWindow>().Show();
        }

        [UsedImplicitly, CommandHandler(CommandIds.Window.Context, CommandHint.Menu)]
        private static void ShowRegistry(CommandExecuteContext context)
        {
            throw new NotImplementedException();
        }

        [UsedImplicitly, CommandHandler(CommandIds.Window.Configuration, CommandHint.Menu)]
        public static void ShowConfigurationWindow(CommandExecuteContext context)
        {
            throw new NotImplementedException();
        }

        [UsedImplicitly, CommandHandler(CommandIds.Window.BindingsDebugger, CommandHint.Menu)]
        public static void ShowBindingsDebuggerWindow(CommandExecuteContext context)
        {
            throw new NotImplementedException();
        }
    }
}
