using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Editor.Build;
using UnityEditor;
using UnityEngine;

namespace Unity.Editor
{
    internal static class PlatformSettings
    {
        private static readonly ReadOnlyCollection<Platform> m_AvailablePlatforms;
        private static readonly string[] m_AvailablePlatformNames;

        static PlatformSettings()
        {
            var availablePlatforms = new List<Platform>();

            var platformTypes = TypeCache.GetTypesDerivedFrom<Platform>();
            foreach (var platformType in platformTypes)
            {
                try
                {
                    if (platformType.IsAbstract)
                        continue;

                    var platform = (Platform)Activator.CreateInstance(platformType);
                    availablePlatforms.Add(platform);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error instantiating {platformType.FullName}: " + e.Message);
                    continue;
                }
            }

            m_AvailablePlatforms = new ReadOnlyCollection<Platform>(availablePlatforms);

            var availablePlatformCount = m_AvailablePlatforms.Count;
            m_AvailablePlatformNames = new string[availablePlatformCount];

            for (int i = 0; i < availablePlatformCount; i++)
                m_AvailablePlatformNames[i] = m_AvailablePlatforms[i].GetDisplayName();
        }

        public static Platform GetDefaultPlatform()
        {
            return new DesktopDotNetPlatform();
        }

        public static Platform GetPlatformFromName(string name)
        {
            int availablePlatformCount = m_AvailablePlatforms.Count;
            for (int i = 0; i < availablePlatformCount; i++)
            {
                var platform = m_AvailablePlatforms[i];
                if (GetPlatformName(platform) == name)
                    return platform;
            }

            return GetDefaultPlatform();
        }

        public static string GetPlatformName(Platform platform)
        {
            return platform.ToString();
        }

        public static Platform DrawPlatformPopup(Platform platform, string label = null, GUIStyle style = null)
        {
            int platformIndex = 0;
            int availablePlatformCount = m_AvailablePlatforms.Count;

            for (int i = 0; i < availablePlatformCount; i++)
            {
                if (m_AvailablePlatforms[i].GetType() == platform.GetType())
                {
                    platformIndex = i;
                    break;
                }
            }

            int newPlatformIndex;
            style = style ?? EditorStyles.popup;

            if (!string.IsNullOrEmpty(label))
            {
                newPlatformIndex = EditorGUILayout.Popup(label, platformIndex, m_AvailablePlatformNames, style, GUILayout.MinWidth(125));
            }
            else
            {
                newPlatformIndex = EditorGUILayout.Popup(platformIndex, m_AvailablePlatformNames, style, GUILayout.MinWidth(125));
            }

            return m_AvailablePlatforms[newPlatformIndex];
        }
    }
}
