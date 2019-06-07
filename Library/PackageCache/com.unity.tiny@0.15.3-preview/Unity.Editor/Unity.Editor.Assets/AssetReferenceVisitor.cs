using System.Collections.Generic;
using Unity.Authoring.Core;
using Unity.Editor.Extensions;
using Unity.Entities;
using Unity.Properties;
using UnityEngine;

namespace Unity.Editor.Assets
{
    internal class AssetReferenceVisitor : PropertyVisitor
    {
        private class Adapter : IPropertyVisitorAdapter,
            IVisitAdapter<Entity>
        {
            private readonly EntityManager m_EntityManager;
            private readonly List<Object> m_AssetReferences;

            public Adapter(EntityManager entityManager, List<Object> assetReferences)
            {
                m_EntityManager = entityManager;
                m_AssetReferences = assetReferences;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref Entity value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, Entity>
            {
                if (!m_EntityManager.Exists(value))
                {
                    return VisitStatus.Override;
                }

                if (!m_EntityManager.HasComponent<AssetReference>(value))
                {
                    return VisitStatus.Override;
                }

                var reference = m_EntityManager.GetComponentData<AssetReference>(value);
                var obj = reference.ToUnityObject();

                if (!obj || null == obj)
                {
                    return VisitStatus.Override;
                }

                m_AssetReferences.Add(obj);
                return VisitStatus.Override;
            }
        }

        public List<Object> AssetReferences { get; set; } = new List<Object>();

        public AssetReferenceVisitor(EntityManager entityManager)
        {
            AddAdapter(new Adapter(entityManager, AssetReferences));
        }
    }
}
