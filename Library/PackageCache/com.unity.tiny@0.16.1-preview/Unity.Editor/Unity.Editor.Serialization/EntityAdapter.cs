using Unity.Entities;
using Unity.Properties;
using Unity.Serialization.Json;

namespace Unity.Editor.Serialization
{
    internal class EntityAdapter : JsonVisitorAdapter,
        IVisitAdapter<Entity>
    {
        private readonly EntityManager m_EntityManager;

        public EntityAdapter(JsonVisitor visitor, EntityManager entityManager) : base(visitor)
        {
            m_EntityManager = entityManager;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref Entity value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, Entity>
        {
            if (!m_EntityManager.Exists(value))
            {
                return VisitStatus.Override;
            }

            var guid = m_EntityManager.GetComponentData<EntityGuid>(value);
            if (guid == EntityGuid.Null)
            {
                return VisitStatus.Override;
            }

            Append(property, guid, (builder, v) => { builder.Append(EncodeJsonString(v.ToString())); });
            return VisitStatus.Override;
        }
    }
}
