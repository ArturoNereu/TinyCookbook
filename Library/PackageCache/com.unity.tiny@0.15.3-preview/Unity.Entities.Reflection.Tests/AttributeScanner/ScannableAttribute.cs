using System;

namespace Unity.Entities.Reflection.Tests
{
    /// <summary>
    /// Base class for all attributes that can be scanned by the <see cref="AttributeScanner{T}"/>.
    /// </summary>
    internal abstract class ScannableAttribute : Attribute, IComparable<ScannableAttribute>
    {
        /// <summary>
        /// Returns the order that attributes of the same type will be sorted by.
        /// </summary>
        public int Order { get; }

        protected internal ScannableAttribute(int order)
        {
            Order = order;
        }

        int IComparable<ScannableAttribute>.CompareTo(ScannableAttribute other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Order.CompareTo(other.Order);
        }
    }
}
