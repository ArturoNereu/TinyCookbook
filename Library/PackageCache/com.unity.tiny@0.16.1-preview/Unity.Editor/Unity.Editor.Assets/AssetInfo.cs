using System;
using System.Collections.Generic;

namespace Unity.Editor.Assets
{
    internal class AssetInfo : IEquatable<AssetInfo>
    {
        private AssetInfo m_Parent;
        private readonly List<AssetInfo> m_Children = new List<AssetInfo>();

        /// <summary>
        /// The object that this asset references.
        /// </summary>
        public UnityEngine.Object Object { get; }

        /// <summary>
        /// This asset's name, which can be different from the object's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// This asset's parent (if any).
        /// </summary>
        public AssetInfo Parent
        {
            get => m_Parent;
            set
            {
                // Remove previous parent
                if (m_Parent != null)
                {
                    m_Parent.RemoveChild(this);
                }

                // Set new parent
                m_Parent = value;

                // Add to parent's children list
                if (m_Parent != null)
                {
                    m_Parent.AddChild(this);
                }
            }
        }

        /// <summary>
        /// This asset's children.
        /// </summary>
        public IReadOnlyList<AssetInfo> Children => m_Children;

        public AssetInfo(UnityEngine.Object obj, string name, AssetInfo parent = null)
        {
            Object = obj;
            Name = name;
            Parent = parent;
        }

        public bool Equals(AssetInfo other)
        {
            return other != null ? Object == other.Object : false;
        }

        public override bool Equals(object other)
        {
            return other is AssetInfo assetInfo ? Equals(assetInfo) : false;
        }

        public override int GetHashCode()
        {
            return Object.GetHashCode();
        }

        private void AddChild(AssetInfo assetInfo)
        {
            if (m_Children.Contains(assetInfo))
            {
                return;
            }
            m_Children.Add(assetInfo);
        }

        private void RemoveChild(AssetInfo assetInfo)
        {
            m_Children.Remove(assetInfo);
        }
    }
}
