using Unity.Entities;
using Unity.Properties;
using Unity.Serialization.Json;

namespace Unity.Editor.Serialization
{
    internal class NativeStringAdapter : JsonVisitorAdapter,
        IVisitAdapter<NativeString64>,
        IVisitAdapter<NativeString512>,
        IVisitAdapter<NativeString4096>
    {
        public NativeStringAdapter(JsonVisitor visitor) : base(visitor)
        {
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref NativeString64 value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, NativeString64>
        {
            Append(property, value.ToString(), (builder, v) => { builder.Append(EncodeJsonString(v.ToString())); });
            return VisitStatus.Override;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref NativeString512 value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, NativeString512>
        {
            Append(property, value.ToString(), (builder, v) => { builder.Append(EncodeJsonString(v.ToString())); });
            return VisitStatus.Override;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref NativeString4096 value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, NativeString4096>
        {
            Append(property, value.ToString(), (builder, v) => { builder.Append(EncodeJsonString(v.ToString())); });
            return VisitStatus.Override;
        }
    }
}