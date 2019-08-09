using System;
using System.Collections.Generic;
using UnityEditor.Searcher;

namespace Unity.Editor
{
    internal class TypeSearcherItem : SearcherItem
    {
        public Type Type { get; }

        public TypeSearcherItem(Type type, string help = "", List<SearcherItem> children = null) : base(type.Name, help, children)
        {
            Type = type;
        }
    }
}
