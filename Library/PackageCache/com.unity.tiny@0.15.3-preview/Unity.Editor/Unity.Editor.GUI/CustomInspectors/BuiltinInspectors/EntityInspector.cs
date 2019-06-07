using System;
using System.Linq;
using Unity.Authoring;
using Unity.Editor.Bindings;
using Unity.Entities;
using Unity.Tiny;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.Editor
{
    internal class EntityInspector : IStructInspector<Entity>
    {
        private ObjectField m_ObjectField;
        private bool m_SimpleMode;
        private IAssetManager m_AssetManager;
        private IUnityComponentCacheManager m_ComponentCache;
        private IWorldManager m_WorldManager;
        private Type ObjectType => m_ObjectField.objectType;


        public VisualElement Build(InspectorDataProxy<Entity> proxy)
        {
            m_ObjectField = new ObjectField {label = proxy.Name};
            m_ObjectField.RegisterValueChangedCallback(evt => ValueChanged(proxy, evt));
            m_WorldManager = proxy.Session.GetManager<IWorldManager>();
            m_AssetManager = proxy.Session.GetManager<IAssetManager>();
            m_ComponentCache = proxy.Session.GetManager<IUnityComponentCacheManager>();

            // TODO: Find correct mapping
            var withComponents = proxy.GetAttribute<EntityWithComponentsAttribute>();
            if (null != withComponents)
            {
                m_ObjectField.objectType = DomainCache.GetAssetType(withComponents.Types.FirstOrDefault());
                m_SimpleMode = false;
            }

            // Revert back to very generic
            if (null == m_ObjectField.objectType)
            {
                m_ObjectField.objectType = typeof(EntityReference);
                m_SimpleMode = true;
            }
            return m_ObjectField;
        }

        private void ValueChanged(InspectorDataProxy<Entity> proxy, ChangeEvent<Object> evt)
        {
            if (m_SimpleMode)
            {
                if (evt.newValue && null != evt.newValue)
                {
                    EntityReference entityRef = null;
                    switch (evt.newValue)
                    {
                        case EntityReference reference:
                            entityRef = reference;
                            break;
                        case Component component:
                            entityRef = component.GetComponent<EntityReference>();
                            break;
                        case GameObject gameObject:
                            entityRef = gameObject.GetComponent<EntityReference>();
                            break;
                    }

                    if (entityRef && null != entityRef)
                    {
                        proxy.Data = m_WorldManager.GetEntityFromGuid(entityRef.Guid);
                    }
                    else
                    {
                        proxy.Data = Entity.Null;
                    }
                }
                else
                {
                    proxy.Data = Entity.Null;
                }
            }
            else
            {
                if (evt.newValue && null != evt.newValue)
                {
                    proxy.Data = m_AssetManager.GetEntity(evt.newValue);
                }
                else
                {
                    proxy.Data = Entity.Null;
                }
            }
        }

        public void Update(InspectorDataProxy<Entity> proxy)
        {
            m_ObjectField.SetValueWithoutNotify(m_SimpleMode
                ? m_ComponentCache.GetEntityReference(proxy.Data)
                : m_AssetManager.GetUnityObject(proxy.Data, ObjectType));
        }
    }
}
