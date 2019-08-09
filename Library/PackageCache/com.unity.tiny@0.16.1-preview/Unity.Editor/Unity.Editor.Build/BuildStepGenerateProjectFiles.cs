using System.Text;
using Unity.Editor.Tools;

namespace Unity.Editor.Build
{
    internal partial class BuildStep
    {
        internal static IBuildStep GenerateProjectFiles => new BuildStepGenerateProjectFiles();

        private class BuildStepGenerateProjectFiles : IBuildStep
        {
            public string Description => "Generating project files";

            public bool IsEnabled(BuildPipeline.BuildContext context) => true;

            public bool Run(BuildPipeline.BuildContext context)
            {
                var commandOutput = new StringBuilder();
                var buildOutput = new StringBuilder();
                var buildProgress = BeeTools.Run("ProjectFiles", commandOutput, buildOutput, context.OutputDirectory);
                while (buildProgress.MoveNext())
                {
                    context.ProgressBar?.Update(buildProgress.Current.Info, buildProgress.Current.Progress);
                }
                return buildProgress.Current.ExitCode == 0;
            }
        }
    }
}
