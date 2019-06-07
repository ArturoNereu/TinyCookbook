using System;
using Unity.Entities;

namespace Unity.Authoring.Core
{
    /// <summary>
    /// Component that allows you to map an Entity back to `UnityEngine.Object`.
    /// </summary>
    [HideInInspector]
    public struct AssetReference : IComponentData, IEquatable<AssetReference>, IComparable<AssetReference>
    {
        public Guid Guid;
        public long FileId;
        public int Type;

        public override bool Equals(object obj)
        {
            return obj is AssetReference assetDatabaseReference ? Equals(assetDatabaseReference) : false;
        }

        public bool Equals(AssetReference other)
        {
            return Guid == other.Guid && FileId == other.FileId && Type == other.Type;
        }

        public static bool operator ==(AssetReference lhs, AssetReference rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(AssetReference lhs, AssetReference rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            var hashCode = 1377581207;
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            hashCode = hashCode * -1521134295 + FileId.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            return hashCode;
        }

        public int CompareTo(AssetReference other)
        {
            if (Guid != other.Guid)
            {
                return Guid.CompareTo(other.Guid);
            }

            if (FileId != other.FileId)
            {
                return FileId > other.FileId ? 1 : -1;
            }

            if (Type != other.Type)
            {
                return Type > other.Type ? 1 : -1;
            }

            return 0;
        }
    }
}
