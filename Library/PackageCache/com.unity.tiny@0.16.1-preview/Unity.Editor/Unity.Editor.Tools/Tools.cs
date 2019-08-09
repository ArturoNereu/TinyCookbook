using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Editor.Extensions;
using UnityEditor;

namespace Unity.Editor.Tools
{
    internal static class NodeTools
    {
        private static string ToolsManagerNativeName
        {
            get
            {
                var toolsManager = "DotsEditorTools";
#if UNITY_EDITOR_WIN
                return $"{toolsManager}-win";
#elif UNITY_EDITOR_OSX
                return $"{toolsManager}-macos";
#else
#error not implemented
#endif
            }
        }

        public static bool Run(string name, params string[] arguments)
        {
            var toolDir = Application.ToolsDirectory.Combine("manager");
            var result = Shell.Run(new ShellProcessArgs()
            {
                Executable = Path.Combine(toolDir.FullName, ToolsManagerNativeName),
                Arguments = name.AsEnumerable().Concat(arguments),
                WorkingDirectory = toolDir
            });
            return result.Succeeded;
        }

        public static Process RunAsync(string name, params string[] arguments)
        {
            var toolDir = Application.ToolsDirectory.Combine("manager");
            return Shell.RunAsync(new ShellProcessArgs()
            {
                Executable = Path.Combine(toolDir.FullName, ToolsManagerNativeName),
                Arguments = name.AsEnumerable().Concat(arguments),
                WorkingDirectory = toolDir
            });
        }
    }

    internal static class ImageTools
    {
        private static string PlatformDir
        {
            get
            {
#if UNITY_EDITOR_WIN
                return "win";
#elif UNITY_EDITOR_OSX
                return "osx";
#else
#error not implemented
#endif
            }
        }

        public static bool Run(string name, params string[] arguments)
        {
            var toolDir = Application.ToolsDirectory.Combine("images", PlatformDir);
            var result = Shell.Run(new ShellProcessArgs()
            {
                Executable = name,
                Arguments = arguments,
                WorkingDirectory = toolDir,
                ExtraPaths = toolDir.FullName.AsEnumerable()
            });
            return result.Succeeded;
        }
    }

    internal static class BeeTools
    {
        // Group 1: progress numerator
        // Group 2: progress denominator
        // Group 3: progress description
        private static readonly Regex BeeProgressRegex = new Regex(@"\[(?:(\s*\d+)/(\s*\d+)|\s*\w*)\s*(?:\w*)\]\s*(.*)", RegexOptions.Compiled);

        public struct ProgressInfo
        {
            public float Progress;
            public string Info;
            public string FullInfo;
            public bool IsDone;
            public int ExitCode;
        }

        public static IEnumerator<ProgressInfo> Run(string arguments, StringBuilder command, StringBuilder output, DirectoryInfo workingDirectory = null)
        {
            var beeExe = Path.GetFullPath("Packages/com.unity.tiny/DotsPlayer/bee~/bee.exe");
            var executable = beeExe;
            arguments = "--no-colors " + arguments;

#if !UNITY_EDITOR_WIN
            arguments = executable.DoubleQuoted() + " " + arguments;
            executable = Application.MonoDirectory.GetFile("mono").FullName;
#endif

            command.Append(executable);
            command.Append(" ");
            command.Append(arguments);

            var progressInfo = new ProgressInfo()
            {
                Progress = 0.0f,
                Info = null
            };

            void ProgressHandler(object sender, DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    lock (output)
                    {
                        output.AppendLine(args.Data);
                    }
                }

                var msg = args.Data;
                if (string.IsNullOrWhiteSpace(msg))
                {
                    return;
                }

                progressInfo.FullInfo = msg;

                var match = BeeProgressRegex.Match(msg);
                if (match.Success)
                {
                    var num = match.Groups[1].Value;
                    var den = match.Groups[2].Value;
                    if (int.TryParse(num, out var numInt) && int.TryParse(den, out var denInt))
                    {
                        progressInfo.Progress = (float)numInt / denInt;
                    }
                    progressInfo.Info = match.Groups[3].Value;
                }
                else
                {
                    progressInfo.Progress = float.MinValue;
                    progressInfo.Info = null;
                }
            }

            var config = new ShellProcessArgs()
            {
                Executable = executable,
                Arguments = arguments.AsEnumerable(),
                WorkingDirectory = workingDirectory,
#if !UNITY_EDITOR_WIN
                // bee requires external programs to perform build actions
                EnvironmentVariables = new Dictionary<string, string>() { {"PATH", string.Join(":", 
                    Application.MonoDirectory.FullName, 
                    "/bin", 
                    "/usr/bin", 
                    "/usr/local/bin")} },
#else
                EnvironmentVariables = null,
#endif
                OutputDataReceived = ProgressHandler,
                ErrorDataReceived = ProgressHandler
            };

            var bee = Shell.RunAsync(config);

            yield return progressInfo;

            const int maxBuildTimeInMs = 30 * 60 * 1000; // 30 minutes

            var statusEnum = Shell.WaitForProcess(bee, maxBuildTimeInMs);
            while (statusEnum.MoveNext())
            {
                yield return progressInfo;
            }

            progressInfo.Progress = 1.0f;
            progressInfo.IsDone = true;
            progressInfo.ExitCode = bee.ExitCode;
            progressInfo.Info = "Build completed";
            yield return progressInfo;
        }
    }
}
