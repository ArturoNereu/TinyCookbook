using JetBrains.Annotations;
using System;
using System.IO;
using System.Text;
using Unity.Editor.Build;
using UnityEditor;
using BuildPipeline = Unity.Editor.Build.BuildPipeline;

namespace Unity.Editor.MenuItems
{
    internal static class EditorToolsCompilation
    {
        public static string CompileEditorToolsFlagFilePath = Path.Combine(Application.OutputDirectory.FullName, "compile-editor-tools-from-source-flag");
        private const string MenuPath = "Tools/Enable editor tools compilation";

        [UsedImplicitly, CommandHandler(CommandIds.Tools.EnableCompileEditorTools, CommandHint.Menu)]
        public static void EnableEditorToolsCompilationMenuItemHandler(CommandExecuteContext context)
        {
            if (Menu.GetChecked(MenuPath))
            {
                DisableEditorToolsCompilation();
            }
            else
            {
                EnableEditorToolsCompilation();
            }

            Menu.SetChecked(MenuPath, File.Exists(CompileEditorToolsFlagFilePath));
        }

        private static void EnableEditorToolsCompilation()
        {
            var command = new StringBuilder();
            var output = new StringBuilder();
            using (var progressBar = new Utilities.ProgressBarScope("Compile editor tools ...", "Building..."))
            {
                var buildContext = new BuildPipeline.BuildContext(new BuildSettings
                {
                    Project = Application.AuthoringProject,
                    BuildTarget = new DesktopDotNetBuildTarget(),
                    Configuration = Configuration.Debug,
                    OutputDirectory = Application.OutputDirectory
                }, progressBar);

                var flagFile = new FileInfo(CompileEditorToolsFlagFilePath);
                try
                {
                    if (!flagFile.Exists)
                        flagFile.Create().Dispose();

                    if (!BuildStep.GenerateBeeFiles.Run(buildContext))
                    {
                        Debug.LogError($"Failed to generate bee files.\n");
                        return;
                    }

                    var buildProgress = Tools.BeeTools.Run("compile-editor-tools", command, output, buildContext.OutputDirectory);
                    while (buildProgress.MoveNext())
                    {
                        var info = buildProgress.Current;
                        progressBar.Update(info.Info, info.Progress);
                    }

                    var success = buildProgress.Current.ExitCode == 0;
                    if (!success)
                        Debug.LogError("Failed to compile editor tools\n" + command + "\n" + output);
                    else
                        Debug.Log("Editor tools compiled successfully.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to build DOTS editor tools.\n{e}");
                }
            }
        }

        static void DisableEditorToolsCompilation()
        {
            if (!File.Exists(CompileEditorToolsFlagFilePath))
                return;

            try
            {
                File.Delete(CompileEditorToolsFlagFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Unable to delete flag file: {CompileEditorToolsFlagFilePath}. Try to delete it manually.\n{e}");
            }
        }
    }
}
