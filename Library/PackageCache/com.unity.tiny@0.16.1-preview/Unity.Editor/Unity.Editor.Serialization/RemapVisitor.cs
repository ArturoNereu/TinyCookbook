using System;
using Unity.Collections;
using Unity.Properties;

namespace Unity.Editor.Serialization
{
    internal class RemapVisitor<T> : PropertyVisitor
        where T : struct, IEquatable<T>
    {
        private class Adapter : IPropertyVisitorAdapter,
            IVisitAdapter<T>
        {
            private readonly NativeHashMap<T, T> m_Remap;

            public Adapter(NativeHashMap<T, T> remap)
            {
                m_Remap = remap;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref T value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, T>
            {
                if (m_Remap.TryGetValue(value, out var newValue))
                {
                    value = newValue;
                }

                return VisitStatus.Handled;
            }
        }

        public RemapVisitor(NativeHashMap<T, T> remap)
        {
            AddAdapter(new Adapter(remap));
        }
    }
}
