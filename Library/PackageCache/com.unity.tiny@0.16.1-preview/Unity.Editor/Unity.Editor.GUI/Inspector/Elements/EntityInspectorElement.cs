using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Authoring.Core;
using Unity.Authoring.Hashing;
using Unity.Collections;
using Unity.Entities;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal class EntityInspectorElement : VisualElement, IDisposable
    {
        private readonly Session m_Session;
        private readonly NativeList<Entity> m_Targets;
        private EntityHeaderElement m_EntityHeader;
        private List<IComponentDataElement> m_Components;
        private readonly IWorldManagerInternal m_WorldManager;
        private readonly IChangeManager m_ChangeManager;

        public EntityManager EntityManager => m_WorldManager.EntityManager;

        public EntityInspectorElement(Session session, NativeList<Entity> targets)
        {
            m_Session = session;
            m_Targets = targets;
            m_Components = new List<IComponentDataElement>();
            m_WorldManager = m_Session.GetManager<IWorldManagerInternal>();
            m_ChangeManager = m_Session.GetManager<IChangeManager>();

            var entityManager = m_Session.GetManager<IWorldManager>().EntityManager;
            
            var firstTarget = m_Targets[0];
            var container = new EntityContainer(entityManager, firstTarget);
            
            var visitor = new InspectorVisitor<Entity>(m_Session, m_Targets);
            visitor.AddAdapter(new EntityContainerAdapter(visitor));
            visitor.AddAdapter(new PrimitivesAdapter<Entity>(visitor));

            visitor.Context.PushParent(this);

            StronglyTypedVisit(visitor, ref container);
            
            m_EntityHeader = this.Q<EntityHeaderElement>();
            m_Components = Children().OfType<IComponentDataElement>().ToList();
            OrderComponentsFromComponentOrder();
        }

        internal void MoveUp(IComponentDataElement element)
        {
            var index = m_Components.IndexOf(element);
            if (index < 0 || index == 0)
            {
                return;
            }
            var firstHash = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(element.ComponentType)).StableTypeHash;
            var secondHash = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(m_Components[index-1].ComponentType)).StableTypeHash;
            PropagateToComponentOrder(firstHash, secondHash);
        }

        internal void MoveDown(IComponentDataElement element)
        {
            var index = m_Components.IndexOf(element);
            if (index < 0 || index == m_Components.Count - 1)
            {
                return;
            }
            
            var firstHash = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(element.ComponentType)).StableTypeHash;
            var secondHash = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(m_Components[index+1].ComponentType)).StableTypeHash;
            PropagateToComponentOrder(firstHash, secondHash);
        }
        
        private void OrderComponentsFromComponentOrder()
        {
            var entity = m_Targets[0];
            if (!EntityManager.HasComponent<ComponentOrder>(entity))
            {
                return;
            }

            var order = EntityManager.GetBuffer<ComponentOrder>(entity);
            m_Components = m_Components.OrderBy(d => OrderFrom(d, order)).ToList();
            
            Clear();
            contentContainer.Add(m_EntityHeader);
            foreach (var element in m_Components)
            {
                contentContainer.Add(element as VisualElement);
            }
        }
        
        private void PropagateToComponentOrder(ulong lhs, ulong rhs)
        {
            for(var i = 0; i < m_Targets.Length; ++i)
            {
                var entity = m_Targets[i];
                if (!EntityManager.HasComponent<ComponentOrder>(entity))
                {
                    continue;
                }

                var firstIndex = -1;
                var secondIndex = -1;
                
                var order = EntityManager.GetBuffer<ComponentOrder>(entity);
                for (var index = 0; index < order.Length; ++index)
                {
                    if (order[index].StableTypeHash == lhs)
                    {
                        firstIndex = index;
                    }
                    if (order[index].StableTypeHash == rhs)
                    {
                        secondIndex = index;
                    }
                }

                if (firstIndex >= 0 && secondIndex >= 0)
                {
                    var temp = order[firstIndex];
                    order[firstIndex] = order[secondIndex];
                    order[secondIndex] = temp;
                }
            }

            OrderComponentsFromComponentOrder();
        }

        private int OrderFrom(IComponentDataElement element, DynamicBuffer<ComponentOrder> order)
        {
            var hash = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(element.ComponentType)).StableTypeHash;
            for (var i = 0; i < order.Length; ++i)
            {
                if (order[i].StableTypeHash == hash)
                {
                    return i;
                }
            }
            return int.MaxValue;
        }

        private static void StronglyTypedVisit<TContainer>(InspectorVisitor<Entity> visitor, ref TContainer container)
        {
            PropertyContainer.Visit(new Wrapper<TContainer>(container), visitor);
        }
        

        public void Dispose()
        {
            // TODO: Here because we'll want to handle changes in a more granular fashion than rebuilding the inspector.
        }
       
    }
}