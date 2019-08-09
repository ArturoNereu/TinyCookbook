using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Authoring;
using Unity.Tiny.Core2D;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Analytics;

// ReSharper disable InconsistentNaming

// Note to contributors: Use snake_case for serialized fields sent as part of event payloads
// this convention is used by the Data Science team

#pragma warning disable 649
#pragma warning disable 414

namespace Unity.Editor
{
    [InitializeOnLoad]
    internal static class Analytics
    {
        private static bool s_Registered;

        private enum EventName
        {
            tinyEditor,
            tinyEditorBuild
        }

        private static HashSet<int> s_OnceHashCodes = new HashSet<int>();

        static Analytics()
        {
            EditorApplication.delayCall += () =>
            {
                UnityEngine.Application.logMessageReceived += (condition, trace, type) =>
                {
                    if (type == LogType.Exception &&
                        !string.IsNullOrEmpty(trace) &&
                        trace.Contains(Application.PackageId))
                    {
                        if (s_OnceHashCodes.Add(trace.GetHashCode()))
                        {
                            SendErrorEvent("__uncaught__", condition, trace);
                        }
                    }
                };
            };
        }

        private static bool RegisterEvents()
        {
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                return false;
            }
            if (!EditorAnalytics.enabled)
            {
                TraceError("Editor analytics are disabled");
                return false;
            }

            if (s_Registered)
            {
                return true;
            }

            var allNames = Enum.GetNames(typeof(EventName));
            if (allNames.Any(eventName => !RegisterEvent(eventName)))
            {
                return false;
            }

            s_Registered = true;
            return true;
        }

        private static bool RegisterEvent(string eventName)
        {
            const string vendorKey = "unity.tiny.editor";
            var result = EditorAnalytics.RegisterEventWithLimit(eventName, 100, 1000, vendorKey);
            switch (result)
            {
                case AnalyticsResult.Ok:
                {
                    #if UNITY_TINY_INTERNAL
                    UnityEngine.Debug.Log($"Analytics: Registered event: {eventName}");
                    #endif
                    return true;
                }
                case AnalyticsResult.TooManyRequests:
                    // this is fine - event registration survives domain reload (native)
                    return true;
                default:
                {
                    TraceError($"failed to register analytics event '{eventName}'. Result: '{result}'");
                    return false;
                }
            }

        }

        private static void TraceError(string message)
        {
            message = "Analytics: " + message;
#if UNITY_TINY_INTERNAL
            UnityEngine.Debug.LogError(message);
#else
            Console.WriteLine(message);
#endif
        }

        [Serializable]
        internal struct PackageInfo
        {
            public string version;
            public bool preview;
            public bool embedded;

            public static PackageInfo Create()
            {
                var pkgInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(Analytics).Assembly);
                return new PackageInfo()
                {
                    version = pkgInfo.version,
                    preview = pkgInfo.version.Contains("-preview"),
                    embedded = pkgInfo.source != PackageSource.Registry && pkgInfo.source != PackageSource.BuiltIn
                };
            }
        }

        [Serializable]
        private struct ContextInfo
        {
            public bool internal_build;
            public string buildTarget;
            public string configuration;
            public bool run;

            public static ContextInfo Default
            {
                get
                {
                    var result = new ContextInfo()
                    {
                        #if UNITY_TINY_INTERNAL
                        internal_build = true,
                        #endif
                    };

                    var project = Application.AuthoringProject;
                    if (project == null)
                        return result;

                    return Create(project.Settings, project.Session);
                }
            }

            public static ContextInfo Create(ProjectSettings settings, Session session)
            {
                var workspaceManager = session.GetManager<WorkspaceManager>();

                return new ContextInfo()
                {
                    #if UNITY_TINY_INTERNAL
                    internal_build = true,
                    #endif
                    buildTarget = workspaceManager.ActiveBuildTarget.ToString(),
                    run = true,
                    configuration = workspaceManager.ActiveConfiguration.ToString()
                };
            }
        }

        [Serializable]
        private struct ProjectInfo
        {
            public string[] modules;

            public static ProjectInfo Default => Create(Application.AuthoringProject);

            public static ProjectInfo Create(Project project)
            {
                var result = new ProjectInfo();

                if (project == null)
                    return result;

                result.modules = project.IncludedAssemblyDefinitions().Select(a => a.name).ToArray();
                Array.Sort(result.modules);

                return result;
            }
        }

        public enum EventCategory
        {
            Custom = 0,
            Information = 1,
            Warning = 2,
            Error = 3,
            Usage = 4
        }

        [Serializable]
        private struct GenericEvent
        {
            public PackageInfo package;
            public ContextInfo context;
            public ProjectInfo project;

            public string category;
            public int category_id;
            public string name;
            public string message;
            public string description;
            public long duration;
        }

        public static void SendCustomEvent(string category, string name, string message = null, string description = null)
        {
            SendEvent(EventCategory.Custom, category, name, message, description, TimeSpan.Zero);
        }

        public static void SendCustomEvent(string category, string name, TimeSpan duration, string message = null, string description = null)
        {
            SendEvent(EventCategory.Custom, category, name, message, description, duration);
        }

        public static void SendExceptionOnce(string name, Exception ex)
        {
            if (ex == null)
            {
                return;
            }
            var hashCode = ex.StackTrace.GetHashCode();
            if (s_OnceHashCodes.Add(hashCode))
            {
                SendException(name, ex);
            }
        }

        public static void SendException(string name, Exception ex)
        {
            if (ex == null)
            {
                return;
            }
            SendErrorEvent(name, ex.Message, ex.ToString());
        }

        public static void SendErrorEvent(string name, string message = null, string description = null)
        {
            SendEvent(EventCategory.Error, name, TimeSpan.Zero, message, description);
        }

        public static void SendEvent(EventCategory category, string name, string message = null, string description = null)
        {
            SendEvent(category, category.ToString(), name, message, description, TimeSpan.Zero);
        }

        public static void SendEvent(EventCategory category, string name, TimeSpan duration, string message = null, string description = null)
        {
            SendEvent(category, category.ToString(), name, message, description, duration);
        }

        private static void SendEvent(EventCategory category, string categoryName, string name, string message, string description,
            TimeSpan duration)
        {
            if (string.IsNullOrEmpty(categoryName) || string.IsNullOrEmpty(name))
            {
                TraceError(new ArgumentNullException().ToString());
                return;
            }
            var e = new GenericEvent()
            {
                package = PackageInfo.Create(),
                context = ContextInfo.Default,
                project = ProjectInfo.Default,

                category = categoryName,
                category_id = (int)category,
                name = name,
                message = message,
                description = description,
                duration = duration.Ticks
            };

            Send(EventName.tinyEditor, e);
        }

        [Serializable]
        private struct BuildEvent
        {
            public PackageInfo package;
            public ContextInfo context;
            public ProjectInfo project;

            public long duration;

            public long total_bytes;
            public long runtime_bytes;
            public long assets_bytes;
            public long code_bytes;

            public long total_raw_bytes;
            public long runtime_raw_bytes;
            public long assets_raw_bytes;
            public long code_raw_bytes;

            public long heap_size;
            public bool opt_auto_resize;
            public bool opt_ws_client;
            public bool opt_webp_decompressor;
            public bool opt_ecma5;
            public bool opt_single_file_output;
            public bool opt_embed_assets;
            public string default_texture_format;
        }

        public static void SendBuildEvent(Project project, Build.BuildResult buildResult)
        {
            if (project?.Settings == null )
                return;

            var configEntity = project.WorldManager.GetConfigEntity();
            var displayInfo = project.EntityManager.GetComponentData<DisplayInfo>(configEntity);

            var e = new BuildEvent()
            {
                package = PackageInfo.Create(),
                context = ContextInfo.Default,
                project = ProjectInfo.Create(project),

                duration = buildResult.Duration.Ticks,

                heap_size = project.Settings.WebSettings.MemorySizeInMB,
                opt_auto_resize = displayInfo.autoSizeToFrame,
                opt_webp_decompressor = false, // TODO: re-populate from settings
                opt_ecma5 = false, // deprecated
                opt_single_file_output = project.Settings.WebSettings.SingleFileOutput,
                opt_embed_assets = project.Settings.WebSettings.SingleFileOutput,
                default_texture_format = project.Settings.DefaultTextureSettings.FormatType.ToString()
            };

            Send(EventName.tinyEditorBuild, e);
        }

        private static void Send(EventName eventName, object eventData)
        {
            if (!RegisterEvents())
            {
                return;
            }

            var result = EditorAnalytics.SendEventWithLimit(eventName.ToString(), eventData);
            if (result == AnalyticsResult.Ok)
            {
                #if UNITY_TINY_INTERNAL
                Console.WriteLine($"Analytics: event='{eventName}', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(eventData, true)}");
                #endif
            }
            else
            {
                TraceError($"failed to send event {eventName}. Result: {result}");
            }
        }
    }
}

