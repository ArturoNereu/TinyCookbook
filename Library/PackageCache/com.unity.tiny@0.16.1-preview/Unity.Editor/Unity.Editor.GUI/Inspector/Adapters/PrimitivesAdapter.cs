using Unity.Properties;

namespace Unity.Editor
{
    internal sealed class PrimitivesAdapter<T> : InspectorAdapter<T>,
        IVisitAdapterPrimitives,
        IVisitAdapter
        where T : struct
    {
        public PrimitivesAdapter(InspectorVisitor<T> visitor) : base(visitor)
        {
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref sbyte value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, sbyte>
        {
            GuiFactory.SByteField(property, ref container, ref value, Context);
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref short value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, short>
        {
            GuiFactory.ShortField(property, ref container, ref value, Context);
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref int value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, int>
        {
            GuiFactory.IntField(property, ref container, ref value, Context);
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref long value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, long>
        {
            GuiFactory.LongField(property, ref container, ref value, Context);
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref byte value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, byte>
        {
            GuiFactory.ByteField(property, ref container, ref value, Context);
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref ushort value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, ushort>
        {
            GuiFactory.UShortField(property, ref container, ref value, Context);
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref uint value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, uint>
        {
            GuiFactory.UIntField(property, ref container, ref value, Context);
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref ulong value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, ulong>
        {
            GuiFactory.ULongField(property, ref container, ref value, Context);
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref float value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, float>
        {
            GuiFactory.FloatField(property, ref container, ref value, Context);
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref double value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, double>
        {
            GuiFactory.DoubleField(property, ref container, ref value, Context);
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref bool value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, bool>
        {
            GuiFactory.Toggle(property, ref container, ref value, Context);
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref char value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, char>
        {
            GuiFactory.CharField(property, ref container, ref value, Context);
            return VisitStatus.Handled;
        }


        public VisitStatus Visit<TProperty, TContainer, TValue>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref TValue value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, TValue>
        {
            if (!typeof(TValue).IsEnum)
            {
                return VisitStatus.Unhandled;
            }

            GuiFactory.EnumField(property, ref container, ref value, Context);
            return VisitStatus.Override;
        }
    }
}
