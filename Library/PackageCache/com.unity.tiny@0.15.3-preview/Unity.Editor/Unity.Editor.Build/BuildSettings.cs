using System.IO;

namespace Unity.Editor.Build
{
    public class BuildSettings
    {
        public Project Project { get; set; }
        public Platform Platform { get; set; }
        public Configuration Configuration { get; set; }
        public DirectoryInfo OutputDirectory { get; set; }
    }
}
