using System.Linq;
using System.Reflection;
using Unity.Authoring;
using Unity.Authoring.ChangeTracking;
using Unity.Authoring.Core;
using Unity.Authoring.Hashing;
using Unity.Editor.Undo;
using Unity.Entities;

namespace Unity.Editor
{
    internal class MandatoryEditorComponentsManager : ISessionManagerInternal
    {
        private IChangeManager m_ChangeManager;
        private IEditorUndoManager m_Undo;
        private IWorldManager m_WorldManager;
        private EntityManager EntityManager => m_WorldManager.EntityManager;
        private bool m_IsUndoing;

        private readonly ulong ComponentOrderHash =
            TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(typeof(ComponentOrder))).StableTypeHash;

        public void Load(Session session)
        {
            m_WorldManager = session.GetManager<IWorldManager>();
            m_ChangeManager = session.GetManager<IChangeManager>();
            m_ChangeManager.RegisterChangeCallback(HandleChanges, int.MinValue);

            m_Undo = session.GetManager<IEditorUndoManager>();
            m_Undo.UndoRedoBatchStarted += HandleUndoRedoStarted;
            m_Undo.UndoRedoBatchEnded += HandleUndoRedoEnded;
        }

        public void Unload(Session session)
        {
            m_ChangeManager.UnregisterChangeCallback(HandleChanges);
            m_Undo.UndoRedoBatchStarted -= HandleUndoRedoStarted;
            m_Undo.UndoRedoBatchEnded -= HandleUndoRedoEnded;
        }

        private void HandleChanges(Changes changes)
        {
            if (m_IsUndoing)
            {
                return;
            }

            foreach (var guid in changes.CreatedEntities())
            {
                var entity = m_WorldManager.GetEntityFromGuid(guid);
                PopulateInitialComponentList(entity);
            }

            foreach (var diff in changes.WorldDiff.AddComponents)
            {
                var hash = changes.WorldDiff.TypeHashes[diff.TypeHashIndex];
                var typeIndex = TypeManager.GetTypeIndexFromStableTypeHash(hash);
                var type = TypeManager.GetType(typeIndex);
                if (hash == ComponentOrderHash || type.GetCustomAttributes(typeof(HideInInspectorAttribute)).Any())
                {
                    continue;
                }
                var guid = changes.WorldDiff.Entities[diff.EntityIndex].ToGuid();
                var entity = m_WorldManager.GetEntityFromGuid(guid);
                AddTypeToBuffer(entity, hash);
            }
            
            foreach (var diff in changes.WorldDiff.RemoveComponents)
            {
                var hash = changes.WorldDiff.TypeHashes[diff.TypeHashIndex];
                var typeIndex = TypeManager.GetTypeIndexFromStableTypeHash(hash);
                var type = TypeManager.GetType(typeIndex);
                if (hash == ComponentOrderHash || type.GetCustomAttributes(typeof(HideInInspectorAttribute)).Any())
                {
                    continue;
                }
                var guid = changes.WorldDiff.Entities[diff.EntityIndex].ToGuid();
                var entity = m_WorldManager.GetEntityFromGuid(guid);
                RemoveTypeFromBuffer(entity, hash);
            }
        }

        private void PopulateInitialComponentList(Entity entity)
        {
            if (EntityManager.HasComponent<ComponentOrder>(entity))
            {
                return;
            }

            var buffer = EntityManager.AddBuffer<ComponentOrder>(entity);
            foreach (var componentType in EntityManager.GetChunk(entity).Archetype.GetComponentTypes())
            {
                var typeInfo = TypeManager.GetTypeInfo(componentType.TypeIndex);
                if (typeInfo.Type.GetCustomAttributes(typeof(HideInInspectorAttribute)).Any())
                {
                    continue;
                }
                buffer.Add(new ComponentOrder{ StableTypeHash = typeInfo.StableTypeHash });
            }
        }

        private void AddTypeToBuffer(Entity entity, ulong typeHash)
        {
            if (!EntityManager.HasComponent<ComponentOrder>(entity))
            {
                PopulateInitialComponentList(entity);
                return;
            }
            
            var buffer = EntityManager.AddBuffer<ComponentOrder>(entity);
            for (var i = 0; i < buffer.Length; ++i)
            {
                if (buffer[i].StableTypeHash == typeHash)
                {
                    return;
                }
            }
            buffer.Add(new ComponentOrder{ StableTypeHash = typeHash });
        }
        
        private void RemoveTypeFromBuffer(Entity entity, ulong typeHash)
        {
            if (!EntityManager.HasComponent<ComponentOrder>(entity))
            {
                return;
            }
            
            var buffer = EntityManager.AddBuffer<ComponentOrder>(entity);
            var index = 0;
            for (; index < buffer.Length; ++index)
            {
                if (buffer[index].StableTypeHash == typeHash)
                {
                    break;
                }
            }

            if (index < buffer.Length)
            {
                buffer.RemoveAt(index);
            }
        }
        
        private void HandleUndoRedoStarted()
        {
            m_IsUndoing = true;
        }
        
        private void HandleUndoRedoEnded()
        {
            m_IsUndoing = false;
        }
    }
}