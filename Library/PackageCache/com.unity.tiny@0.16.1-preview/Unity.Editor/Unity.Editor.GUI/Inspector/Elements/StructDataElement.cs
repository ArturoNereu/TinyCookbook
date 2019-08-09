using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal class StructDataElement<TComponentData, T> : VisualElement, IBindable, IStructDataElement<T>, IDataUpdater
        where T : struct
        where TComponentData : struct
    {
        private IInspector<T> Inspector { get; }
        protected IComponentDataElement<TComponentData> Root { get; }
        private readonly int m_BaseOffset;
        private readonly int m_ElementSize;
        public int Index;
        public int Offset => math.max(m_ElementSize * Index, 0) + m_BaseOffset;

        private string Name { get; }

        private T m_Data;
        private IPropertyAttributeCollection m_Attributes;

        public Session Session { get; }

        public virtual T Data
        {
            get => m_Data;
            set
            {
                Root.SetDataAtOffset(value, Offset);
                m_Data = value;
            }
        }

        public Entity MainTarget => Root.MainTarget;
        public NativeArray<Entity> Targets => Root.Targets;

        public TAttribute GetAttribute<TAttribute>() where TAttribute : Attribute
        {
            return m_Attributes.GetAttribute<TAttribute>();
        }

        public StructDataElement(IInspector<T> inspector, IComponentDataElement<TComponentData> root, int index, int offset, int elementSize, string name)
        {
            Root = root;
            Session = Root.Session;
            Name = name;
            Index = index;
            m_BaseOffset = offset;
            m_ElementSize = elementSize;
            Inspector = inspector;
            this.name = typeof(T).Name;
            AddToClassList("unity-ecs-struct");
            UpdateWithoutPropagation();
        }

        public void Update()
        {
            UpdateWithoutPropagation();
            UpdateBindings();
            Inspector?.Update(MakeDataProxy());
        }

        public IBinding binding { get; set; }
        string IBindable.bindingPath { get; set; }

        public static bool IsZeroSizeStruct<TValue>()
        {
            return IsZeroSizeStruct(typeof(TValue));
        }

        private static bool IsZeroSizeStruct(Type t)
        {
            return t.IsValueType && !t.IsPrimitive &&
                   t.GetFields(BindingFlags.Public | BindingFlags.Instance).All(fi => IsZeroSizeStruct(fi.FieldType));
        }

        public virtual void BuildFromVisitor<TProperty, TContainer, TValue>(IPropertyVisitor visitor, TProperty property,
            ref TContainer container, ref TValue value, InspectorContext context)
            where TProperty : IProperty<TContainer, TValue>
        {
            m_Attributes = property.Attributes;

            context.GetParent(out var currentParent);
            currentParent.contentContainer.Add(this);

            context.PushParent(this);
            var element = Inspector?.Build(MakeDataProxy());

            if (element == null)
            {
                VisualElement subRoot;
                if (null == Inspector)
                {
                    if (!IsZeroSizeStruct<TValue>())
                    {
                        subRoot = GuiFactory.Foldout(property, ref container, ref value, context, property.GetName());
                        subRoot.AddToClassList("unity-ecs-struct-with-field");
                    }
                    else
                    {
                        subRoot = GuiFactory.Label(property, ref container, ref value, context);
                        subRoot.AddToClassList("unity-ecs-struct-no-field");
                    }

                    PropertyContainer.Visit(ref value, visitor);
                }
                else
                {
                    subRoot = GuiFactory.Label(property, ref container, ref value, context);
                    subRoot.AddToClassList("unity-ecs-struct-no-field");
                }

                subRoot.Q<Label>().AddToClassList("unity-ecs-struct-label");
                context.PopParent(subRoot);
            }
            else
            {
                contentContainer.Add(element);
                if (Inspector is IInspectorTemplateProvider provider && provider.AutoRegisterBindings)
                {
                    RegisterBindings();
                }
            }

            context.PopParent(this);
        }

        public void UpdateWithoutPropagation()
        {
            m_Data = Root.GetDataAtOffset<T>(Offset);
        }

        private InspectorDataProxy<T> MakeDataProxy()
        {
            return new InspectorDataProxy<T>(this, Root.Targets, Name);
        }

        public void RegisterBindings()
        {
            UpdateBindings(true);
        }

        private void UpdateBindings(bool registerCallbacks = false)
        {
//            if (null == m_Visitor)
//            {
//                m_Visitor = new BindingVisitor(this, Registry);
//            }
//
//            m_Visitor.RegisterCallbacks = registerCallbacks;
//            m_Visitor.Entity = Entity;
//            var obj = Entity.GetComponentAsTinyObject<TComponentData>();
//
//            obj.Visit(m_Visitor);
        }
    }
}
