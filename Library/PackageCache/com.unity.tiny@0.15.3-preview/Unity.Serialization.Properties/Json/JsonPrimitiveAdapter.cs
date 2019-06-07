using System;
using System.Globalization;
using Unity.Properties;

namespace Unity.Serialization.Json
{
    public class JsonPrimitiveAdapter : JsonVisitorAdapter
        , IVisitAdapterPrimitives
        , IVisitAdapter<string>
        , IVisitAdapter<Guid>
        , IVisitAdapter
    {
        public JsonPrimitiveAdapter(JsonVisitor visitor) : base(visitor)
        {
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref sbyte value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, sbyte>
        {
            Append(property, value, (builder, v) => { builder.Append(v); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref short value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, short>
        {
            Append(property, value, (builder, v) => { builder.Append(v); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref int value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, int>
        {
            Append(property, value, (builder, v) => { builder.Append(v); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref long value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, long>
        {
            Append(property, value, (builder, v) => { builder.Append(v); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref byte value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, byte>
        {
            Append(property, value, (builder, v) => { builder.Append(v); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref ushort value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, ushort>
        {
            Append(property, value, (builder, v) => { builder.Append(v); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref uint value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, uint>
        {
            Append(property, value, (builder, v) => { builder.Append(v); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref ulong value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, ulong>
        {
            Append(property, value, (builder, v) => { builder.Append(v); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref float value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, float>
        {
            Append(property, value, (builder, v) => { builder.Append(v.ToString(CultureInfo.InvariantCulture)); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref double value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, double>
        {
            Append(property, value, (builder, v) => { builder.Append(v.ToString(CultureInfo.InvariantCulture)); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref bool value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, bool>
        {
            Append(property, value, (builder, v) => { builder.Append(v ? "true" : "false"); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref char value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, char>
        {
            Append(property, value, (builder, v) => { builder.Append(EncodeJsonString(string.Empty + v)); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref string value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, string>
        {
            Append(property, value, (builder, v) => { builder.Append(EncodeJsonString(v)); });
                                                   return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref Guid value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, Guid>
        {
            Append(property, value, (builder, v) => { builder.Append(EncodeJsonString(v.ToString("N"))); });
            return VisitStatus.Handled;
        }

        public VisitStatus Visit<TProperty, TContainer, TValue>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, TValue>
        {
            if (typeof(TValue).IsEnum)
            {
                Append(property, value, (builder, v) => { builder.Append((int) (object) v); });
                return VisitStatus.Handled;
            }

            return VisitStatus.Unhandled;
        }
    }
}
