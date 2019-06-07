using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Editor.Assets;
using Unity.Entities;
using Unity.Properties;
using Unity.Tiny.Scenes;
using Object = UnityEngine.Object;

namespace Unity.Editor.Persistence
{
    /// <summary>
    /// While not the most performant it solves our deterministic ordering problem for now.
    /// </summary>
    internal struct EntityGuidComparer : IComparer<Entity>
    {
        private readonly EntityManager m_EntityManager;

        public EntityGuidComparer(EntityManager entityManager)
        {
            m_EntityManager = entityManager;
        }

        public int Compare(Entity x, Entity y)
        {
            if (!m_EntityManager.HasComponent<EntityGuid>(x) || !m_EntityManager.HasComponent<EntityGuid>(y))
            {
                return 0;
            }

            var a = m_EntityManager.GetComponentData<EntityGuid>(x);
            var b = m_EntityManager.GetComponentData<EntityGuid>(y);

            return a.CompareTo(b);
        }
    }

    internal readonly struct SceneContainer
    {
        internal readonly EntityManager EntityManager;
        internal readonly Scene Scene;
        internal readonly List<Object> AssetReferences;

        static SceneContainer()
        {
            PropertyBagResolver.Register(new ScenePropertyBag());
        }

        public SceneContainer(EntityManager entityManager, Scene scene)
        {
            EntityManager = entityManager;
            Scene = scene;
            AssetReferences = null;

            using (var entities = scene.ToEntityArray(entityManager, Allocator.TempJob))
            {
                entities.Sort(new EntityGuidComparer(entityManager));

                var assetReferenceVisitor = new AssetReferenceVisitor(entityManager);

                foreach (var entity in entities)
                {
                    PropertyContainer.Visit(new EntityContainer(entityManager, entity), assetReferenceVisitor);
                }

                AssetReferences = assetReferenceVisitor.AssetReferences.Distinct().ToList();
            }
        }

        public int EntityCount() => Scene.EntityCount(EntityManager);
    }

    internal class ScenePropertyBag : PropertyBag<SceneContainer>
    {
        private readonly Property<SceneContainer, string> m_FileTypeProperty = new Property<SceneContainer, string>("FileType",
                                                                                                  (ref SceneContainer container) => "Scene");

        private readonly Property<SceneContainer, Guid> m_GuidProperty = new Property<SceneContainer, Guid>("Guid",
                                                                                                  (ref SceneContainer container) => container.Scene.SceneGuid.Guid);

        private readonly Property<SceneContainer, int> m_SerializedVersionProperty = new Property<SceneContainer, int>("SerializedVersion",
                                                                                                     (ref SceneContainer container) => 1);


        private readonly ListProperty<SceneContainer, Object> m_AssetReferences = new ListProperty<SceneContainer, Object>("AssetReferences",
                                                                                                     (ref SceneContainer container) => container.AssetReferences,
                                                                                                     null);

        private struct EntityContainerProperty : ICollectionElementProperty<SceneContainer, EntityContainer>
        {
            private readonly EntityContainer m_Container;

            public string GetName() => $"[{Index}]";

            public bool IsReadOnly => true;
            public bool IsContainer => true;
            public int Index { get; }
            public IPropertyAttributeCollection Attributes => null;

            public EntityContainerProperty(EntityContainer container, int index)
            {
                m_Container = container;
                Index = index;
            }

            public EntityContainer GetValue(ref SceneContainer container) => m_Container;
            public void SetValue(ref SceneContainer container, EntityContainer value) => throw new Exception("Property is ReadOnly");
        }

        private struct EntitiesProperty : ICollectionProperty<SceneContainer, IEnumerable<EntityContainer>>
        {
            private readonly NativeArray<Entity> m_Entities;

            public string GetName() => "Entities";

            public bool IsReadOnly => true;
            public bool IsContainer => false;
            public IPropertyAttributeCollection Attributes => null;

            public EntitiesProperty(NativeArray<Entity> entities)
            {
                m_Entities = entities;
            }

            public IEnumerable<EntityContainer> GetValue(ref SceneContainer container) => null;
            public void SetValue(ref SceneContainer container, IEnumerable<EntityContainer> value) => throw new Exception("Property is ReadOnly");
            public int GetCount(ref SceneContainer container) => container.EntityCount();
            public void SetCount(ref SceneContainer container, int count) => throw new Exception("Property is ReadOnly");
            public void Clear(ref SceneContainer container) => throw new Exception("Property is ReadOnly");

            public void GetPropertyAtIndex<TGetter>(ref SceneContainer container, int index, ref ChangeTracker changeTracker, TGetter getter)
                where TGetter : ICollectionElementGetter<SceneContainer>
            {
                getter.VisitProperty<EntityContainerProperty, EntityContainer>(new EntityContainerProperty(new EntityContainer(container.EntityManager, m_Entities[index]), index), ref container);
            }
        }

        public override void Accept<TVisitor>(ref SceneContainer container, TVisitor visitor, ref ChangeTracker changeTracker)
        {
            visitor.VisitProperty<Property<SceneContainer, string>, SceneContainer, string>(m_FileTypeProperty, ref container, ref changeTracker);
            visitor.VisitProperty<Property<SceneContainer, Guid>, SceneContainer, Guid>(m_GuidProperty, ref container, ref changeTracker);
            visitor.VisitProperty<Property<SceneContainer, int>, SceneContainer, int>(m_SerializedVersionProperty, ref container, ref changeTracker);
            visitor.VisitCollectionProperty<ListProperty<SceneContainer, Object>, SceneContainer, IList<Object>>(m_AssetReferences, ref container, ref changeTracker);

            using (var entities = container.Scene.ToEntityArray(container.EntityManager, Allocator.TempJob))
            {
                entities.Sort(new EntityGuidComparer(container.EntityManager));

                visitor.VisitCollectionProperty<EntitiesProperty, SceneContainer, IEnumerable<EntityContainer>>(new EntitiesProperty(entities), ref container, ref changeTracker);
            }
        }

        public override bool FindProperty<TAction>(string name, ref SceneContainer container, ref ChangeTracker changeTracker, ref TAction action)
        {
            throw new NotImplementedException();
        }
    }
}
