using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Unity.Editor.Build
{
    internal class BuildManifest
    {
        private readonly HashSet<Entry> m_Entries = new HashSet<Entry>();

        public class Entry : IEquatable<Entry>
        {
            public Guid AssetGuid;
            public string AssetPath;
            public uint ExportVersion;
            public Guid ExportHash;
            public List<string> ExportedFiles = new List<string>();

            public bool Equals(Entry other)
            {
                return AssetGuid == other.AssetGuid;
            }

            public override int GetHashCode()
            {
                return AssetGuid.GetHashCode();
            }
        }

        public IReadOnlyDictionary<Guid, string> Assets => m_Entries.ToDictionary(x => x.AssetGuid, x => x.AssetPath);
        public IReadOnlyCollection<FileInfo> ExportedFiles => m_Entries.SelectMany(x => x.ExportedFiles.Select(f => new FileInfo(f))).ToList();

        public Entry Add(Guid assetGuid, string assetPath, IEnumerable<FileInfo> exportedFiles, uint exportVersion = 1, Guid exportHash = new Guid())
        {
            var entry = new Entry
            {
                AssetGuid = assetGuid,
                AssetPath = assetPath,
                ExportVersion = exportVersion,
                ExportHash = exportHash,
                ExportedFiles = exportedFiles.Select(f => f.FullName).ToList()
            };
            return m_Entries.Add(entry) ? entry : null;
        }
    }
}
