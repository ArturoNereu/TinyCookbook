using System;
using System.Linq;

namespace Unity.Editor.Bindings
{
    internal readonly struct BindingConfiguration : IEquatable<BindingConfiguration>
    {
        public static BindingConfiguration Null = default;
        public BindingProfile[] Bindings { get; }

        public BindingConfiguration(BindingProfile[] bindings)
        {
            Bindings = bindings;
        }

        public bool Equals(BindingConfiguration other)
        {
            if (null == Bindings)
            {
                return null == other.Bindings;
            }

            return null != other.Bindings && Bindings.SequenceEqual(other.Bindings);
        }

        public static bool operator ==(BindingConfiguration lhs, BindingConfiguration rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(BindingConfiguration lhs, BindingConfiguration rhs)
        {
            return !(lhs == rhs);
        }
        
        public override bool Equals(object obj)
        {
            return obj is BindingConfiguration other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Bindings != null ? Bindings.GetHashCode() : 0);
        }
    }
}

