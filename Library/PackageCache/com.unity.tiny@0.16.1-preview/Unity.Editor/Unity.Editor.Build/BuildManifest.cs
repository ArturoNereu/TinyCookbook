using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Unity.Editor.Build
{
    internal class BuildManifest
    {
        private readonly HashSet<Entry> m_Entries = new HashSet<Entry>();

        private class Entry : IEquatable<Entry>
        {
            public Guid AssetGuid;
            public string AssetPath;
            public List<string> ExportedFiles;

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

        public void Add(Guid assetGuid, string assetPath, IEnumerable<FileInfo> exportedFiles)
        {
            if (exportedFiles == null || exportedFiles.Count() == 0)
            {
                return;
            }

            m_Entries.Add(new Entry
            {
                AssetGuid = assetGuid,
                AssetPath = assetPath ?? string.Empty,
                ExportedFiles = exportedFiles.Select(f => f.FullName).ToList()
            });
        }
    }
}
