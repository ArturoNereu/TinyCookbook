using System;
using System.Diagnostics;
using System.IO;
using Unity.Editor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Unity.Editor.Tools
{
    internal class HTTPServer : BasicServer
    {
        public static HTTPServer Instance { get; private set; }
        protected override string[] ShellArgs =>
            new []
            {
                $"-p {Port}",
                $"-w {Process.GetCurrentProcess().Id}",
                $"-c {ContentDir.DoubleQuoted()}",
                $"-i {IndexFile.DoubleQuoted()}",
                $"-t {Path.GetFullPath(".").DoubleQuoted()}"
            };

        public override Uri URL => new UriBuilder("http", LocalIP, Port).Uri;

        public Uri LocalURL => Listening ? new UriBuilder("http", "localhost", Port).Uri : new Uri(Path.Combine(ContentDir, "index.html"));
        public string BuildTimeStamp
        {
            get => EditorPrefs.GetString($"Unity.Tiny.{Name}.BuildTimeStamp", null);
            set => EditorPrefs.SetString($"Unity.Tiny.{Name}.BuildTimeStamp", value);
        }
        private string ContentDir { get; set; }
        private string IndexFile { get; set; }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Instance = new HTTPServer();
            Application.EndAuthoringProject += (project) =>
            {
                Instance.Close();
            };
        }

        private HTTPServer() : base("HTTPServer")
        {
        }

        private void Host(string contentDir, string indexFile, int port)
        {
            Close();

            ContentDir = contentDir;
            IndexFile = indexFile;
            BuildTimeStamp = DateTime.Now.ToString("d MMM yyyy HH:mm:ss");
            if (Listen(port))
            {
                UnityEngine.Debug.Log($"DOTS project content hosted at {URL.AbsoluteUri}");
            }
        }

        public bool HostAndOpen(string contentDir, string indexFile, int port)
        {
            if (port == 0 || string.IsNullOrEmpty(contentDir) || !Directory.Exists(contentDir))
            {
                return false;
            }

            using (new Utilities.ProgressBarScope("HTTP Server", "Starting..."))
            {
                // Get hosted URL from content directory
                Host(contentDir, indexFile, port);
                UnityEngine.Application.OpenURL(LocalURL.AbsoluteUri);
            }

            return true;
        }
    }
}
