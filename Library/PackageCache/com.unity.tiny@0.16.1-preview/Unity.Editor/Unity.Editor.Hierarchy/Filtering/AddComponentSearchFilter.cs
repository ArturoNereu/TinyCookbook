using System.Collections.Generic;
using System.Linq;

namespace Unity.Editor
{
    internal class AddComponentSearchFilter
    {
        private readonly NameFilter[] m_Names;

        internal AddComponentSearchFilter(List<NameFilter> names)
        {
            m_Names = names.ToArray();
        }

        public bool Keep(string name)
        {
            return m_Names.All(filter => filter.Keep(name));
        }
    }
}