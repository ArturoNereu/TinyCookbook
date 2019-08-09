using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Editor.Bridge
{
    internal static class DragAndDropService
    {
        private static readonly List<Action> k_RemoveHandlerMethods = new List<Action>(); 
        
        public static void RegisterSceneViewHandler(Func<UnityEngine.Object, Vector3, Vector2, Transform, bool, UnityEditor.DragAndDropVisualMode> handler)
        {
            k_RemoveHandlerMethods.Add(UnityEditor.DragAndDropService.AddDropHandler((o, v3, v2, t, b) => handler(o, v3, v2, t, b)));
        }
        
        public static void RegisterProjectBrowserHandler(Func<int, string, bool, UnityEditor.DragAndDropVisualMode> handler)
        {
            k_RemoveHandlerMethods.Add(UnityEditor.DragAndDropService.AddDropHandler((i, s, b) => handler(i, s, b)));
        }

        public static void UnregisterAllHandlers()
        {
            foreach (var removeHandler in k_RemoveHandlerMethods)
            {
                removeHandler();
            }
            k_RemoveHandlerMethods.Clear();
        }
    }
}