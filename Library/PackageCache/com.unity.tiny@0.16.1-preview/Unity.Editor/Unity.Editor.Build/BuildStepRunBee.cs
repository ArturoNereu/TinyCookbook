using System;
using System.IO;
using System.Text;
using Unity.Editor.Extensions;
using Unity.Editor.MenuItems;
using Unity.Editor.Tools;

namespace Unity.Editor.Build
{
    internal partial class BuildStep
    {
        internal static IBuildStep RunBee => new BuildStepRunBee();

        private const string GetEditorToolsTarget = "get-editor-tools";
        private const string CompileEditorToolsTarget = "compile-editor-tools";

        private static string ResolveEditorToolsTargetName()
        {
            return File.Exists(EditorToolsCompilation.CompileEditorToolsFlagFilePath)
                ? CompileEditorToolsTarget
                : GetEditorToolsTarget;
        }


        private class BuildStepRunBee : IBuildStep
        {
            public string Description => "Running bee";

            public bool IsEnabled(BuildPipeline.BuildContext context) => true;

            public bool Run(BuildPipeline.BuildContext context)
            {
                var commandOutput = new StringBuilder();
                var buildOutput = new StringBuilder();

                const string commandExt =
#if UNITY_EDITOR_WIN
                    ".bat";
#else
                    ".sh";
#endif

                var result = false;
                try
                {
                    var buildProgress = BeeTools.Run(ResolveEditorToolsTargetName() + " " + context.BeeTargetName, commandOutput, buildOutput, context.OutputDirectory);
                    while (buildProgress.MoveNext())
                    {
                        context.ProgressBar?.Update(buildProgress.Current.Info, buildProgress.Current.Progress);
                    }

                    result = buildProgress.Current.ExitCode == 0;
                }
                finally
                {
                    // Write runbuild file
                    var runBuildFile = context.OutputDirectory.GetFile("runbuild" + commandExt);
                    runBuildFile.UpdateAllText(commandOutput.ToString());

                    // Write build log file
                    var buildLogFile = context.OutputDirectory.GetFile("build.log");
                    buildLogFile.WriteAllText(buildOutput.ToString(), Encoding.UTF8);

                    // Log build error to Unity console
                    if (!result)
                    {
                        if (buildLogFile.Exists)
                        {
                            var logLines = buildLogFile.ReadAllLines(Encoding.UTF8);
                            var lineIndex = logLines.Length - 1;
                            for (; lineIndex >= 0; --lineIndex)
                            {
                                if (logLines[lineIndex] == "##### ExitCode")
                                {
                                    break;
                                }
                            }
                            var buildLogTail = new StringBuilder($"Build failed. Open {buildLogFile.FullName.HyperLink()} for more details.{Environment.NewLine}");
                            for (var i = Math.Max(0, lineIndex); i < logLines.Length; ++i)
                            {
                                buildLogTail.AppendLine(logLines[i]);
                            }
                            Debug.LogError(buildLogTail);
                        }
                        else
                        {
                            Debug.LogError("Build failed, no build.log file found.");
                        }
                    }
                }

                return result;
            }
        }
    }
}
