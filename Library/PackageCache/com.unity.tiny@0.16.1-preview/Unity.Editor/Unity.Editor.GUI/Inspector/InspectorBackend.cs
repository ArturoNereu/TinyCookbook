using System.Collections.Generic;
using Unity.Authoring;
using Unity.Entities;
using UnityEditor;

namespace Unity.Editor
{
    internal abstract class InspectorBackend<T> : IInspectorBackend<T>
    {
        protected readonly Session Session;
        protected readonly EntityManager EntityManager;
        private InspectorMode m_Mode = InspectorMode.Normal;

        public InspectorMode Mode
        {
            get => m_Mode;
            set
            {
                if (m_Mode == value)
                {
                    return;
                }
                m_Mode = value;
                Build();
            }
        }

        public List<T> Targets { get; }

        protected InspectorBackend(Session session)
        {
            Session = session;
            EntityManager = Session.GetManager<IWorldManager>().EntityManager;
            Targets = new List<T>();
        }

        public virtual void OnCreated() { }
        public virtual void Build() { }
        public virtual void OnDestroyed() { }
        public virtual void Reset() { }
    }
}
