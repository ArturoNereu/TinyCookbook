using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Authoring;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Properties;
using Unity.Tiny.Core2D;
using Unity.Tiny.Scenes;
using UnityEngine.Assertions;

namespace Unity.Editor
{
    internal class SceneGraph : SceneGraphBase
    {
        private readonly Session m_Session;
        private readonly Scene m_Scene;
        private IWorldManager m_WorldManager;
        private readonly EntityManager m_EntityManager;

        public Scene Scene => m_Scene;

        public SceneGraph(Session session, Scene scene)
        {
            m_Session = session;
            m_Scene = scene;
            m_WorldManager = m_Session.GetManager<IWorldManager>();
            m_EntityManager = m_WorldManager.EntityManager;
            Reload();
        }

        protected override ISceneGraphNode CreateNode(ISceneGraphNode source, ISceneGraphNode parent)
        {
            if (source is EntityNode sourceNode)
            {
                var targetEntity = m_EntityManager.CreateEntity(m_EntityManager.GetChunk(sourceNode.Entity).Archetype);
                var sourceEntity = sourceNode.Entity;
                var visitor = new CopyVisitor(m_EntityManager, sourceEntity, targetEntity);
                PropertyContainer.Visit(new EntityContainer(m_EntityManager, sourceEntity), visitor);
                m_WorldManager.SetEntityGuid(targetEntity, Guid.NewGuid());
                var entityNode = new EntityNode(this, m_Session, targetEntity);

                var newEntityName = EntityNameHelper.GetUniqueEntityName(m_WorldManager.GetEntityName(targetEntity), m_WorldManager, parent?.Children ?? Roots);
                m_WorldManager.SetEntityName(targetEntity, newEntityName);

                Add(entityNode, parent);
                return entityNode;
            }

            return null;
        }

        protected override void Remap(ISceneGraphNode source, ISceneGraphNode target)
        {
            // Extract all entities from the source and target trees
            var sourceEntities = source.GetDescendants().OfType<EntityNode>().Select(n => n.Entity).ToList();
            var targetEntities = target.GetDescendants().OfType<EntityNode>().Select(n => n.Entity).ToList();

            Assert.IsTrue(sourceEntities.Count == targetEntities.Count);

            // Build the remap information
            var entityReferenceRemap = new Dictionary<Entity, Entity>();

            for (var i = 0; i < sourceEntities.Count; i++)
            {
                entityReferenceRemap.Add(sourceEntities[i], targetEntities[i]);
            }

            var visitor = new RemapVisitor(entityReferenceRemap);

            // Remap each component of the target tree
            foreach (var entity in targetEntities)
            {
                PropertyContainer.Visit(new EntityContainer(m_EntityManager, entity), visitor);
            }
        }

        protected override void OnDeleteNode(ISceneGraphNode node)
        {
            switch (node)
            {
                case EntityNode entityNode:
                {
                    m_EntityManager.DestroyEntity(entityNode.Entity);
                }
                break;
            }
        }

        protected override void OnInsertNode(ISceneGraphNode node, ISceneGraphNode parent, int siblingIndex)
        {
            var parentEntity = new Parent { Value = (parent as EntityNode)?.Entity ?? Entity.Null };

            switch (node)
            {
                case EntityNode entityNode:
                {
                    var entity = entityNode.Entity;
                    if (m_EntityManager.HasComponent<Parent>(entity))
                    {
                        m_EntityManager.SetComponentData(entity, parentEntity);
                    }
                    else
                    {
                        m_EntityManager.AddComponentData(entity, parentEntity);
                    }

                    m_Scene.AddEntityReference(m_EntityManager, entity);
                    TransferScene(node.Children);
                }
                break;
            }

            if (null != parent)
            {
                ForceSiblingIndex(parent.Children, false);
            }
            else
            {
                ForceSiblingIndex(false);
            }
        }

        private void TransferScene(List<ISceneGraphNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node is EntityNode entityNode)
                {
                    m_Scene.AddEntityReference(m_EntityManager, entityNode.Entity);
                }

                TransferScene(node.Children);
            }
        }

        private class RemapVisitor : PropertyVisitor
        {
            public class EntityReferenceAdapter : IPropertyVisitorAdapter
                , IVisitContainerAdapter<Entity>
                , IVisitAdapter<EntityContainer, Entity>
            {
                private RemapVisitor Visitor;

                public EntityReferenceAdapter(RemapVisitor visitor)
                {
                    Visitor = visitor;
                }

                public VisitStatus BeginContainer<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property,
                    ref TContainer container, ref Entity value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, Entity>
                {
                    if (Visitor.m_Remap.TryGetValue(value, out var entity))
                    {
                        value = entity;
                    }
                    return VisitStatus.Unhandled;
                }

                public void EndContainer<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container,
                    ref Entity value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, Entity>
                {
                }

                public VisitStatus Visit<TProperty>(IPropertyVisitor visitor, TProperty property, ref EntityContainer container,
                    ref Entity value, ref ChangeTracker changeTracker) where TProperty : IProperty<EntityContainer, Entity>
                {
                    return VisitStatus.Unhandled;
                }
            }

            private readonly IDictionary<Entity, Entity> m_Remap;

            public RemapVisitor(Dictionary<Entity, Entity> remap)
            {
                m_Remap = remap;
                AddAdapter(new EntityReferenceAdapter(this));
            }
        }

        public bool IsRoot(ISceneGraphNode node)
        {
            return Roots.Contains(node);
        }

        private unsafe class CopyVisitor : PropertyVisitor
        {
            private readonly EntityManager m_EntityManager;
            private readonly Entity m_SourceEntity;
            private readonly Entity m_TargetEntity;

            public CopyVisitor(EntityManager manager, Entity sourceEntity, Entity targetEntity)
            {
                m_EntityManager = manager;
                m_SourceEntity = sourceEntity;
                m_TargetEntity = targetEntity;
            }

            protected override VisitStatus BeginContainer<TProperty, TContainer, TValue>(TProperty property, ref TContainer container,
                ref TValue value, ref ChangeTracker changeTracker)
            {
                if (typeof(IComponentData).IsAssignableFrom(typeof(TValue)))
                {
                    var index = TypeManager.GetTypeIndex(typeof(TValue));
                    if (!TypeManager.GetTypeInfo(index).IsZeroSized)
                    {
                        Unsafe.Copy(m_EntityManager.GetComponentDataRawRW(m_TargetEntity, index), ref value);
                    }
                }

                if (typeof(ISharedComponentData).IsAssignableFrom(typeof(TValue)))
                {
                    var index = TypeManager.GetTypeIndex(typeof(TValue));
                    m_EntityManager.SetSharedComponentDataBoxed(m_TargetEntity, index, value);
                }

                if (typeof(IDynamicBufferContainer).IsAssignableFrom(typeof(TValue)) &&
                    value is IDynamicBufferContainer buffer)
                {
                    var index = TypeManager.GetTypeIndex(buffer.ElementType);
                    var componentType = TypeManager.GetTypeInfo(index);

                    var srcBuffer =
                        (BufferHeader*) m_EntityManager.GetComponentDataRawRW(m_SourceEntity, componentType.TypeIndex);
                    var dstBuffer =
                        (BufferHeader*) m_EntityManager.GetComponentDataRawRW(m_TargetEntity, componentType.TypeIndex);

                    dstBuffer->Length = srcBuffer->Length;
                    BufferHeader.EnsureCapacity(dstBuffer, srcBuffer->Length, componentType.ElementSize, 4,
                        BufferHeader.TrashMode.RetainOldData);

                    // Copy all blittable data
                    UnsafeUtility.MemCpy(BufferHeader.GetElementPointer(dstBuffer),
                        BufferHeader.GetElementPointer(srcBuffer), componentType.ElementSize * srcBuffer->Length);
                }

                return VisitStatus.Override;
            }
        }

        public EntityNode FindNode(Entity entity)
        {
            var guid = m_WorldManager.GetEntityGuid(entity);
            if (guid == Guid.Empty)
            {
                return null;
            }

            return FindNode(guid);
        }

        public EntityNode FindNode(Guid guid)
        {
            return Roots.Select(r => FindEntityNodeRecursive(r, guid)).FirstOrDefault(r => null != r);
        }

        private static EntityNode FindEntityNodeRecursive(ISceneGraphNode node, Guid guid)
        {
            if (node is EntityNode entityNode && entityNode.Guid == guid)
            {
                return entityNode;
            }

            return node.Children
                .Select(r => FindEntityNodeRecursive(r, guid))
                .FirstOrDefault(r => null != r);
        }

        private void Reload()
        {
            Roots.Clear();

            var entities = m_Scene.ToEntityArray(m_EntityManager, Allocator.TempJob);

            try
            {
                using (var pooledDictionary = DictionaryPool<Entity, EntityNode>.GetDisposable())
                using (var pooledList = ListPool<Entity>.GetDisposable())
                {
                    var entityCache = pooledDictionary.Dictionary;
                    var allEntities = pooledList.List;
                    for (var i = 0; i < entities.Length; ++i)
                    {
                        var entity = entities[i];
                        var entityNode = new EntityNode(this, m_Session, entity);
                        allEntities.Add(entity);
                        entityCache.Add(entity, entityNode);
                    }

                    for (var i = 0; i < allEntities.Count; ++i)
                    {
                        var entity = allEntities[i];
                        var node = entityCache[entity];

                        EntityNode parent = null;

                        // Do we have a transform parent?
                        if (m_EntityManager.HasComponent<Parent>(entity))
                        {
                            var Parent = m_EntityManager.GetComponentData<Parent>(entity);
                            entityCache.TryGetValue(Parent.Value, out parent);
                        }

                        if (null != parent)
                        {
                            node.Parent = parent;
                            parent.Children.Add(node);
                        }
                        else
                        {
                            Roots.Add(node);
                        }
                    }
                }
            }
            finally
            {
                entities.Dispose();
            }

            SortBySiblingIndex(Roots);
            ForceSiblingIndex(Roots, true);
        }

        private void ForceSiblingIndex(bool recurse)
        {
            ForceSiblingIndex(Roots, recurse);
        }

        private void ForceSiblingIndex(List<ISceneGraphNode> nodes, bool recurse)
        {
            for (var i = 0; i < nodes.Count; ++i)
            {
                var node = nodes[i];
                if (node is EntityNode entityNode)
                {
                    entityNode.SetSiblingIndex(i);
                }

                if (recurse)
                {
                    ForceSiblingIndex(node.Children, recurse);
                }
            }
        }

        private void SortBySiblingIndex(List<ISceneGraphNode> nodes)
        {
            nodes.Sort(DoSortBySiblingIndex);
            for (var i = 0; i < nodes.Count; ++i)
            {
                var node = nodes[i];
                SortBySiblingIndex(node.Children);
            }
        }

        private int DoSortBySiblingIndex(ISceneGraphNode x, ISceneGraphNode y)
        {
            if (!(y is EntityNode rhs))
            {
                return -1;
            }

            if (!(x is EntityNode lhs))
            {
                return 1;
            }

            return lhs.Index.Index.CompareTo(rhs.Index.Index);
        }


    }
}
