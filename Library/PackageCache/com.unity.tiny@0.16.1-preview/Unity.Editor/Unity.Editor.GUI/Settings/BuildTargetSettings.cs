using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Editor.Build;
using UnityEditor;
using UnityEngine;
using BuildTarget = Unity.Editor.Build.BuildTarget;

namespace Unity.Editor
{
    internal static class BuildTargetSettings
    {
        private static readonly ReadOnlyCollection<BuildTarget> m_AvailableBuildTargets;
        private static readonly string[] m_AvailableBuildTargetNames;

        public static IReadOnlyCollection<BuildTarget> AvailableBuildTargets => m_AvailableBuildTargets;

        static BuildTargetSettings()
        {
            var availableBuildTargets = new List<BuildTarget>();
            var buildTargetTypes = TypeCache.GetTypesDerivedFrom<BuildTarget>();
          
            foreach (var buildTargetType in buildTargetTypes)
            {
                try
                {
                    if (buildTargetType.IsAbstract)
                        continue;

                    var buildTarget = (BuildTarget)Activator.CreateInstance(buildTargetType);
                    availableBuildTargets.Add(buildTarget);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error instantiating {buildTargetType.FullName}: " + e.Message);
                }
            }

            m_AvailableBuildTargets = new ReadOnlyCollection<BuildTarget>(availableBuildTargets);

            var availableBuildTargetCount = m_AvailableBuildTargets.Count;
            m_AvailableBuildTargetNames = new string[availableBuildTargetCount];

            for (int i = 0; i < availableBuildTargetCount; i++)
                m_AvailableBuildTargetNames[i] = m_AvailableBuildTargets[i].GetDisplayName();
        }

        public static BuildTarget GetDefaultBuildTarget()
        {
            return new DesktopDotNetBuildTarget();
        }

        public static BuildTarget GetBuildTargetFromName(string name)
        {
            int availableBuildTargetCount = m_AvailableBuildTargets.Count;
            for (int i = 0; i < availableBuildTargetCount; i++)
            {
                var buildTarget = m_AvailableBuildTargets[i];
                if (GetBuildTargetName(buildTarget) == name)
                    return buildTarget;
            }

            return GetDefaultBuildTarget();
        }

        public static string GetBuildTargetName(BuildTarget buildTarget)
        {
            return buildTarget.ToString();
        }

        public static BuildTarget DrawBuildTargetPopup(BuildTarget buildTarget, string label = null, GUIStyle style = null)
        {
            int buildTargetIndex = 0;
            int availableBuildTargetCount = m_AvailableBuildTargets.Count;

            for (int i = 0; i < availableBuildTargetCount; i++)
            {
                if (m_AvailableBuildTargets[i].GetType() == buildTarget.GetType())
                {
                    buildTargetIndex = i;
                    break;
                }
            }

            int newBuildTargetIndex;
            style = style ?? EditorStyles.popup;

            if (!string.IsNullOrEmpty(label))
            {
                newBuildTargetIndex = EditorGUILayout.Popup(label, buildTargetIndex, m_AvailableBuildTargetNames, style, UnityEngine.GUILayout.MinWidth(125));
            }
            else
            {
                newBuildTargetIndex = EditorGUILayout.Popup(buildTargetIndex, m_AvailableBuildTargetNames, style, UnityEngine.GUILayout.MinWidth(125));
            }

            return m_AvailableBuildTargets[newBuildTargetIndex];
        }
    }
}
