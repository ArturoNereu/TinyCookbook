using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Editor.Extensions;
using Assert = UnityEngine.Assertions.Assert;
using Debug = UnityEngine.Debug;

namespace Unity.Editor.Tools
{
    internal class ShellProcessOutput
    {
        public bool Succeeded { get; set; } = true;
        public string Command { get; set; }
        public StringBuilder CommandOutput { get; set; }
        public string FullOutput { get; set; }
        public string ErrorOutput { get; set; }
        public int ExitCode { get; set; }
    }

    internal class ShellProcessArgs
    {
        public const int DefaultMaxIdleTimeInMilliseconds = 30000;

        public static readonly ShellProcessArgs Default = new ShellProcessArgs();

        public string Executable { get; set; }
        public IEnumerable<string> Arguments { get; set; }
        public DirectoryInfo WorkingDirectory { get; set; }
        public IEnumerable<string> ExtraPaths { get; set; }
        public IReadOnlyDictionary<string, string> EnvironmentVariables { get; set; }
        public int MaxIdleTimeInMilliseconds { get; set; } = DefaultMaxIdleTimeInMilliseconds;
        public DataReceivedEventHandler OutputDataReceived { get; set; }
        public DataReceivedEventHandler ErrorDataReceived { get; set; }
        public bool ThrowOnError { get; set; } = true;
    }

    internal static class Shell
    {
#if UNITY_EDITOR_WIN
        private const char k_PathSeparator = ';';
#else
        private const char k_PathSeparator = ':';
#endif

        public static ShellProcessOutput Run(ShellProcessArgs shellArgs)
        {
            Assert.IsNotNull(shellArgs);
            Assert.IsFalse(string.IsNullOrEmpty(shellArgs.Executable));

            var command = string.Join(" ", shellArgs.Executable.AsArray().Concat(shellArgs.Arguments));
            try
            {
                var runOutput = new ShellProcessOutput();
                var hasErrors = false;
                var output = new StringBuilder();
                var logOutput = new StringBuilder();
                var errorOutput = new StringBuilder();

                // Setup shell command
                if (shellArgs.ExtraPaths != null)
                {
                    var extraPaths = string.Join(k_PathSeparator.ToString(), shellArgs.ExtraPaths.Select(p => p.DoubleQuoted()));
#if UNITY_EDITOR_WIN
                    command = $"SET PATH={extraPaths}{k_PathSeparator}%PATH%{Environment.NewLine}{command}";
#else
                    command = $"export PATH={extraPaths}{k_PathSeparator}$PATH{Environment.NewLine}{command}";
#endif
                }

                LogProcessData($"TINY SHELL> {shellArgs.WorkingDirectory?.FullName ?? Application.RootDirectory.FullName}", logOutput);
                LogProcessData(command, logOutput);

                // Setup temporary command file
                var tmpCommandFile = Path.GetTempPath() + Guid.NewGuid().ToString();
#if UNITY_EDITOR_WIN
                tmpCommandFile += ".bat";
#else
                tmpCommandFile += ".sh";
#endif
                File.WriteAllText(tmpCommandFile, command);

                // Prepare data received handlers
                DataReceivedEventHandler outputReceived = (sender, e) =>
                {
                    LogProcessData(e.Data, output);
                    logOutput.AppendLine(e.Data);
                };
                DataReceivedEventHandler errorReceived = (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorOutput.AppendLine(e.Data);
                        hasErrors = true;
                    }
                    LogProcessData(e.Data, output);
                    logOutput.AppendLine(e.Data);
                };

                // Run command in shell and wait for exit
                try
                {
                    using (var process = StartProcess(new ShellProcessArgs()
                    {
#if UNITY_EDITOR_WIN
                        Executable = "cmd.exe",
                        Arguments = new string[] { "/Q", "/C", tmpCommandFile.DoubleQuoted() },
#else
                        Executable = "bash",
                        Arguments = tmpCommandFile.DoubleQuoted().AsArray(),
#endif
                        WorkingDirectory = shellArgs.WorkingDirectory,
                        OutputDataReceived = outputReceived,
                        ErrorDataReceived = errorReceived
                    }))
                    {
                        var processUpdate = WaitForProcess(process, shellArgs.MaxIdleTimeInMilliseconds);
                        while (processUpdate.MoveNext())
                        {
                        }
                        var exitCode = runOutput.ExitCode = processUpdate.Current == ProcessStatus.Killed ? -1 : process.ExitCode;
                        runOutput.Command = command;
                        runOutput.CommandOutput = output;
                        runOutput.FullOutput = logOutput.ToString();
                        runOutput.ErrorOutput = errorOutput.ToString();
                        LogProcessData($"Process exited with code '{exitCode}'", logOutput);
                        hasErrors |= (exitCode != 0);
                    }
                }
                finally
                {
                    File.Delete(tmpCommandFile);
                }

                if (hasErrors && shellArgs.ThrowOnError)
                {
                    throw new Exception(errorOutput.ToString());
                }

                runOutput.Succeeded = !hasErrors;
                return runOutput;
            }
            catch (Exception)
            {
                //TinyEditorAnalytics.SendException(nameof(Shell.Run), e);
                throw;
            }
        }

        private static void LogProcessData(string data, StringBuilder output)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            Console.WriteLine(data); // Editor.log
            output.AppendLine(data);
        }

        public static Process RunAsync(ShellProcessArgs args)
        {
            return StartProcess(args);
        }

        private static Process StartProcess(ShellProcessArgs args)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = args.Executable,
                Arguments = string.Join(" ", args.Arguments),
                WorkingDirectory = args.WorkingDirectory?.FullName ?? new DirectoryInfo(".").FullName,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            if (args.EnvironmentVariables != null)
            {
                foreach (var pair in args.EnvironmentVariables)
                {
                    startInfo.EnvironmentVariables[pair.Key] = pair.Value;
                }
            }

            var process = new Process { StartInfo = startInfo };

            if (args.OutputDataReceived != null)
            {
                process.OutputDataReceived += args.OutputDataReceived;
            }

            if (args.ErrorDataReceived != null)
            {
                process.ErrorDataReceived += args.ErrorDataReceived;
            }

            process.Start();

            if (args.OutputDataReceived != null)
            {
                process.BeginOutputReadLine();
            }

            if (args.ErrorDataReceived != null)
            {
                process.BeginErrorReadLine();
            }

            return process;
        }

        public enum ProcessStatus
        {
            Running,
            Killed,
            Done
        }

        public static IEnumerator<ProcessStatus> WaitForProcess(Process process, int maxIdleTimeInMs, int yieldFrequencyInMs = 30)
        {
            var totalWaitInMs = 0;
            for (; ; )
            {
                if (process.WaitForExit(yieldFrequencyInMs))
                {
                    // WaitForExit with a timeout will not wait for async event handling operations to finish.
                    // To ensure that async event handling has been completed, call WaitForExit that takes no parameters.
                    // See remarks: https://msdn.microsoft.com/en-us/library/ty0d8k56(v=vs.110)
                    process.WaitForExit();
                    yield return ProcessStatus.Done;
                    break;
                }

                totalWaitInMs += yieldFrequencyInMs;

                if (totalWaitInMs < maxIdleTimeInMs)
                {
                    yield return ProcessStatus.Running;
                    continue;
                }

                // idle for too long with no output? -> kill
                // nb: testing the process threads WaitState doesn't work on OSX
                Debug.LogError("Idle process detected. See console for more details.");
                process.Kill();
                yield return ProcessStatus.Killed;
                break;
            }
        }
    }
}
