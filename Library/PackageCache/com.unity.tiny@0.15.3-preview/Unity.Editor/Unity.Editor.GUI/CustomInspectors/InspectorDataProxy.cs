using System;
using Unity.Authoring;
using Unity.Collections;
using Unity.Entities;

namespace Unity.Editor
{
    /// <summary>
    /// Acts as a proxy to get and set data for an <see cref="IComponentData"/> that is being inspected.
    /// </summary>
    /// <typeparam name="T">The <see cref="IComponentData"/> type to inspect.</typeparam>
    public struct InspectorDataProxy<T>
    {
        public Session Session
        {
            get => m_Parent.Session;
        }

        public Entity MainTarget { get; }
        public NativeArray<Entity> Targets { get; }

        public T Data
        {
            get => m_Parent.Data;
            set => m_Parent.Data = value;
        }

        public TAttribute GetAttribute<TAttribute>() where TAttribute : Attribute
        {
            return m_Parent.GetAttribute<TAttribute>();
        }

        public string Name { get; }

        /// <summary>
        /// Constructs a new <see cref="InspectorDataProxy{TComponentData}"/> instance from an Inspector.
        /// </summary>
        /// <param name="inspector">The Inspector.</param>
        public InspectorDataProxy(IDataProvider<T> inspector, NativeArray<Entity> targets, string name = "")
        {
            m_Parent = inspector;
            Name = name;
            Targets = targets;
            MainTarget = Targets[0];
        }

        private readonly IDataProvider<T> m_Parent;
    }
}
