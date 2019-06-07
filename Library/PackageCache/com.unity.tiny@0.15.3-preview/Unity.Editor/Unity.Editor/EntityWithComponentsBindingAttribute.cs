using System;
using System.Linq;
using Unity.Tiny;

namespace Unity.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class EntityWithComponentsBindingAttribute : Attribute, IEquatable<EntityWithComponentsBindingAttribute>
    {
        public Type[] Types { get; }

        public EntityWithComponentsBindingAttribute(params Type[] types)
        {
            Types = types;
        }

        public EntityWithComponentsBindingAttribute(EntityWithComponentsAttribute attr) :
            this(attr.Types)
        {
        }

        public bool Equals(EntityWithComponentsBindingAttribute other)
        {
            return other != null ? Types.SequenceEqual(other.Types) : false;
        }

        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }

            if (other is EntityWithComponentsBindingAttribute attr)
            {
                return Equals(attr);
            }

            return false;
        }

        public static bool operator ==(EntityWithComponentsBindingAttribute lhs, EntityWithComponentsBindingAttribute rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (lhs == null || rhs == null)
            {
                return false;
            }

            return lhs.Types.SequenceEqual(rhs.Types);
        }

        public static bool operator !=(EntityWithComponentsBindingAttribute lhs, EntityWithComponentsBindingAttribute rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            return Types.Sum(type => hash * 23 + type.GetHashCode());
        }

        public override string ToString()
        {
            return string.Concat(Types.Select(t => t.Name));
        }
    }
}
