using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Editor.Build;

namespace Unity.Editor.Tests
{
    internal class BuildTests
    {
        private const string k_ProjectPath = "Assets/Samples/HelloWorld/HelloWorld.project";

        private static object[] GetBuildTestCaseSource()
        {
            var buildTestCases = new List<object[]>();
            foreach (var buildTarget in BuildTargetSettings.AvailableBuildTargets)
            {
                foreach (var configuration in Enum.GetValues(typeof(Configuration)).Cast<Configuration>())
                {
                    buildTestCases.Add(new object[] { k_ProjectPath, buildTarget, configuration });
                }
            }
            return buildTestCases.ToArray();
        }

        [Test, TestCaseSource(nameof(GetBuildTestCaseSource))]
        public void BuildProject(string projectPath, BuildTarget buildTarget, Configuration configuration)
        {
            var projectFile = new FileInfo(projectPath);
            if (!projectFile.Exists)
            {
                UnityEngine.Debug.LogWarning($"Skipping {nameof(BuildProject)} test case since {projectPath} could not be found.");
                return;
            }

            var project = Project.Open(projectFile);
            Assert.IsNotNull(project);
            Assert.IsTrue(Project.Projects.Contains(project));

            try
            {
                var result = BuildPipeline.Build(new BuildSettings()
                {
                    Project = project,
                    BuildTarget = buildTarget,
                    Configuration = configuration,
                    OutputDirectory = Application.OutputDirectory
                });
                Assert.IsTrue(result.Success);
            }
            finally
            {
                project.Dispose();
                Assert.IsFalse(Project.Projects.Contains(project));
            }
        }
    }
}
