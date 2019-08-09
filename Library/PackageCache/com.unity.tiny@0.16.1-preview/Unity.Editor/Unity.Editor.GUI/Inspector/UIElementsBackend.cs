using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Authoring;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.UIElements;
using Unity.Properties;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEngine;

namespace Unity.Editor
{
    internal sealed class UIElementsBackend : InspectorBackend<Entity>
    {
        private class CommonComponentsVisitor : PropertyVisitor
        {
            private readonly NativeList<Entity> m_Targets;
            private readonly NativeList<ComponentType> m_OutTypes;
            private readonly EntityManager m_EntityManager;

            public CommonComponentsVisitor(NativeList<Entity> mTargets, EntityManager mEntityManager, NativeList<ComponentType> mOutTypes)
            {
                m_Targets = mTargets;
                m_EntityManager = mEntityManager;
                m_OutTypes = mOutTypes;
            }
            
            protected override VisitStatus BeginContainer<TProperty, TContainer, TValue>(TProperty property, ref TContainer container,
                ref TValue value, ref ChangeTracker changeTracker)
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
                for (var i = 0; i < m_Targets.Length; ++i)
                {
                    if (!m_EntityManager.HasComponent(m_Targets[i], componentType))
                    {
                        return VisitStatus.Override;
                    }
                }
                m_OutTypes.Add(componentType);
                return VisitStatus.Override;
            }
        }
        
        private VisualElement RootElement { get; }
        private EntityInspectorElement InspectorElement { get; set; }
        private NativeList<Entity> ActualTargets;

        public UIElementsBackend(Session session, VisualElement root)
            : base(session)
        {
            RootElement = root;
            ActualTargets = new NativeList<Entity>(Allocator.Persistent);
        }

        private void ReSeatTargets(IEnumerable<Entity> entities)
        {
            ActualTargets.Clear();
            foreach (var entity in entities.Where(EntityManager.Exists))
            {
                ActualTargets.Add(entity);
            }
        }

        public override void Build()
        {
            Reset();

            var targets = new List<Entity>();
            targets.AddRange(Targets.Where(e => e != Entity.Null && EntityManager.Exists(e)));

            ReSeatTargets(targets);

            if (targets.Count == 0)
            {
                return;
            }

            InspectorElement = new EntityInspectorElement(Session, ActualTargets);
            RootElement.contentContainer.Add(InspectorElement);

            var addComponentButton = new Button();
            addComponentButton.AddToClassList(UssClassNames.AddComponentButton);
            addComponentButton.text = "Add Component";
            addComponentButton.RegisterCallback<MouseUpEvent>(evt =>
            {
                using (var list = new NativeList<ComponentType>(10, Allocator.TempJob))
                using (var map = new NativeHashMap<int, int>(10, Allocator.TempJob))
                {
                    var container = new EntityContainer(EntityManager, ActualTargets[0], true);
                    PropertyContainer.Visit(ref container, new CommonComponentsVisitor(ActualTargets, EntityManager, list));

                    for (var i = 0; i < list.Length; ++i)
                    {
                        map.TryAdd(list[i].TypeIndex, 0);
                    }
                    
                    var databases = new[]
                    {
                        ComponentSearcherDatabases.DynamicPopulateComponents(map),
                        ComponentSearcherDatabases.DynamicPopulateSharedComponents(map),
                        ComponentSearcherDatabases.DynamicPopulateBufferComponents(map),
                    };

                    var searcher = new Searcher(
                        databases,
                        ComponentSearcherDatabases.SearcherAdapter);


                    var editorWindow = EditorWindow.focusedWindow;
                    var button = evt.target as Button;

                    SearcherWindow.Show(
                        editorWindow,
                        searcher,
                        AddType,
                        button.worldBound.center + Vector2.up * (button.worldBound.height - 10.0f) / 2.0f,
                        a => { },
                        new SearcherWindow.Alignment(SearcherWindow.Alignment.Vertical.Top,
                            SearcherWindow.Alignment.Horizontal.Center)
                    );
                }
            });

            RootElement.contentContainer.Add(addComponentButton);
        }

        

        public override void OnDestroyed()
        {
            if (ActualTargets.IsCreated)
            {
                ActualTargets.Dispose();
            }
            InspectorElement?.Dispose();
        }

        public override void Reset()
        {
            RootElement.Clear();
            InspectorElement?.Dispose();
        }

        private bool AddType(SearcherItem item)
        {
            if (!(item is TypeSearcherItem typedItem))
            {
                return false;
            }

            item.Database.ItemList.Remove(item);

            foreach (var entity in Targets)
            {
                var type = typedItem.Type;
                var index = TypeManager.GetTypeIndex(type);
                if (!EntityManager.HasComponent(entity, type))
                {
                    EntityManager.AddComponent(entity, type);
                    if (TypeManager.IsZeroSized(index))
                    {
                        continue;
                    }

                    if (typeof(IComponentData).IsAssignableFrom(type))
                        unsafe
                        {
                            var data = EntityManager.GetComponentDataRawRW(entity, index);
                            var defaultValue = DomainCache.GetDefaultValue(type);
                            // TODO: Find a better way
                            typeof(UIElementsBackend)
                                .GetMethod(nameof(Copy), BindingFlags.NonPublic | BindingFlags.Instance)?
                                .MakeGenericMethod(type)
                                .Invoke(this, new [] {entity, index, defaultValue});
                        }
                }
            }
            EntityInspector.ForceRebuildAll();
            return true;
        }

        private unsafe void Copy<T>(Entity entity, int index, T data) where T : struct
        {
            var destination = EntityManager.GetComponentDataRawRW(entity, index);
            Unsafe.Copy(destination, ref data);
        }
    }
}
