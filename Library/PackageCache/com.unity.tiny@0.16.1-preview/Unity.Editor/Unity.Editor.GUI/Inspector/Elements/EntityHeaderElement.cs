using Unity.Authoring;
using Unity.Collections;
using Unity.Editor.Hierarchy;
using Unity.Entities;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal class EntityHeaderElement : VisualElement, IBinding, IBindable
    {
        private const string k_EnabledToggle = "EntityEnabledFlag";
        private const string k_EntityNameLabel = "EntityName";

        private Entity MainTarget { get; }
        private NativeArray<Entity> Targets { get; }
        private IWorldManager WorldManager { get; }
        private EntityManager EntityManager => WorldManager.EntityManager;
        private TextField m_NameField;
        private Toggle m_EnabledField;

        public EntityHeaderElement(Session session, NativeArray<Entity> targets)
        {
            MainTarget = targets[0];
            Targets = targets;
            WorldManager = session.GetManager<IWorldManager>();
        }

        public void BuildFromInspector<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property,
            ref TContainer container, ref Entity value, InspectorContext context)
            where TProperty : IProperty<TContainer, Entity>
        {
            var configEntity = WorldManager.GetConfigEntity();
            if (value == configEntity)
            {
                return;
            }

            context.GetParent(out var parent);
            var headerRoot = this;
            parent.contentContainer.Add(headerRoot);

            var headerTemplate = StyleSheets.EntityHeader;
            headerTemplate.Template.CloneTree(headerRoot);
            headerRoot.AddStyleSheetSkinVariant(headerTemplate.StyleSheet);

            m_EnabledField = headerRoot.Q<Toggle>(k_EnabledToggle);
            m_EnabledField.value = !EntityManager.HasComponent<Disabled>(value);
            m_EnabledField.RegisterValueChangedCallback(evt =>
            {
                var enabled = evt.newValue;
                for (var i = 0; i < Targets.Length; ++i)
                {
                    var entity = Targets[i];
                    if (EntityManager.HasComponent<Disabled>(entity))
                    {
                        if (enabled)
                        {
                            EntityManager.RemoveComponent<Disabled>(entity);
                        }
                    }
                    else
                    {
                        if (!enabled)
                        {
                            EntityManager.AddComponentData<Disabled>(entity, default);
                        }
                    }
                }
            });

            m_NameField = headerRoot.Q<TextField>(k_EntityNameLabel);
            name = WorldManager.GetEntityName(value);
            m_NameField.value = name;
            m_NameField.RegisterValueChangedCallback(evt =>
            {
                if (string.IsNullOrEmpty(evt.newValue))
                {
                    m_NameField.SetValueWithoutNotify(evt.previousValue);
                    return;
                }
                for (var i = 0; i < Targets.Length; ++i)
                {
                    WorldManager.SetEntityName(Targets[i], evt.newValue);
                }
            });
        }

        void IBinding.PreUpdate() { }

        public void Update()
        {
            if (!EntityManager.Exists(MainTarget))
            {
                return;
            }

            var currentName = WorldManager.GetEntityName(MainTarget);
            if (currentName != m_NameField.value)
            {
                name = currentName;
                m_NameField.SetValueWithoutNotify(currentName);
            }

            var enabled = !EntityManager.HasComponent<Disabled>(MainTarget);
            if (enabled != m_EnabledField.value)
            {
                m_EnabledField.SetValueWithoutNotify(enabled);
            }
        }

        void IBinding.Release() { }

        IBinding IBindable.binding
        {
            get => this;
            set { }
        }

        public string bindingPath { get; set; }
    }
}
