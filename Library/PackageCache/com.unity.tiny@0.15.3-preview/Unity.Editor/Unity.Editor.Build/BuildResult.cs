using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Editor.Extensions;

namespace Unity.Editor.Build
{
    public struct BuildStepStatistics
    {
        public uint Index { get; internal set; }
        public string Name { get; internal set; }
        public TimeSpan Duration { get; internal set; }

        public override string ToString()
        {
            return $"{Index}. {Name}: {Duration.ToShortString()}";
        }
    }

    public struct BuildResult
    {
        public bool Success { get; internal set; }
        public FileInfo Target { get; internal set; }
        public TimeSpan Duration { get; internal set; }
        public IReadOnlyCollection<BuildStepStatistics> Statistics { get; internal set; }

        public override string ToString()
        {
            var stats = string.Join(Environment.NewLine, Statistics.Select(step => step.ToString()));
            return $"Build {(Success ? "succeeded" : "failed")} after {Duration.ToShortString()}.{Environment.NewLine}{stats}";
        }
    }
}
