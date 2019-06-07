using Unity.Authoring;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Reflection;
using Unity.Properties;
using Unity.Tiny.Core;

namespace Unity.Editor
{
    internal class InspectorVisitor<T> : PropertyVisitor
        where T : struct
    {
        public readonly NativeArray<T> Targets;
        public readonly Session Session;

        public InspectorContext Context { get; }

        public InspectorVisitor(Session session, NativeArray<T> targets)
        {
            Context = new InspectorContext();
            Targets = targets;
            Session = session;
        }

        public override bool IsExcluded<TProperty, TContainer, TValue>(TProperty property, ref TContainer container)
        {
            return property.Attributes?.HasAttribute<HideInInspectorAttribute>() ?? AttributeCache<TValue>.HasAttribute<HideInInspectorAttribute>()
                   || typeof(EntityGuid).IsAssignableFrom(typeof(TValue))
                   || typeof(DynamicBufferContainer<EntityName>).IsAssignableFrom(typeof(TValue));
        }

        protected override VisitStatus BeginContainer<TProperty, TContainer, TValue>(TProperty property, ref TContainer container,
            ref TValue value, ref ChangeTracker changeTracker)
        {
            if (container is Wrapper<TValue> wrapper)
            {
                PropertyContainer.Visit(ref value, this);
                return VisitStatus.Override;
            }

            var foldout = GuiFactory.Foldout(property, ref container, ref value, Context);
            using (Context.NewOffsetScope(typeof(TValue), property.GetName()))
            {
                PropertyContainer.Visit(ref value, this);
            }

            Context.PopParent(foldout);
            return VisitStatus.Override;
        }

        protected override void EndContainer<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value,
            ref ChangeTracker changeTracker)
        {
        }
    }
}
