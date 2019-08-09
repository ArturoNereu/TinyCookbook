using JetBrains.Annotations;
using System;
using UnityEditor;

namespace Unity.Editor.MenuItems
{
    internal static class HelpMenuItems
    {
        [UsedImplicitly, CommandHandler(CommandIds.Help.Forums, CommandHint.Menu)]
        private static void OpenUserForums(CommandExecuteContext context)
        {
            UnityEngine.Application.OpenURL("https://forum.unity.com/forums/project-tiny.151/");
        }
    }
}
