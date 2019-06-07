#if !NET_DOTS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Authoring.ChangeTracking;
using Unity.Collections;
using Unity.Entities;

namespace Unity.Authoring.Undo
{
    internal static class WorldDiffExtensions
    {
        public static uint Size(this WorldDiff worldDiff)
        {
            return worldDiff.HasChanges ? (uint)Marshal.SizeOf(worldDiff) : 0;
        }

        public static WorldDiff Clone(this WorldDiff worldDiff, Allocator label)
        {
            var clone = new WorldDiff
            {
                TypeHashes = new NativeArray<ulong>(worldDiff.TypeHashes, label),
                Entities = new NativeArray<EntityGuid>(worldDiff.Entities, label),
                EntityNames = new NativeArray<NativeString64>(worldDiff.EntityNames, label),
                ComponentPayload = new NativeArray<byte>(worldDiff.ComponentPayload, label),
                NewEntityCount = worldDiff.NewEntityCount,
                DeletedEntityCount = worldDiff.DeletedEntityCount,
                AddComponents = new NativeArray<ComponentDiff>(worldDiff.AddComponents, label),
                RemoveComponents = new NativeArray<ComponentDiff>(worldDiff.RemoveComponents, label),
                SetCommands = new NativeArray<DataDiff>(worldDiff.SetCommands, label),
                EntityPatches = new NativeArray<DiffEntityPatch>(worldDiff.EntityPatches, label),
                LinkedEntityGroupAdditions = new NativeArray<LinkedEntityGroupAddition>(worldDiff.LinkedEntityGroupAdditions, label),
                LinkedEntityGroupRemovals = new NativeArray<LinkedEntityGroupRemoval>(worldDiff.LinkedEntityGroupRemovals, label)
            };

            if (null != worldDiff.SharedSetCommands)
            {
                clone.SharedSetCommands = new SetSharedComponentDiff[worldDiff.SharedSetCommands.Length];
                Array.Copy(worldDiff.SharedSetCommands, clone.SharedSetCommands, worldDiff.SharedSetCommands.Length);
            }

            return clone;
        }
    }

    internal interface IUndoManager : ISessionManager
    {
        uint MaxSizeInBytes { get; set; }
        uint UndoCount { get; }
        uint RedoCount { get; }
        void Undo();
        void Redo();
        void Flush();

        void BeginRecording();
        void EndRecording();

        event Action ChangeRecorded;
    }

    internal class UndoManager : SessionManager, IUndoManager
    {
        /// <summary>
        /// A single atomic undo/redo-able operation.
        /// </summary>
        private readonly struct Operation : IDisposable
        {
            private readonly WorldDiff[] m_WorldDiffs;

            public Operation(WorldDiff[] worldDiffs)
            {
                m_WorldDiffs = worldDiffs;
            }

            public long ComputeSize() => m_WorldDiffs.Sum(x => x.Size());

            public void Apply(World world)
            {
                foreach (var diff in m_WorldDiffs)
                {
                    WorldDiffer.ApplyDiff(world, diff);
                }
            }

            public void Dispose()
            {
                foreach (var diff in m_WorldDiffs)
                {
                    diff.Dispose();
                }
            }
        }

        private readonly LinkedList<Operation> m_UndoStack = new LinkedList<Operation>();
        private readonly LinkedList<Operation> m_RedoStack = new LinkedList<Operation>();
        private readonly List<WorldDiff> m_Buffer = new List<WorldDiff>();
        private bool m_Recording;

        private IWorldManager m_WorldManager;
        private IChangeManager m_ChangeManager;

        public uint MaxSizeInBytes { get; set; } = 64 * 1024 * 1024; // 64 MB
        public uint UndoCount => (uint) m_UndoStack.Count;
        public uint RedoCount => (uint) m_RedoStack.Count;

        public event Action ChangeRecorded = delegate { };

        public UndoManager(Session session) : base(session)
        {
        }

        public override void Load()
        {
            m_WorldManager = Session.GetManager<IWorldManager>();
            m_ChangeManager = Session.GetManager<IChangeManager>();

            if (null == m_WorldManager)
            {
                throw new ArgumentNullException(nameof(m_WorldManager));
            }

            m_ChangeManager.RegisterChangeCallback(HandleChanges);
        }

        public override void Unload()
        {
            m_ChangeManager.UnregisterChangeCallback(HandleChanges);
            Flush();
            m_WorldManager = null;
        }

        private void HandleChanges(Changes changes)
        {
            if (!m_Recording)
            {
                return;
            }

            m_Buffer.Add(changes.InverseDiff.Clone(Allocator.Persistent));
        }

        public void Flush()
        {
            ClearOperations(m_UndoStack);
            ClearOperations(m_RedoStack);
        }

        public void BeginRecording()
        {
            m_Recording = true;
            m_Buffer.Clear();
        }

        public void EndRecording()
        {
            m_Recording = false;

            if (m_Buffer.Count <= 0)
            {
                return;
            }

            // Track this operation in the undo stack.
            m_Buffer.Reverse();
            PushOperation(new Operation(m_Buffer.ToArray()), m_UndoStack);

            // This should clear our redo stack since we are `branching`
            ClearOperations(m_RedoStack);

            // Notify listeners of any changes.
            // NOTE: This is plugged in to the main `UnityEngine` undo system to get the actual keyboard hooks.
            ChangeRecorded();
        }

        private static void ClearOperations(LinkedList<Operation> stack)
        {
            foreach (var diff in stack)
            {
                diff.Dispose();
            }

            stack.Clear();
        }

        /// <summary>
        /// Pushes the given operation to the given stack.
        /// </summary>
        /// <param name="operation">The new operation to record.</param>
        /// <param name="stack">The stack to push to (i.e. Undo or Redo)</param>
        private void PushOperation(Operation operation, LinkedList<Operation> stack)
        {
            EnsureMaximumByteSize(operation, MaxSizeInBytes);
            stack.AddLast(operation);
        }

        public void Undo()
        {
            ApplyOperation(m_UndoStack, m_RedoStack);
        }

        public void Redo()
        {
            ApplyOperation(m_RedoStack, m_UndoStack);
        }

        private void ApplyOperation(LinkedList<Operation> fromStack, LinkedList<Operation> toStack)
        {
            if (fromStack.Count == 0)
            {
                return;
            }

            var operation = fromStack.Last();
            operation.Apply(m_WorldManager.World);
            operation.Dispose();
            fromStack.RemoveLast();

            // Start recording changes
            m_Buffer.Clear();
            m_Recording = true;

            // Tick the change tracker and update the shadow world.
            m_ChangeManager.Update();

            // Cleanup state
            m_Recording = false;

            // If we do not actually change anything in the world during the apply, something is wrong.
            if (m_Buffer.Count <= 0)
            {
                // throw new InvalidOperationException("An operation was performed but no changes were detected.");
            }
            else
            {
                // Track this operation in the undo stack.
                PushOperation(new Operation(m_Buffer.ToArray()), toStack);
            }
        }

        /// <summary>
        /// Ensures that the undo stack never exceeds the given maximum number of bytes.
        /// </summary>
        /// <param name="operation">The operation that will be added to the stack.</param>
        /// <param name="maxSize">The maximum number of bytes the undo stack can hold.</param>
        private void EnsureMaximumByteSize(Operation operation, uint maxSize)
        {
            var size = operation.ComputeSize();

            while (m_UndoStack.Count > 0)
            {
                var undo = (uint) m_UndoStack.Sum(x => x.ComputeSize());
                var total = size + undo;

                if (total < maxSize)
                {
                    return;
                }

                m_UndoStack.First().Dispose();
                m_UndoStack.RemoveFirst();
            }
        }
    }
}

#endif
