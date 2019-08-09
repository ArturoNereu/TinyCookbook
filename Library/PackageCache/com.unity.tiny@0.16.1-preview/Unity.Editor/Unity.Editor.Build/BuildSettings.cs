using System.IO;

namespace Unity.Editor.Build
{
    public class BuildSettings
    {
        public Project Project { get; set; }
        public BuildTarget BuildTarget { get; set; }
        public Configuration Configuration { get; set; }
        public DirectoryInfo OutputDirectory { get; set; }
    }
}
