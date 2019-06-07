using Unity.Authoring;
using Unity.Entities;
using Unity.Properties;

namespace Unity.Editor
{
    internal sealed class EntityContainerAdapter : InspectorAdapter<Entity>
        , IVisitAdapter<EntityContainer>
        , IVisitAdapter<EntityContainer, Entity>
        , IVisitContainerAdapter
        , IVisitContainerAdapterC<EntityContainer>
    {
        private EntityManager EntityManager { get; }
        private CustomInspectorManager CustomInspectors { get; }

        public EntityContainerAdapter(InspectorVisitor<Entity> visitor) : base(visitor)
        {
            CustomInspectors = Session.GetManager<CustomInspectorManager>();
            EntityManager = Session.GetManager<IWorldManager>().EntityManager;
        }

        public VisitStatus Visit<TProperty>(IPropertyVisitor visitor, TProperty property, ref EntityContainer container,
            ref Entity value, ref ChangeTracker changeTracker) where TProperty : IProperty<EntityContainer, Entity>
        {
            var header = new EntityHeaderElement(Session, Targets);
            header.BuildFromInspector(visitor, property, ref container, ref value, Context);
            return VisitStatus.Override;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
            ref EntityContainer value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, EntityContainer>
        {
            PropertyContainer.Visit(ref value, Visitor);
            return VisitStatus.Override;
        }

        public VisitStatus BeginContainer<TProperty, TValue>(IPropertyVisitor visitor, TProperty property,
            ref EntityContainer container, ref TValue value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<EntityContainer, TValue>
        {
            var dataType = typeof(TValue);
            var componentType = dataType;

            // Early exit if not a top level component
            if (!typeof(IComponentData).IsAssignableFrom(dataType) &&
                !typeof(ISharedComponentData).IsAssignableFrom(dataType) &&
                !typeof(IDynamicBufferContainer).IsAssignableFrom(dataType))
            {
                return VisitStatus.Unhandled;
            }

            if (value is IDynamicBufferContainer buffer)
            {
                componentType = buffer.ElementType;
            }

            // Early exit if not all the entities have the component
            for (var i = 0; i < Targets.Length; ++i)
            {
                if (!EntityManager.HasComponent(Targets[i], componentType))
                {
                    return VisitStatus.Override;
                }
            }

            var componentElement = CustomInspectors.CreateComponentDataElement(Targets, ref value);

            if (value is IDynamicBufferContainer)
            {
                using (Context.NewStructItemInspectorFactoryScope(componentElement, CustomInspectors, componentType))
                using (Context.NewDataProviderScope(componentElement))
                using (Context.NewOffsetScope(dataType, null))
                {
                    componentElement.BuildFromVisitor(visitor, property, ref container, ref value, Context);
                }
            }
            else
            {
                using (Context.NewStructInspectorFactoryScope<TValue>(componentElement, CustomInspectors))
                using (Context.NewDataProviderScope(componentElement))
                using (Context.NewOffsetScope(dataType, null))
                {
                    componentElement.BuildFromVisitor(visitor, property, ref container, ref value, Context);
                }
            }

            return VisitStatus.Override;
        }

        public void EndContainer<TProperty, TValue>(IPropertyVisitor visitor, TProperty property, ref EntityContainer container,
            ref TValue value, ref ChangeTracker changeTracker) where TProperty : IProperty<EntityContainer, TValue>
        {
        }

        public VisitStatus BeginContainer<TProperty, TValue, TContainer>(IPropertyVisitor visitor, TProperty property,
            ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, TValue>
        {
            if (!typeof(TValue).IsValueType || null == Context.StructElementFactory)
            {
                return VisitStatus.Unhandled;
            }

            var name = property.GetName();
            using (Context.NewOffsetScope(typeof(TValue), name))
            {
                var offset = Context.GetCurrentOffset();
                var index = Context.GetCurrentIndex();
                var isItem = Context.IsCurrentList();
                IStructDataElement structElement;
                if (!isItem)
                {
                    structElement =
                        Context.StructElementFactory.CreateForType<TValue>(name, index, offset);
                }
                else
                {
                    structElement =
                        Context.StructElementFactory.CreateItemForType<TValue>(name, index, offset);
                }

                structElement.BuildFromVisitor(visitor, property, ref container, ref value, Context);
            }

            return VisitStatus.Override;
        }

        public void EndContainer<TProperty, TValue, TContainer>(IPropertyVisitor visitor, TProperty property,
            ref TContainer container,
            ref TValue value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, TValue>
        {
        }
    }
}
