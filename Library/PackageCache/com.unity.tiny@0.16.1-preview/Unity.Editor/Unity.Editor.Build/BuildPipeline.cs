using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Unity.Authoring;
using Unity.Editor.Extensions;
using Unity.Editor.Persistence;
using Unity.Editor.Utilities;
using Unity.Entities;

namespace Unity.Editor.Build
{
    /// <summary>
    /// Lists the available configurations.
    /// </summary>
    public enum Configuration
    {
        Debug,
        Develop,
        Release
    }

    internal static class BuildPipeline
    {
        internal class BuildContext
        {
            public BuildContext(BuildSettings settings, ProgressBarScope progress)
            {
                BuildSettings = settings;
                ProgressBar = progress;
                WorldManager = Session.GetManager<IWorldManager>();
                PersistenceManager = Session.GetManager<IPersistenceManager>();

                OutputDirectory.EnsureExists();
                DataDirectory.EnsureExists();
            }

            public BuildSettings BuildSettings { get; }
            public ProgressBarScope ProgressBar { get; }
            public BuildManifest Manifest { get; } = new BuildManifest();
            public Project Project => BuildSettings.Project;
            public Session Session => Project.Session;
            public IWorldManager WorldManager { get; }
            public World World => WorldManager.World;
            public EntityManager EntityManager => WorldManager.EntityManager;
            public IPersistenceManager PersistenceManager { get; }
            public DirectoryInfo OutputDirectory => BuildSettings.OutputDirectory;
            public DirectoryInfo ArtifactsDirectory => OutputDirectory.Combine(Project.Name.ToIdentifier());
            public DirectoryInfo DataDirectory => ArtifactsDirectory.Combine("Data");
            public DirectoryInfo BuildDirectory => OutputDirectory.Combine("build", Project.Name.ToIdentifier(), BeeTargetName);

            public string BeeTargetName
            {
                get
                {
                    var projectName = Project.Name.ToIdentifier();
                    var buildTargetName = BuildSettings.BuildTarget.GetBeeTargetName();
                    var configurationName = BuildSettings.Configuration.ToString();
                    var targetName = $"{projectName}-{buildTargetName}-{configurationName}";
                    return targetName.ToLower();
                }
            }

            public FileInfo TargetExeFile
            {
                get
                {
                    var file = BuildDirectory.GetFile(Project.Name.ToIdentifier());
                    return file.ChangeExtension(BuildSettings.BuildTarget.GetExecutableExtension());
                }
            }
        }

        public static BuildResult Build(BuildSettings buildSettings)
        {
            if (UnityEditor.EditorApplication.isCompiling)
            {
                throw new InvalidOperationException("Building is not allowed while Unity is compiling.");
            }

            var buildSteps = new List<BuildStep.IBuildStep>()
            {
                BuildStep.PackAllSpriteAtlases,
                BuildStep.ExportAssets,
                BuildStep.ExportEntities,
                BuildStep.ExportConfiguration,
                BuildStep.GenerateBeeFiles,
                BuildStep.RunBee
            };

            // Setup build steps per build platform and configuration
            var platform = buildSettings.BuildTarget;
            var configuration = buildSettings.Configuration;
            switch (configuration)
            {
                case Configuration.Debug:
                    break;
                case Configuration.Develop:
                    break;
                case Configuration.Release:
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(configuration), (int)configuration, configuration.GetType());
            }

            // Run build steps
            using (var progress = new ProgressBarScope($"Build {platform.ToString()} {configuration.ToString()}", "Building..."))
            {
                var results = RunBuildSteps(buildSteps.AsReadOnly(), new BuildContext(buildSettings, progress));
                Analytics.SendBuildEvent(buildSettings.Project, results);
                return results;
            }
        }

        private static BuildResult RunBuildSteps(IReadOnlyList<BuildStep.IBuildStep> buildSteps, BuildContext context)
        {
            var result = true;
            var index = 1u;
            var timer = new Stopwatch();
            var stats = new List<BuildStepStatistics>();

            var startTime = DateTime.Now;
            for (var i = 0; i < buildSteps.Count && result; ++i)
            {
                var buildStep = buildSteps[i];
                if (!buildStep.IsEnabled(context))
                {
                    continue;
                }

                context.ProgressBar.Update(buildStep.Description + "...", (float)i / buildSteps.Count);

                try
                {
                    timer.Restart();
                    result = buildStep.Run(context);
                    timer.Stop();

                    stats.Add(new BuildStepStatistics
                    {
                        Index = index++,
                        Name = buildStep.Description,
                        Duration = timer.Elapsed
                    });
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Build step '{buildStep.Description}' failed with exception: {exception}");
                    result = false;
                }
            }

            return new BuildResult
            {
                Success = result,
                Target = context.TargetExeFile,
                Duration = DateTime.Now - startTime,
                Statistics = stats.AsReadOnly()
            };
        }
    }
}
