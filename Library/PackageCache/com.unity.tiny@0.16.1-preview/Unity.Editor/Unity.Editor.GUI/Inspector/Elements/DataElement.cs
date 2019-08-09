using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Authoring;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Editor.Persistence;
using Unity.Entities;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.Editor
{
    internal abstract class DataElement<TData> : VisualElement, IBinding, IBindable, IComponentDataElement<TData>, IOffsetDataProvider
        where TData : struct
    {
        protected TData m_Data;
        private EntityInspectorElement m_Root;
        private IPropertyAttributeCollection m_Attributes;
        public Entity MainTarget { get; }
        public NativeArray<Entity> Targets { get; }
        protected ChangePropagator<TData> ChangePropagator { get; }
        protected IInspector<TData> Inspector { get; }

        private EntityInspectorElement Root
        {
            get
            {
                if (null == m_Root)
                {
                    m_Root = GetFirstAncestorOfType<EntityInspectorElement>();
                }

                return m_Root;
            }
        }

        protected EntityManager EntityManager { get; }
        private List<IDataUpdater> Updaters { get; }
        private BindingVisitor<TData> m_Visitor;

        protected abstract bool HasData(Entity entity);
        protected abstract TData GetData(Entity entity);
        protected abstract void SetData(Entity entity, TData data);
        protected abstract void RemoveComponent(Entity entity);
        protected bool IsTypeIncluded => DomainCache.IsIncludedInProject(Application.AuthoringProject, Assembly);
        protected abstract Assembly Assembly { get; }
        protected abstract bool IsTagComponent { get; }
        public abstract Type ComponentType { get; }

        public DataElement(Session session, NativeArray<Entity> targets, IInspector<TData> inspector)
        {
            MainTarget = targets[0];
            Targets = targets;
            Session = session;
            EntityManager = session.GetManager<IWorldManager>().EntityManager;
            ChangePropagator = new ChangePropagator<TData>(Targets, GetData, SetData);
            if (!IsTagComponent)
            {
                m_Data = GetData(MainTarget);
            }

            Inspector = inspector;
            name = typeof(TData).Name;
            Updaters = new List<IDataUpdater>();

            AddToClassList("unity-ecs-component");
        }

        public Session Session { get; }

        public TData Data
        {
            get => GetData(MainTarget);
            set
            {
                if (IsTagComponent)
                {
                    return;
                }
                if (ChangePropagator.Apply(m_Data, value))
                {
                    m_Data = value;
                }
            }
        }

        public TAttribute GetAttribute<TAttribute>() where TAttribute : Attribute
        {
            return m_Attributes.GetAttribute<TAttribute>();
        }

        public virtual void BuildFromVisitor<TProperty, TContainer, TValue>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref TValue value, InspectorContext context)
            where TProperty : IProperty<TContainer, TValue>
        {
            m_Attributes = property.Attributes;
            context.GetParent(out var currentParent);
            currentParent.contentContainer.Add(this);

            context.PushParent(this);
            VisualElement subRoot;
            VisualElement menu;

            var removeButton = new Button(ShowContextMenu);
            removeButton.AddToClassList(UssClassNames.ComponentMenu);

            var element = Inspector?.Build(MakeDataProxy());
            if (!IsTagComponent && (null != element || null == Inspector))
            {
                subRoot = GuiFactory.Foldout(property, ref container, ref value, context, property.GetName());
                AddToClassList("unity-ecs-component--with-field");
                subRoot.AddToClassList("unity-ecs-component--toggle");
                menu = subRoot.Q<Toggle>();
                menu.Add(removeButton);
            }
            else
            {
                subRoot = menu = GuiFactory.Label(property, ref container, ref value, context);
                AddToClassList("unity-ecs-component--no-field");
                menu.contentContainer.Add(removeButton);
            }

            menu.RegisterCallback<MouseUpEvent>(evt =>
            {
                // Right-click
                if (evt.button == 1)
                {
                    ShowContextMenu();
                }
            });

            subRoot.Q<Label>().AddToClassList("unity-ecs-component--label");

            if (IsTypeIncluded)
            {
                if (element == null)
                {
                    if (null == Inspector)
                    {
                        PropertyContainer.Visit(ref value, visitor);
                    }
                }
                else
                {
                    subRoot.contentContainer.Add(element);
                    if (Inspector is IInspectorTemplateProvider provider && provider.AutoRegisterBindings)
                    {
                        RegisterBindings();
                    }
                }
            }
            else
            {
                var assembly = Assembly;
                var assemblyName = assembly.GetName().Name;

                var iconMessageContainer = new VisualElement();
                iconMessageContainer.AddToClassList("unity-ecs-component-type-icon-message__container");
                var image = new Image
                {
                    image = EditorGUIUtility.IconContent("d_console.erroricon.sml").image,
                    scaleMode = ScaleMode.ScaleToFit
                };
                iconMessageContainer.Add(image);

                var iconMessage = new Label($"The {assemblyName} module is not included in your open project.");
                iconMessage.AddToClassList("unity-ecs-component-type-icon-message__message");
                iconMessageContainer.Add(iconMessage);

                subRoot.contentContainer.Add(iconMessageContainer);

                var file = Application.AuthoringProject.GetAssemblyDefinitionFile();
                var relativePath = AssetDatabaseUtility.GetPathRelativeToProjectPath(file.FullName);
                var projectAssembly = AssetDatabase.LoadAssetAtPath(relativePath, typeof(Object));
                if (null != projectAssembly)
                {
                    var buttonContainer = new VisualElement();
                    buttonContainer.AddToClassList("unity-ecs-component-type-missing-button__container");
                    var buttonAdd = new Button(() =>
                    {
                        var asmdef = AssemblyDefinition.Deserialize(file);
                        List<string> references = new List<string>(asmdef.references);
                        references.Add(assemblyName);
                        asmdef.references = references.ToArray();
                        asmdef.Serialize(file);
                        AssetDatabase.ImportAsset(relativePath);
                        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    })
                    {
                        text = "Add module and recompile"
                    };

                    buttonAdd.AddToClassList("unity-ecs-component-type-missing__button");
                    buttonContainer.Add(buttonAdd);

                    var button = new Button(() => { Selection.activeInstanceID = projectAssembly.GetInstanceID(); })
                    {
                        text = "Go to project's assembly"
                    };
                    button.AddToClassList("unity-ecs-component-type-missing__button");
                    buttonContainer.Add(button);

                    subRoot.contentContainer.Add(buttonContainer);
                }

                AddToClassList("unity-background");
            }

            context.PopParent(subRoot);
            context.PopParent(this);
            CacheUpdaters();
            Update();
        }

        public virtual unsafe T GetDataAtOffset<T>(int offset) where T : struct
        {
            var component = Data;
            UnsafeUtility.CopyPtrToStructure((byte*) UnsafeUtility.AddressOf(ref component) + offset, out T data);
            return data;
        }

        public void SetDataAtOffset<T>(T data, int index, int offset) where T : struct
        {
            if (this is IBufferData buffer)
            {
                var size = UnsafeUtility.SizeOf(buffer.ElementType);
                SetDataAtOffset(data, index * size + offset);
            }
        }

        public T GetDataAtOffset<T>(int index, int offset) where T : struct
        {
            if (this is IBufferData buffer)
            {
                var size = UnsafeUtility.SizeOf(buffer.ElementType);
                return GetDataAtOffset<T>(index * size + offset);
            }
            throw new NotImplementedException();
        }

        public virtual unsafe void SetDataAtOffset<T>(T data, int offset) where T : struct
        {
            var current = Data;
            var apply = (byte*) UnsafeUtility.AddressOf(ref current) + offset;
            UnsafeUtility.CopyPtrToStructure(apply, out T baseData);
            if (ChangePropagator.SetDataAtOffset(baseData, data, offset))
            {
                m_Data = GetData(MainTarget);
                Update();
            }
        }

        private InspectorDataProxy<TData> MakeDataProxy()
         => new InspectorDataProxy<TData>(this, Targets);

        void IBinding.PreUpdate() {}

        void IBinding.Update()
        {
            if (IsTagComponent || !HasData(MainTarget))
            {
                return;
            }

            // TODO: Get change from WorldDiff instead of polling here.
            var value = GetData(MainTarget);

            if (ChangePropagator.IsDifferent(m_Data, value))
            {
                m_Data = value;
                Update();
            }
        }

        private void Update()
        {
            for (var i = 0; i < Updaters.Count; ++i)
            {
                Updaters[i].Update();
            }
            Inspector?.Update(MakeDataProxy());
        }

        void IBinding.Release() {}

        private void CacheUpdaters()
        {
            CacheUpdaters(this);
        }

        IBinding IBindable.binding
        {
            get => this;
            set{}
        }

        string IBindable.bindingPath
        {
            get => name;
            set{}
        }

        private void CacheUpdaters(VisualElement element)
        {
            if (element is IDataUpdater updater)
            {
                Updaters.Add(updater);
            }

            for (var i = 0; i < element.childCount; ++i)
            {
                var children = element[i];
                CacheUpdaters(children);
            }
        }

        public void RegisterBindings()
        {
            if (null == m_Visitor)
            {
                m_Visitor = new BindingVisitor<TData>(this);
            }

            m_Visitor.ResetOffsets();
            m_Visitor.Entity = MainTarget;
            PropertyContainer.Visit(m_Data, m_Visitor);
        }

        public void RegisterUpdater(IDataUpdater updater)
        {
            Updaters.Add(updater);
        }

        private void ShowContextMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove"), false, () =>
            {
                foreach (var target in Targets)
                {
                    RemoveComponent(target);
                }
            });

            menu.AddSeparator(string.Empty);

            menu.AddItem(new GUIContent("Reset Initial Values.."), false, () =>
            {
                SetData( MainTarget, DomainCache.GetDefaultValue<TData>());
            });
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Move Up"), false, () =>
            {
                Root.MoveUp(this);
            });
            
            menu.AddItem(new GUIContent("Move Down"), false, () =>
            {
                Root.MoveDown(this);
            });
            
            menu.ShowAsContext();
        }
    }
}
