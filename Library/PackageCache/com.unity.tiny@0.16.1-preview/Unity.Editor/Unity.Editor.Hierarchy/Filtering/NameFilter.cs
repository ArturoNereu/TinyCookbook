using System;

namespace Unity.Editor
{
    internal struct NameFilter
    {
        [Flags]
        public enum NameComparer
        {
            Contains = 0,
            StartsWith = 1 << 0,
            EndsWith = 1 << 1,
            Exact = StartsWith | EndsWith,
        }

        public string Name { get; set; }
        public NameComparer Comparer { get; set; }

        public bool Inverted { get; set; }

        public bool Keep(string name)
        {
            if (string.IsNullOrEmpty(Name))
            {
                return true;
            }

            return Compare(name, Name, Comparer) ^ Inverted;
        }

        private static bool Compare(string lhs, string rhs, NameComparer nameComparer)
        {
            if (string.IsNullOrEmpty(lhs) || string.IsNullOrEmpty(rhs))
            {
                return true;
            }

            switch (nameComparer)
            {
                case NameComparer.Contains:
                    return lhs.IndexOf(rhs, StringComparison.OrdinalIgnoreCase) >= 0;
                case NameComparer.StartsWith:
                    return lhs.StartsWith(rhs, StringComparison.OrdinalIgnoreCase);
                case NameComparer.EndsWith:
                    return lhs.EndsWith(rhs, StringComparison.OrdinalIgnoreCase);
                case NameComparer.Exact:
                    return string.Equals(lhs, rhs, StringComparison.OrdinalIgnoreCase);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
