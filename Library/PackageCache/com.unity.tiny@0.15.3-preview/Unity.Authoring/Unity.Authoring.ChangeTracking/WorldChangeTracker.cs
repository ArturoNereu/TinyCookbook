#if !NET_DOTS
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Assertions;
using Unity.Collections;

namespace Unity.Authoring.ChangeTracking
{
    internal class WorldChangeTracker : IDisposable
    {
        private const Allocator k_Allocator = Allocator.TempJob;

        /// <summary>
        /// Handle to an entity within a chunk along with it's guid.
        /// </summary>
        internal struct EntityInChunkWithGuid
        {
            public EntityInChunk Entity;
            public EntityGuid Guid;
        }

        /// <summary>
        /// Intermediate structure used to work with a modified entity change.
        /// </summary>
        internal struct ModifiedEntity
        {
            public EntityGuid Guid;
            public EntityInChunk Before;
            public EntityInChunk After;
        }

        /// <summary>
        /// Storage to represent a packed component within a packed world diff.
        /// </summary>
        public struct PackedComponent
        {
            /// <summary>
            /// Entity index in the packed entities array.
            /// </summary>
            public int PackedEntityIndex;

            /// <summary>
            /// Type index in the packed stableTypeHash array.
            /// </summary>
            public int PackedStableTypeHashIndex;
        }

        public struct ComponentDataChange
        {
            public PackedComponent Component;

            public int Offset;
            public int SizeBytes;
        }

        public struct EntityGuidPatch
        {
            public PackedComponent Component;

            public EntityGuid Guid;
            public int Offset;
        }

        internal struct SharedComponentDataChange
        {
            public PackedComponent Component;
            public int TypeIndex;
            public int BeforeSharedComponentIndex;
            public int AfterSharedComponentIndex;
        }

        /// <summary>
        /// Internal structure to pass params easier.
        ///
        /// This structure will hold all the relevant information for a world to diff.
        /// </summary>
        internal struct WorldState
        {
            /// <summary>
            /// A set of all entities in the world to consider for the diff.
            ///
            /// @NOTE This only includes entities that have `potentially` changed.
            /// </summary>
            public NativeArray<EntityInChunkWithGuid> Entities;

            /// <summary>
            /// Lookup of ALL <see cref="Entity"/> to <see cref="EntityGuid"/> in the world.
            /// We need this to resolve any references we may encounter.
            /// </summary>
            public NativeHashMap<Entity, EntityGuid> EntityGuidLookup;

            /// <summary>
            /// The shared component data manager for these entities.
            /// </summary>
            public ManagedComponentStore ManagedComponentStore;
        }

        internal struct ComponentDataStream : IDisposable
        {
            private NativeList<ComponentDataChange> m_ComponentData;
            private NativeList<SharedComponentDataChange> m_SharedComponentData;
            private NativeList<byte> m_Payload;

            public NativeList<ComponentDataChange> ComponentData => m_ComponentData;
            public NativeList<SharedComponentDataChange> SharedComponentData => m_SharedComponentData;
            public NativeList<byte> Payload => m_Payload;

            public ComponentDataStream(int initialComponentCapacity, int initialPayloadCapacity, Allocator label)
            {
                m_ComponentData = new NativeList<ComponentDataChange>(initialComponentCapacity, label);
                m_SharedComponentData = new NativeList<SharedComponentDataChange>(initialComponentCapacity, label);
                m_Payload = new NativeList<byte>(initialPayloadCapacity, label);
            }

            public void Dispose()
            {
                m_ComponentData.Dispose();
                m_SharedComponentData.Dispose();
                m_Payload.Dispose();
            }

            public unsafe void SetComponentData(PackedComponent component, void* ptr, int sizeOf)
            {
                m_ComponentData.Add(new ComponentDataChange
                {
                    Component = component,
                    Offset = 0,
                    SizeBytes = sizeOf
                });

                m_Payload.AddRange(ptr, sizeOf);
            }

            public void SetSharedComponentData(PackedComponent component, int typeIndex, int afterSharedComponentIndex, int beforeSharedComponentIndex = -1)
            {
                m_SharedComponentData.Add(new SharedComponentDataChange
                {
                    Component = component,
                    TypeIndex = typeIndex,
                    AfterSharedComponentIndex = afterSharedComponentIndex,
                    BeforeSharedComponentIndex = beforeSharedComponentIndex
                });
            }
        }
        
        [BurstCompile]
        private unsafe struct ClearDestroyedEntityReferences : IJobParallelFor
        {
            public uint GlobalSystemVersion;
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public TypeInfoStream TypeInfoStream;
            [ReadOnly, NativeDisableUnsafePtrRestriction] public EntityComponentStore* EntityComponentStore;
            
            public void Execute(int index)
            {
                var chunk = Chunks[index].m_Chunk;
                var archetype = chunk->Archetype;
                
                for (var typeIndexInArchetype = 0; typeIndexInArchetype < archetype->TypesCount; typeIndexInArchetype++)
                {
                    var componentTypeInArchetype = archetype->Types[typeIndexInArchetype];
                    var typeInfo = TypeInfoStream.GetTypeInfo(componentTypeInArchetype.TypeIndex);

                    if (typeInfo.EntityOffsetCount == 0)
                    {
                        continue;
                    }
                    
                    if (componentTypeInArchetype.IsSharedComponent || componentTypeInArchetype.IsZeroSized)
                    {
                        continue;
                    }
                    
                    var typeInChunkPtr = chunk->Buffer + archetype->Offsets[typeIndexInArchetype];
                    var typeSizeOf = archetype->SizeOfs[typeIndexInArchetype];

                    var changed = false;

                    for (var entityIndexInChunk = 0; entityIndexInChunk < Chunks[index].Count; entityIndexInChunk++)
                    {
                        var componentDataPtr = typeInChunkPtr + typeSizeOf * entityIndexInChunk;

                        if (componentTypeInArchetype.IsBuffer)
                        {
                            var bufferHeader = (BufferHeader*) componentDataPtr;
                            var bufferLength = bufferHeader->Length;
                            var bufferPtr = BufferHeader.GetElementPointer(bufferHeader);
                            changed |= SetDestroyedEntityReferencesToNull(typeInfo, bufferPtr, bufferLength);
                        }
                        else
                        {
                            changed |= SetDestroyedEntityReferencesToNull(typeInfo, componentDataPtr, 1);
                        }
                    }

                    if (changed)
                    {
                        chunk->SetChangeVersion(typeIndexInArchetype, GlobalSystemVersion);
                    }
                }
            }
            
            private bool SetDestroyedEntityReferencesToNull(
                TypeInfo typeInfo,
                byte* address,
                int elementCount)
            {
                var changed = false;
                
                for (var elementIndex = 0; elementIndex < elementCount; elementIndex++)
                {
                    var elementPtr = address + typeInfo.ElementSize * elementIndex;
                    
                    for (var entityOffsetIndex = 0; entityOffsetIndex < typeInfo.EntityOffsetCount; entityOffsetIndex++)
                    {
                        var offset = typeInfo.EntityOffsets[entityOffsetIndex];
                        if (EntityComponentStore->Exists(*(Entity*) (elementPtr + offset)))
                        {
                            continue;
                        }
                        *(Entity*) (elementPtr + offset) = Entity.Null;
                        changed = true;
                    }
                }

                return changed;
            }
        }

        [BurstCompile]
        private struct BuildChunkSequenceNumberMap : IJobParallelFor
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [WriteOnly] public NativeHashMap<ulong, ArchetypeChunk>.Concurrent ChunksBySequenceNumber;

            public unsafe void Execute(int index)
            {
                var chunk = Chunks[index];
                ChunksBySequenceNumber.TryAdd(chunk.m_Chunk->SequenceNumber, chunk);
            }
        }

        /// <summary>
        /// Given a set of all <see cref="SrcChunks"/> and <see cref="DstChunks"/> this will return a set of all chunks that should be considered
        /// created or destroyed.
        ///
        /// <see cref="CreatedChunks"/> is a subset of <see cref="SrcChunks"/> that should be duplicated in to the dst world.
        /// <see cref="DestroyedChunks"/> is a subset of <see cref="DstChunks"/> that should be removed from the dst world.
        ///
        /// Offsets are also generated to help parallelize subsequent jobs.
        ///
        /// @NOTE Any MODIFIED chunks will simple be treated as a both a destroy and create in their respective lists.
        ///
        /// </summary>
        [BurstCompile]
        private struct BuildChunkChanges : IJob
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> SrcChunks;
            [ReadOnly] public NativeArray<ArchetypeChunk> DstChunks;
            [ReadOnly] public NativeHashMap<ulong, ArchetypeChunk> SrcChunksBySequenceNumber;
            [ReadOnly] public NativeHashMap<ulong, ulong> DstToSrcChunkSequenceHash;

            public NativeList<ArchetypeChunk> CreatedChunks;
            public NativeList<ArchetypeChunk> DestroyedChunks;
            public NativeArray<int> CreatedChunkOffsets;
            public NativeArray<int> DestroyedChunkOffsets;
            public NativeHashMap<ulong, byte> VisitedChunks;

            [NativeDisableUnsafePtrRestriction] public unsafe int* CreateEntityCount;
            [NativeDisableUnsafePtrRestriction] public unsafe int* DestroyedEntityCount;

            public unsafe void Execute()
            {
                var createdChunksOffset = 0;
                var destroyedChunksOffset = 0;

                // Scan through the destination chunks.
                for (var i = 0; i < DstChunks.Length; i++)
                {
                    var dstChunk = DstChunks[i];
                    var srcChunk = default(ArchetypeChunk);

                    // Any look for a matching chunk in the destination world.
                    if (DstToSrcChunkSequenceHash.TryGetValue(dstChunk.m_Chunk->SequenceNumber, out var srcChunkSequenceNumber) &&
                        SrcChunksBySequenceNumber.TryGetValue(srcChunkSequenceNumber, out srcChunk))
                    {
                    }

                    if (srcChunk.m_Chunk == null)
                    {
                        // This chunk exists in the destination world but NOT in the source world.
                        // It should be destroyed.
                        DestroyedChunks.Add(dstChunk);

                        DestroyedChunkOffsets[DestroyedChunks.Length - 1] = destroyedChunksOffset;
                        destroyedChunksOffset += dstChunk.m_Chunk->Count;
                    }
                    else
                    {
                        if (ChunksAreDifferent(dstChunk.m_Chunk, srcChunk.m_Chunk))
                        {
                            // The chunk exists in both worlds, but it has been changed in some way.
                            // For now we will destroy and re-create it.
                            DestroyedChunks.Add(dstChunk);
                            CreatedChunks.Add(srcChunk);

                            DestroyedChunkOffsets[DestroyedChunks.Length - 1] = destroyedChunksOffset;
                            destroyedChunksOffset += dstChunk.m_Chunk->Count;

                            CreatedChunkOffsets[CreatedChunks.Length - 1] = createdChunksOffset;
                            createdChunksOffset += srcChunk.m_Chunk->Count;
                        }

                        VisitedChunks.TryAdd(srcChunk.m_Chunk->SequenceNumber, 1);
                    }
                }

                // Scan through the source chunks.
                for (var i = 0; i < SrcChunks.Length; i++)
                {
                    var srcChunk = SrcChunks[i];

                    // We only care about chunks we have not visited yet.
                    if (!VisitedChunks.TryGetValue(srcChunk.m_Chunk->SequenceNumber, out _))
                    {
                        // This chunk exists in the source world but NOT in the destination world.
                        // It should be created.
                        CreatedChunks.Add(srcChunk);

                        CreatedChunkOffsets[CreatedChunks.Length - 1] = createdChunksOffset;
                        createdChunksOffset += srcChunk.m_Chunk->Count;
                    }
                }

                *CreateEntityCount = createdChunksOffset;
                *DestroyedEntityCount = destroyedChunksOffset;
            }

            private static unsafe bool ChunksAreDifferent(Chunk* srcChunk, Chunk* dstChunk)
            {
                if (srcChunk->Count != dstChunk->Count)
                {
                    return true;
                }

                var typeCount = srcChunk->Archetype->TypesCount;

                for (var typeIndex = 0; typeIndex < typeCount; ++typeIndex)
                {
                    if (srcChunk->GetChangeVersion(typeIndex) != dstChunk->GetChangeVersion(typeIndex))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [BurstCompile]
        private struct BuildEntityInChunkWithGuid : IJobParallelFor
        {
            public int EntityGuidTypeIndex;
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public NativeArray<int> Offsets;
            [NativeDisableParallelForRestriction, WriteOnly]
            public NativeArray<EntityInChunkWithGuid> Entities;

            public unsafe void Execute(int index)
            {
                var chunk = Chunks[index].m_Chunk;
                var baseIndex = Offsets[index];

                var archetype = chunk->Archetype;
                var entityGuidIndexInArchetype = GetTypeIndexInArchetype(archetype, EntityGuidTypeIndex);
                var entityGuidBuffer = (EntityGuid*) (chunk->Buffer + archetype->Offsets[entityGuidIndexInArchetype]);

                for (var i = 0; i < chunk->Count; ++i)
                {
                    Entities[baseIndex + i] = new EntityInChunkWithGuid
                    {
                        Guid = entityGuidBuffer[i],
                        Entity = new EntityInChunk {Chunk = chunk, IndexInChunk = i}
                    };
                }
            }

            private static unsafe int GetTypeIndexInArchetype(Archetype* archetype, int typeIndex)
            {
                for (var i = 1; i < archetype->TypesCount; ++i)
                {
                    if (archetype->Types[i].TypeIndex == typeIndex)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        [BurstCompile]
        private struct BuildEntityGuidLookup : IJobParallelFor
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [WriteOnly] public NativeHashMap<Entity, EntityGuid>.Concurrent Lookup;

            public int EntityGuidTypeIndex;

            public unsafe void Execute(int index)
            {
                var chunk = Chunks[index].m_Chunk;
                var archetype = chunk->Archetype;
                var entities = (Entity*) (chunk->Buffer + archetype->Offsets[0]);

                var entityGuidTypeIndexInArchetype = 0;

                for (var i = 1; i < archetype->TypesCount; ++i)
                {
                    if (archetype->Types[i].TypeIndex != EntityGuidTypeIndex)
                    {
                        continue;
                    }

                    entityGuidTypeIndexInArchetype = i;
                    break;
                }

                var guids = (EntityGuid*) (chunk->Buffer + archetype->Offsets[entityGuidTypeIndexInArchetype]);

                for (var i = 0; i < chunk->Count; ++i)
                {
                    Lookup.TryAdd(entities[i], guids[i]);
                }
            }
        }

        [BurstCompile]
        private struct SortWithComparer<T, TComparer> : IJob
            where T : struct
            where TComparer : struct, IComparer<T>
        {
            public NativeArray<T> Array;

            public void Execute()
            {
                Array.Sort(new TComparer());
            }
        }

        private struct EntityInChunkWithGuidComparer : IComparer<EntityInChunkWithGuid>
        {
            public int Compare(EntityInChunkWithGuid x, EntityInChunkWithGuid y)
            {
                return x.Guid.CompareTo(y.Guid);
            }
        }

        private struct WorldDiffScope : IDisposable
        {
            private readonly WorldState m_BeforeState;
            private readonly WorldState m_AfterState;
            private readonly TypeInfoStream m_TypeInfoStream;

            private NativeList<EntityInChunkWithGuid> m_CreatedEntities;
            private NativeList<ModifiedEntity> m_ModifiedEntities;
            private NativeList<EntityInChunkWithGuid> m_DestroyedEntities;
            private PackedCollection<EntityGuid> m_PackedEntities;
            private PackedCollection<ulong> m_PackedStableTypeHashes;
            private NativeList<PackedComponent> m_AddComponents;
            private NativeList<PackedComponent> m_RemoveComponents;
            private ComponentDataStream m_ComponentDataStream;
            private NativeList<EntityGuidPatch> m_EntityGuidPatches;

            private JobHandle m_JobHandle;

            public WorldDiffScope(WorldState beforeState, WorldState afterState, TypeInfoStream typeInfoStream, Allocator allocator)
            {
                m_BeforeState = beforeState;
                m_AfterState = afterState;
                m_TypeInfoStream = typeInfoStream;
                m_CreatedEntities = new NativeList<EntityInChunkWithGuid>(1, allocator);
                m_ModifiedEntities = new NativeList<ModifiedEntity>(1, allocator);
                m_DestroyedEntities = new NativeList<EntityInChunkWithGuid>(1, allocator);
                m_PackedEntities = new PackedCollection<EntityGuid>(1, allocator);
                m_PackedStableTypeHashes = new PackedCollection<ulong>(1, allocator);
                m_AddComponents = new NativeList<PackedComponent>(1, allocator);
                m_RemoveComponents = new NativeList<PackedComponent>(1, allocator);
                m_ComponentDataStream = new ComponentDataStream(1, 1, allocator);
                m_EntityGuidPatches = new NativeList<EntityGuidPatch>(1, allocator);
                m_JobHandle = default;
            }

            public JobHandle ScheduleJobs()
            {
                var buildEntityChanges = new BuildEntityChanges
                {
                    BeforeEntities = m_BeforeState.Entities,
                    AfterEntities = m_AfterState.Entities,
                    CreatedEntities = m_CreatedEntities,
                    ModifiedEntities = m_ModifiedEntities,
                    DestroyedEntities = m_DestroyedEntities,
                }.Schedule();

                m_JobHandle = new BuildComponentDataChanges
                {
                    PackedEntityCollection = m_PackedEntities,
                    PackedStableTypeHashCollection = m_PackedStableTypeHashes,
                    CreatedEntities = m_CreatedEntities,
                    ModifiedEntities = m_ModifiedEntities,
                    AddComponents = m_AddComponents,
                    RemoveComponents = m_RemoveComponents,
                    ComponentDataStream = m_ComponentDataStream,
                    EntityGuidPatches = m_EntityGuidPatches,
                    TypeInfoStream = m_TypeInfoStream,
                    EntityGuidLookup = m_AfterState.EntityGuidLookup
                }.Schedule(buildEntityChanges);

                return m_JobHandle;
            }

            public WorldDiff GetWorldDiff(Allocator allocator)
            {
                m_JobHandle.Complete();

                var entities = m_PackedEntities.List;

                for (var i = 0; i < m_DestroyedEntities.Length; i++)
                {
                    entities.Add(m_DestroyedEntities[i].Guid);
                }

                // Allocate and copy in to the results buffers.
                var result = new WorldDiff
                {
                    NewEntityCount = m_CreatedEntities.Length,
                    DeletedEntityCount = m_DestroyedEntities.Length,
                    TypeHashes = m_PackedStableTypeHashes.List.ToArray(allocator),
                    Entities = m_PackedEntities.List.ToArray(allocator),
                    EntityNames = new NativeArray<NativeString64>(m_PackedEntities.Length, allocator),
                    ComponentPayload = m_ComponentDataStream.Payload.ToArray(allocator),
                    AddComponents = ToArray<ComponentDiff, PackedComponent>(m_AddComponents, allocator),
                    RemoveComponents = ToArray<ComponentDiff, PackedComponent>(m_RemoveComponents, allocator),
                    SetCommands = ToArray<DataDiff, ComponentDataChange>(m_ComponentDataStream.ComponentData, allocator),
                    EntityPatches = ToArray<DiffEntityPatch, EntityGuidPatch>(m_EntityGuidPatches, allocator),
                    SharedSetCommands = GetChangedSharedComponents(m_ComponentDataStream.SharedComponentData, m_BeforeState.ManagedComponentStore, m_AfterState.ManagedComponentStore),
                    LinkedEntityGroupAdditions = new NativeArray<LinkedEntityGroupAddition>(0, allocator),
                    LinkedEntityGroupRemovals = new NativeArray<LinkedEntityGroupRemoval>(0, allocator)
                };

                return result;
            }

            private static readonly SetSharedComponentDiff[] s_EmptySetSharedComponentDiff = new SetSharedComponentDiff[0];

            private static SetSharedComponentDiff[] GetChangedSharedComponents(
                NativeList<SharedComponentDataChange> changes,
                ManagedComponentStore beforeManagedComponentStore,
                ManagedComponentStore afterManagedComponentStore)
            {
                if (changes.Length == 0)
                {
                    return s_EmptySetSharedComponentDiff;
                }

                var result = new List<SetSharedComponentDiff>();

                for (var i = 0; i < changes.Length; i++)
                {
                    var change = changes[i];

                    var afterValue = afterManagedComponentStore.GetSharedComponentDataBoxed(change.AfterSharedComponentIndex, change.TypeIndex);

                    if (change.BeforeSharedComponentIndex > -1)
                    {
                        var beforeValue = beforeManagedComponentStore.GetSharedComponentDataBoxed(change.BeforeSharedComponentIndex, change.TypeIndex);

                        if (TypeManager.Equals(beforeValue, afterValue, change.TypeIndex))
                        {
                            continue;
                        }
                    }

                    result.Add(new SetSharedComponentDiff
                    {
                        EntityIndex = change.Component.PackedEntityIndex,
                        TypeHashIndex = change.Component.PackedStableTypeHashIndex,
                        BoxedSharedValue = afterValue
                    });
                }

                return result.ToArray();
            }

            public void Dispose()
            {
                m_CreatedEntities.Dispose();
                m_ModifiedEntities.Dispose();
                m_DestroyedEntities.Dispose();
                m_PackedEntities.Dispose();
                m_PackedStableTypeHashes.Dispose();
                m_AddComponents.Dispose();
                m_RemoveComponents.Dispose();
                m_ComponentDataStream.Dispose();
                m_EntityGuidPatches.Dispose();
            }
        }

        [BurstCompile]
        private struct BuildEntityChanges : IJob
        {
            [ReadOnly] public NativeArray<EntityInChunkWithGuid> BeforeEntities;
            [ReadOnly] public NativeArray<EntityInChunkWithGuid> AfterEntities;

            [WriteOnly] public NativeList<EntityInChunkWithGuid> CreatedEntities;
            [WriteOnly] public NativeList<ModifiedEntity> ModifiedEntities;
            [WriteOnly] public NativeList<EntityInChunkWithGuid> DestroyedEntities;

            public void Execute()
            {
                var afterEntityIndex = 0;
                var beforeEntityIndex = 0;

                while (beforeEntityIndex < BeforeEntities.Length && afterEntityIndex < AfterEntities.Length)
                {
                    var beforeEntity = BeforeEntities[beforeEntityIndex];
                    var afterEntity = AfterEntities[afterEntityIndex];

                    var compare = beforeEntity.Guid.CompareTo(afterEntity.Guid);

                    if (compare < 0)
                    {
                        DestroyedEntities.Add(beforeEntity);
                        beforeEntityIndex++;
                    }
                    else if (compare == 0)
                    {
                        ModifiedEntities.Add(new ModifiedEntity
                        {
                            Guid = beforeEntity.Guid,
                            After = afterEntity.Entity,
                            Before = beforeEntity.Entity
                        });

                        afterEntityIndex++;
                        beforeEntityIndex++;
                    }
                    else
                    {
                        CreatedEntities.Add(afterEntity);

                        afterEntityIndex++;
                    }
                }

                while (beforeEntityIndex < BeforeEntities.Length)
                {
                    DestroyedEntities.Add(BeforeEntities[beforeEntityIndex]);
                    beforeEntityIndex++;
                }

                while (afterEntityIndex < AfterEntities.Length)
                {
                    CreatedEntities.Add(AfterEntities[afterEntityIndex]);
                    afterEntityIndex++;
                }
            }
        }

        [BurstCompile]
        private struct BuildComponentDataChanges : IJob
        {
            public PackedCollection<EntityGuid> PackedEntityCollection;
            public PackedCollection<ulong> PackedStableTypeHashCollection;
            [ReadOnly] public TypeInfoStream TypeInfoStream;

            [ReadOnly] public NativeList<EntityInChunkWithGuid> CreatedEntities;
            [ReadOnly] public NativeList<ModifiedEntity> ModifiedEntities;

            [WriteOnly] public NativeList<PackedComponent> AddComponents;
            [WriteOnly] public NativeList<PackedComponent> RemoveComponents;
            [WriteOnly] public ComponentDataStream ComponentDataStream;
            [WriteOnly] public NativeList<EntityGuidPatch> EntityGuidPatches;

            [ReadOnly] public NativeHashMap<Entity, EntityGuid> EntityGuidLookup;

            public unsafe void Execute()
            {
                for (var i = 0; i < CreatedEntities.Length; ++i)
                {
                    var guid = CreatedEntities[i].Guid;
                    
                    // IMPORTANT
                    // We are doing a simple `Add` here and not a `GetOrAdd`.
                    // This means we can support multiple entities with the same guid.
                    // In such a case the returned world diff will be perfectly valid, BUT it will be almost impossible for consumers
                    // to resolve the correct entity which will likely cause problems downstream. In addition the `EntityGuidPatch` values could
                    // be invalid.
                    var packedEntityIndex = PackedEntityCollection.Add(guid);

                    var entity = CreatedEntities[i].Entity;
                    var chunk = entity.Chunk;
                    var archetype = chunk->Archetype;
                    var typesCount = archetype->TypesCount;

                    for (var indexInTypeArray = 1; indexInTypeArray < typesCount; indexInTypeArray++)
                    {
                        var typeInArchetype = archetype->Types[indexInTypeArray];

                        if (typeInArchetype.IsSystemStateComponent)
                        {
                            continue;
                        }

                        var typeIndex = typeInArchetype.TypeIndex;
                        var typeInfo = TypeInfoStream.GetTypeInfo(typeIndex);
                        var packedTypeIndex = PackedStableTypeHashCollection.GetOrAdd(typeInfo.StableTypeHash);

                        var packedComponent = new PackedComponent
                        {
                            PackedEntityIndex = packedEntityIndex,
                            PackedStableTypeHashIndex = packedTypeIndex
                        };

                        AddComponentData(
                            chunk,
                            archetype,
                            typeInArchetype,
                            indexInTypeArray,
                            entity.IndexInChunk,
                            packedComponent,
                            typeInfo
                        );
                    }
                }

                for (var i = 0; i < ModifiedEntities.Length; ++i)
                {
                    var guid = ModifiedEntities[i].Guid;
                    
                    // IMPORTANT
                    // We are doing a simple `Add` here and not a `GetOrAdd`.
                    // This means we can support multiple entities with the same guid.
                    // In such a case the returned world diff will be perfectly valid, BUT it will be almost impossible for consumers
                    // to resolve the correct entity which will likely cause problems downstream. In addition the `EntityGuidPatch` values could
                    // be invalid.
                    var packedEntityIndex = PackedEntityCollection.Add(guid);

                    var afterEntity = ModifiedEntities[i].After;
                    var afterChunk = afterEntity.Chunk;
                    var afterArchetype = afterChunk->Archetype;
                    var afterTypesCount = afterArchetype->TypesCount;

                    var beforeEntity = ModifiedEntities[i].Before;
                    var beforeChunk = beforeEntity.Chunk;
                    var beforeArchetype = beforeChunk->Archetype;

                    for (var afterIndexInTypeArray = 1; afterIndexInTypeArray < afterTypesCount; afterIndexInTypeArray++)
                    {
                        var afterTypeInArchetype = afterArchetype->Types[afterIndexInTypeArray];

                        if (afterTypeInArchetype.IsSystemStateComponent)
                        {
                            continue;
                        }

                        var afterTypeIndex = afterTypeInArchetype.TypeIndex;
                        var beforeIndexInTypeArray = ChunkDataUtility.GetIndexInTypeArray(beforeArchetype, afterTypeIndex);

                        if (-1 == beforeIndexInTypeArray)
                        {
                            var typeInfo = TypeInfoStream.GetTypeInfo(afterTypeInArchetype.TypeIndex);
                            var packedTypeIndex = PackedStableTypeHashCollection.GetOrAdd(typeInfo.StableTypeHash);

                            var packedComponent = new PackedComponent
                            {
                                PackedEntityIndex = packedEntityIndex,
                                PackedStableTypeHashIndex = packedTypeIndex
                            };

                            // This type does not exist on the before world. This was a newly added component.
                            AddComponentData(
                                afterChunk,
                                afterArchetype,
                                afterTypeInArchetype,
                                afterIndexInTypeArray,
                                afterEntity.IndexInChunk,
                                packedComponent,
                                typeInfo
                            );

                            continue;
                        }

                        if (afterTypeInArchetype.IsSharedComponent)
                        {
                            var typeInfo = TypeInfoStream.GetTypeInfo(afterTypeInArchetype.TypeIndex);
                            var packedTypeIndex = PackedStableTypeHashCollection.GetOrAdd(typeInfo.StableTypeHash);

                            var packedComponent = new PackedComponent
                            {
                                PackedEntityIndex = packedEntityIndex,
                                PackedStableTypeHashIndex = packedTypeIndex
                            };

                            var beforeOffset = beforeIndexInTypeArray - beforeChunk->Archetype->FirstSharedComponent;
                            var beforeSharedComponentIndex = beforeChunk->GetSharedComponentValue(beforeOffset);

                            var afterOffset = afterIndexInTypeArray - afterChunk->Archetype->FirstSharedComponent;
                            var afterSharedComponentIndex = afterChunk->GetSharedComponentValue(afterOffset);

                            // No managed objects in burst land. Do what we can a defer the actual unpacking until later.
                            ComponentDataStream.SetSharedComponentData(packedComponent, afterTypeIndex, afterSharedComponentIndex, beforeSharedComponentIndex);

                            continue;
                        }

                        // IMPORTANT This means `IsZeroSizedInChunk` which is always true for shared components.
                        // Always check shared components first.
                        if (afterTypeInArchetype.IsZeroSized)
                        {
                            continue;
                        }

                        if (afterTypeInArchetype.IsBuffer)
                        {
                            if (afterTypeIndex == TypeInfoStream.LinkedEntityGroupTypeIndex)
                            {
                                throw new Exception("LinkedEntityGroups are not supported yet.");
                            }

                            var typeInfo = TypeInfoStream.GetTypeInfo(afterTypeIndex);

                            var beforeBuffer = (BufferHeader*) (beforeChunk->Buffer + beforeArchetype->Offsets[beforeIndexInTypeArray] +
                                                                beforeEntity.IndexInChunk * beforeArchetype->SizeOfs[beforeIndexInTypeArray]);
                            var beforeElementPtr = BufferHeader.GetElementPointer(beforeBuffer);
                            var beforeLength = beforeBuffer->Length;

                            var afterBuffer = (BufferHeader*) (afterChunk->Buffer + afterArchetype->Offsets[afterIndexInTypeArray] +
                                                               afterEntity.IndexInChunk * afterArchetype->SizeOfs[afterIndexInTypeArray]);
                            var afterElementPtr = BufferHeader.GetElementPointer(afterBuffer);
                            var afterLength = afterBuffer->Length;

                            if (afterLength != beforeLength ||
                                UnsafeUtility.MemCmp(beforeElementPtr, afterElementPtr, afterLength * typeInfo.ElementSize) != 0)
                            {
                                var packedTypeIndex = PackedStableTypeHashCollection.GetOrAdd(typeInfo.StableTypeHash);

                                var packedComponent = new PackedComponent
                                {
                                    PackedEntityIndex = packedEntityIndex,
                                    PackedStableTypeHashIndex = packedTypeIndex
                                };

                                ComponentDataStream.SetComponentData(packedComponent, afterElementPtr, typeInfo.ElementSize * afterLength);
                                ExtractEntityGuidPatches(typeInfo, packedComponent, afterElementPtr, afterLength);
                            }
                        }
                        else
                        {
                            if (beforeArchetype->SizeOfs[beforeIndexInTypeArray] != afterArchetype->SizeOfs[afterIndexInTypeArray])
                            {
                                throw new Exception("Archetype->SizeOfs do not match");
                            }

                            var beforeAddress = beforeChunk->Buffer + beforeArchetype->Offsets[beforeIndexInTypeArray] + beforeArchetype->SizeOfs[beforeIndexInTypeArray] * beforeEntity.IndexInChunk;
                            var afterAddress = afterChunk->Buffer + afterArchetype->Offsets[afterIndexInTypeArray] +  afterArchetype->SizeOfs[afterIndexInTypeArray] * afterEntity.IndexInChunk;

                            if (UnsafeUtility.MemCmp(beforeAddress, afterAddress, beforeArchetype->SizeOfs[beforeIndexInTypeArray]) != 0)
                            {
                                var typeInfo = TypeInfoStream.GetTypeInfo(afterTypeInArchetype.TypeIndex);
                                var packedTypeIndex = PackedStableTypeHashCollection.GetOrAdd(typeInfo.StableTypeHash);

                                var packedComponent = new PackedComponent
                                {
                                    PackedEntityIndex = packedEntityIndex,
                                    PackedStableTypeHashIndex = packedTypeIndex
                                };

                                ComponentDataStream.SetComponentData(packedComponent, afterAddress, beforeArchetype->SizeOfs[beforeIndexInTypeArray]);
                                ExtractEntityGuidPatches(typeInfo, packedComponent, afterAddress, 1);
                            }
                        }
                    }

                    for (var beforeTypeIndexInArchetype = 1; beforeTypeIndexInArchetype < beforeArchetype->TypesCount; beforeTypeIndexInArchetype++)
                    {
                        var beforeTypeIndex = beforeArchetype->Types[beforeTypeIndexInArchetype].TypeIndex;

                        if (-1 == ChunkDataUtility.GetIndexInTypeArray(afterArchetype, beforeTypeIndex))
                        {
                            var typeInfo = TypeInfoStream.GetTypeInfo(beforeTypeIndex);
                            var packedTypeIndex = PackedStableTypeHashCollection.GetOrAdd(typeInfo.StableTypeHash);
                            RemoveComponents.Add(new PackedComponent {PackedEntityIndex = packedEntityIndex, PackedStableTypeHashIndex = packedTypeIndex});
                        }
                    }
                }
            }

            private unsafe void AddComponentData(
                Chunk* chunk,
                Archetype* archetype,
                ComponentTypeInArchetype typeInArchetype,
                int indexInTypeArray,
                int entityIndexInChunk,
                PackedComponent component,
                TypeInfo typeInfo
            )
            {
                AddComponents.Add(component);

                if (typeInArchetype.IsSharedComponent)
                {
                    var offset = indexInTypeArray - chunk->Archetype->FirstSharedComponent;
                    var sharedComponentIndex = chunk->GetSharedComponentValue(offset);

                    // No managed objects in burst land. Do what we can a defer the actual unpacking until later.
                    ComponentDataStream.SetSharedComponentData(component, typeInArchetype.TypeIndex, sharedComponentIndex);
                    return;
                }

                // IMPORTANT This means `IsZeroSizedInChunk` which is always true for shared components.
                // Always check shared components first.
                if (typeInArchetype.IsZeroSized)
                {
                    // Zero sized components have no data to copy.
                    return;
                }

                if (typeInArchetype.IsBuffer)
                {
                    if (typeInArchetype.TypeIndex == TypeInfoStream.LinkedEntityGroupTypeIndex)
                    {
                        throw new Exception("LinkedEntityGroups are not supported yet.");
                    }

                    var sizeOf = archetype->SizeOfs[indexInTypeArray];
                    var buffer = (BufferHeader*) (chunk->Buffer + archetype->Offsets[indexInTypeArray] + entityIndexInChunk * sizeOf);
                    var length = buffer->Length;

                    if (length == 0)
                    {
                        return;
                    }

                    var elementPtr = BufferHeader.GetElementPointer(buffer);
                    ComponentDataStream.SetComponentData(component, elementPtr, typeInfo.ElementSize * length);
                    ExtractEntityGuidPatches(typeInfo, component, elementPtr, length);
                }
                else
                {
                    var sizeOf = archetype->SizeOfs[indexInTypeArray];
                    var ptr = chunk->Buffer + archetype->Offsets[indexInTypeArray] + entityIndexInChunk * sizeOf;
                    ComponentDataStream.SetComponentData(component, ptr, sizeOf);
                    ExtractEntityGuidPatches(typeInfo, component, ptr, 1);
                }
            }

            private unsafe void ExtractEntityGuidPatches(
                TypeInfo typeInfo,
                PackedComponent component,
                byte* afterAddress,
                int elementCount)
            {
                if (typeInfo.EntityOffsetCount == 0)
                {
                    return;
                }

                var elementOffset = 0;

                for (var elementIndex = 0; elementIndex < elementCount; ++elementIndex)
                {
                    for (var offsetIndex = 0; offsetIndex < typeInfo.EntityOffsetCount; ++offsetIndex)
                    {
                        var offset = elementOffset + typeInfo.EntityOffsets[offsetIndex];

                        var entity = *(Entity*) (afterAddress + offset);

                        // If the entity has no guid, then guid will be null (desired)
                        if (!EntityGuidLookup.TryGetValue(entity, out var guid))
                        {
                            guid = EntityGuid.Null;
                        }

                        EntityGuidPatches.Add(new EntityGuidPatch
                        {
                            Component = component,
                            Offset = offset,
                            Guid = guid
                        });
                    }

                    elementOffset += typeInfo.ElementSize;
                }
            }
        }

        /// <summary>
        /// The source world we are reflecting.
        /// </summary>
        private readonly World m_SrcWorld;

        private readonly EntityQuery m_SrcWorldEntityGuidQuery;

        /// <summary>
        /// The shadow world which is a copy of the <see cref="m_SrcWorld"/>
        /// </summary>
        private readonly World m_DstWorld;

        private readonly EntityQuery m_DstWorldEntityGuidQuery;

        /// <summary>
        /// Mapping of chunk sequence numbers from the <see cref="m_SrcWorld"/> to the <see cref="m_DstWorld"/>
        /// </summary>
        private NativeHashMap<ulong, ulong> m_DstChunkToSrcChunkSequenceHashMap;

        /// <summary>
        /// Duplicated parts of the TypeManager so we can read TypeInfo in bursted jobs.
        /// </summary>
        private TypeInfoStream m_TypeInfoStream;

        public WorldChangeTracker([NotNull] World srcWorld, Allocator allocator)
        {
            m_SrcWorld = srcWorld;
            m_DstWorld = new World(srcWorld.Name + " (Shadow)");
            m_SrcWorldEntityGuidQuery = CreateEntityGuidQuery(srcWorld.EntityManager);
            m_DstWorldEntityGuidQuery = CreateEntityGuidQuery(m_DstWorld.EntityManager);

            m_DstChunkToSrcChunkSequenceHashMap = new NativeHashMap<ulong, ulong>(64, allocator);
            m_TypeInfoStream = new TypeInfoStream(allocator);

            foreach (var type in TypeManager.AllTypes)
            {
                m_TypeInfoStream.Add(type);
            }
        }

        private static EntityQuery CreateEntityGuidQuery(EntityManager entityManager)
        {
            return entityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {typeof(EntityGuid)},
                Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
            });
        }

        public void Dispose()
        {
            m_DstWorld.Dispose();
            m_DstChunkToSrcChunkSequenceHashMap.Dispose();
            m_TypeInfoStream.Dispose();
        }

        public unsafe bool TryGetChanges(out Changes changes)
        {
            var result = false;
            changes = default;

            using (var srcChunks = m_SrcWorldEntityGuidQuery.CreateArchetypeChunkArray(k_Allocator))
            using (var dstChunks = m_DstWorldEntityGuidQuery.CreateArchetypeChunkArray(k_Allocator))
            {
                new ClearDestroyedEntityReferences
                {
                    GlobalSystemVersion = m_SrcWorld.EntityManager.GlobalSystemVersion,
                    Chunks = srcChunks,
                    EntityComponentStore = m_SrcWorld.EntityManager.EntityComponentStore,
                    TypeInfoStream = m_TypeInfoStream
                }.Schedule(srcChunks.Length, srcChunks.Length / 4).Complete();
                
                using (var srcChunksBySequenceNumber = new NativeHashMap<ulong, ArchetypeChunk>(srcChunks.Length, k_Allocator))
                using (var createdChunks = new NativeList<ArchetypeChunk>(srcChunks.Length, k_Allocator))
                using (var destroyedChunks = new NativeList<ArchetypeChunk>(dstChunks.Length, k_Allocator))
                using (var visitedChunks = new NativeHashMap<ulong, byte>(srcChunks.Length, k_Allocator))
                using (var createdChunksOffsets = new NativeArray<int>(srcChunks.Length, k_Allocator))
                using (var destroyedChunksOffsets = new NativeArray<int>(dstChunks.Length, k_Allocator))
                {
                    var createdChunkEntityCount = 0;
                    var destroyedChunkEntityCount = 0;

                    new BuildChunkSequenceNumberMap
                    {
                        Chunks = srcChunks,
                        ChunksBySequenceNumber = srcChunksBySequenceNumber.ToConcurrent()
                    }.Schedule(srcChunks.Length, 64).Complete();

                    // Get a set of all changed chunks.
                    new BuildChunkChanges
                    {
                        SrcChunks = srcChunks,
                        DstChunks = dstChunks,
                        DstToSrcChunkSequenceHash = m_DstChunkToSrcChunkSequenceHashMap,
                        SrcChunksBySequenceNumber = srcChunksBySequenceNumber,
                        CreatedChunks = createdChunks,
                        DestroyedChunks = destroyedChunks,
                        CreatedChunkOffsets = createdChunksOffsets,
                        DestroyedChunkOffsets = destroyedChunksOffsets,
                        VisitedChunks = visitedChunks,
                        CreateEntityCount = &createdChunkEntityCount,
                        DestroyedEntityCount = &destroyedChunkEntityCount
                    }.Run();
                    
                    // If we have no changed or destroyed chunks then we have no work to do.
                    if (createdChunks.Length != 0 || destroyedChunks.Length != 0)
                    {
                        var srcEntityCount = m_SrcWorldEntityGuidQuery.CalculateLength();
                        var dstEntityCount = m_DstWorldEntityGuidQuery.CalculateLength();
                        var archetypeChanges = m_DstWorld.EntityManager.EntityComponentStore->BeginArchetypeChangeTracking();

                        using (var srcEntityGuidLookup = new NativeHashMap<Entity, EntityGuid>(srcEntityCount, k_Allocator))
                        using (var dstEntityGuidLookup = new NativeHashMap<Entity, EntityGuid>(dstEntityCount, k_Allocator))
                        {
                            using (var srcEntities = new NativeArray<EntityInChunkWithGuid>(createdChunkEntityCount, k_Allocator))
                            using (var dstEntities = new NativeArray<EntityInChunkWithGuid>(destroyedChunkEntityCount, k_Allocator))
                            {
                                // Now that we have a set of all chunks we need to operate on
                                // We need to extract out all guids and so we can do a deep comparison.
                                // This will give us an array of all `EntityInChunkWithGuid` for both worlds.
                                var buildSrcEntities = new BuildEntityInChunkWithGuid
                                {
                                    EntityGuidTypeIndex = TypeManager.GetTypeIndex<EntityGuid>(),
                                    Chunks = createdChunks,
                                    Offsets = createdChunksOffsets,
                                    Entities = srcEntities
                                }.Schedule(createdChunks.Length, Math.Max(createdChunks.Length / 4, 1));

                                var buildDstEntities = new BuildEntityInChunkWithGuid
                                {
                                    EntityGuidTypeIndex = TypeManager.GetTypeIndex<EntityGuid>(),
                                    Chunks = destroyedChunks,
                                    Offsets = destroyedChunksOffsets,
                                    Entities = dstEntities
                                }.Schedule(destroyedChunks.Length, Math.Max(destroyedChunks.Length / 4, 1));

                                // Sort the by guid so we can compare the two arrays.
                                var sortSrcEntities = new SortWithComparer<EntityInChunkWithGuid, EntityInChunkWithGuidComparer>
                                {
                                    Array = srcEntities
                                }.Schedule(buildSrcEntities);

                                var sortDstEntities = new SortWithComparer<EntityInChunkWithGuid, EntityInChunkWithGuidComparer>
                                {
                                    Array = dstEntities
                                }.Schedule(buildDstEntities);

                                var sortEntities = JobHandle.CombineDependencies(sortSrcEntities, sortDstEntities);

                                // @TODO `srcEntityGuidLookup` and  `dstEntityGuidLookup` is a real problem here.
                                // We need to be prepared to lookup ANY entity in the world in order to resolve references.
                                // This means we need a lookup for all possible entities in the world with an `EntityGuid` component
                                // Furthermore we are generating the inverse diff which means we need this lookup for both worlds.
                                //
                                // * One optimization we can do is maintain and update the `dstEntityGuidLookup` since that world is only ever updated here.
                                //
                                // For now we just brute force this with jobs and burst...
                                //
                                var buildEntityGuidLookups = JobHandle.CombineDependencies(
                                    new BuildEntityGuidLookup
                                    {
                                        Chunks = srcChunks,
                                        Lookup = srcEntityGuidLookup.ToConcurrent(),
                                        EntityGuidTypeIndex = TypeManager.GetTypeIndex<EntityGuid>()
                                    }.Schedule(srcChunks.Length, Math.Max(createdChunks.Length / 4, 1)),
                                    new BuildEntityGuidLookup
                                    {
                                        Chunks = dstChunks,
                                        Lookup = dstEntityGuidLookup.ToConcurrent(),
                                        EntityGuidTypeIndex = TypeManager.GetTypeIndex<EntityGuid>()
                                    }.Schedule(dstChunks.Length, Math.Max(destroyedChunks.Length / 4, 1)));

                                // Resolve the graph built above
                                //
                                // srcEntities -- sortSrcEntities --
                                //                                  |-- sortEntities --
                                // dstEntities -- sortDstEntities --                   |
                                //                                                     |-- Complete()
                                // srcEntityGuidLookups --                             |
                                //                        |-- buildEntityGuidLookups --
                                // dstEntityGuidLookups --
                                //
                                JobHandle.CombineDependencies(sortEntities, buildEntityGuidLookups).Complete();

                                var srcState = new WorldState
                                {
                                    Entities = srcEntities,
                                    EntityGuidLookup = srcEntityGuidLookup,
                                    ManagedComponentStore = m_SrcWorld.EntityManager.ManagedComponentStore
                                };

                                var dstState = new WorldState
                                {
                                    Entities = dstEntities,
                                    EntityGuidLookup = dstEntityGuidLookup,
                                    ManagedComponentStore = m_DstWorld.EntityManager.ManagedComponentStore
                                };

                                WorldDiff worldDiff;
                                WorldDiff inverseWorldDiff;

                                // With all shared work done we can now resolve the actual data diff(s)
                                // We can simply invert the srcEntities and dstEntities to generate the inverseDiff.
                                //
                                // @NOTE We are computing both the diff and inverse diff at the same time. This means we need double the buffers :(
                                //       This could be troublesome for large worlds and big changes (e.g. initial load)
                                using (var worldDiffScope = new WorldDiffScope(dstState, srcState, m_TypeInfoStream, k_Allocator))
                                using (var inverseWorldDiffScope = new WorldDiffScope(srcState, dstState, m_TypeInfoStream, k_Allocator))
                                {
                                    // Schedule all jobs for the diff generation in parallel.
                                    JobHandle.CombineDependencies(worldDiffScope.ScheduleJobs(), inverseWorldDiffScope.ScheduleJobs()).Complete();

                                    worldDiff = worldDiffScope.GetWorldDiff(Allocator.Persistent);
                                    inverseWorldDiff = inverseWorldDiffScope.GetWorldDiff(Allocator.Persistent);
                                }

                                changes = new Changes(worldDiff, inverseWorldDiff);
                                result = true;
                            }

                            // Drop all destroyed chunks
                            for (var i = 0; i < destroyedChunks.Length; i++)
                            {
                                var dstChunk = destroyedChunks[i].m_Chunk;

                                m_DstChunkToSrcChunkSequenceHashMap.Remove(dstChunk->SequenceNumber);

                                EntityManagerMoveEntitiesUtility.DestroyChunkForDiffing(dstChunk,
                                                                                        m_DstWorld.EntityManager.EntityComponentStore,
                                                                                        m_DstWorld.EntityManager.ManagedComponentStore);
                            }

                            // Clone all new chunks
                            var dstClonedChunks = new NativeArray<ArchetypeChunk>(createdChunks.Length, k_Allocator, NativeArrayOptions.UninitializedMemory);
                            {
                                for (var i = 0; i < createdChunks.Length; i++)
                                {
                                    var srcChunk = createdChunks[i].m_Chunk;

                                    // Do a full clone of this chunk
                                    var dstChunk = CloneChunkWithoutAllocatingEntities(
                                        m_DstWorld.EntityManager,
                                        srcChunk,
                                        m_SrcWorld.EntityManager.ManagedComponentStore);

                                    dstClonedChunks[i] = new ArchetypeChunk {m_Chunk = dstChunk};
                                    m_DstChunkToSrcChunkSequenceHashMap.TryAdd(dstChunk->SequenceNumber, srcChunk->SequenceNumber);
                                }

                                // Ensure capacity since we can not resize a concurrent hash map in a parallel job.
                                m_DstChunkToSrcChunkSequenceHashMap.Capacity = Math.Max(
                                    m_DstChunkToSrcChunkSequenceHashMap.Capacity,
                                    m_DstChunkToSrcChunkSequenceHashMap.Length + createdChunks.Length);

                                // Ensure capacity in the dst world before we start linking entities.
                                m_DstWorld.EntityManager.EntityComponentStore->ReallocCapacity(m_SrcWorld.EntityManager.EntityCapacity);

                                new PatchClonedChunks
                                {
                                    SrcChunks = createdChunks,
                                    DstChunks = dstClonedChunks,
                                    DstChunkToSrcChunkSequenceHashMap = m_DstChunkToSrcChunkSequenceHashMap.ToConcurrent(),
                                    DstEntityComponentStore = m_DstWorld.EntityManager.EntityComponentStore
                                }.Schedule(createdChunks.Length, 64).Complete();
                            }
                            dstClonedChunks.Dispose();
                        }
                        var changedArchetypes =  m_DstWorld.EntityManager.EntityComponentStore->EndArchetypeChangeTracking(archetypeChanges);
                        m_DstWorld.EntityManager.EntityGroupManager.AddAdditionalArchetypes(changedArchetypes);
                    }
                }
            }

            m_SrcWorld.GetOrCreateSystem<ChangeTrackerSystem>().Update();
            return result;
        }

        [BurstCompile]
        private unsafe struct PatchClonedChunks : IJobParallelFor
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> SrcChunks;
            [ReadOnly] public NativeArray<ArchetypeChunk> DstChunks;

            [NativeDisableUnsafePtrRestriction] public EntityComponentStore* DstEntityComponentStore;

            public NativeHashMap<ulong, ulong>.Concurrent DstChunkToSrcChunkSequenceHashMap;

            public void Execute(int index)
            {
                var srcChunk = SrcChunks[index].m_Chunk;
                var dstChunk = DstChunks[index].m_Chunk;

                var archetype = srcChunk->Archetype;
                var typeCount = archetype->TypesCount;

                for (var typeIndex = 0; typeIndex < typeCount; typeIndex++)
                {
                    dstChunk->SetChangeVersion(typeIndex, srcChunk->GetChangeVersion(typeIndex));
                }

                DstEntityComponentStore->AddExistingEntitiesInChunk(dstChunk);
                DstChunkToSrcChunkSequenceHashMap.TryAdd(dstChunk->SequenceNumber, srcChunk->SequenceNumber);
            }
        }

        /// <remarks>
        /// This is essentially a copy paste of <see cref="EntityManagerMoveEntitiesUtility.CloneChunkForDiffing"/> except skipping the `AllocateEntities` step.
        /// </remarks>
        private static unsafe Chunk* CloneChunkWithoutAllocatingEntities(EntityManager dstEntityManager, Chunk* srcChunk, ManagedComponentStore srcSharedComponentManager)
        {
            var dstEntityComponentStore = dstEntityManager.EntityComponentStore;
            var dstEntityGroupManager = dstEntityManager.EntityGroupManager;
            var dstManagedComponentStore = dstEntityManager.ManagedComponentStore;

            // Copy shared component data
            var dstSharedIndices = stackalloc int[srcChunk->Archetype->NumSharedComponents];
            srcChunk->SharedComponentValues.CopyTo(dstSharedIndices, 0, srcChunk->Archetype->NumSharedComponents);
            dstManagedComponentStore.CopySharedComponents(srcSharedComponentManager, dstSharedIndices, srcChunk->Archetype->NumSharedComponents);

            // Allocate a new chunk
            var srcArchetype = srcChunk->Archetype;
            var dstArchetype = EntityManagerCreateArchetypeUtility.GetOrCreateArchetype(srcArchetype->Types,
                        srcArchetype->TypesCount, dstEntityComponentStore);

            var dstChunk = EntityManagerCreateDestroyEntitiesUtility.GetCleanChunk(
                dstArchetype,
                dstSharedIndices,
                dstEntityComponentStore,
                dstManagedComponentStore);

            // Release any references obtained by GetCleanChunk & CopySharedComponents
            for (var i = 0; i < srcChunk->Archetype->NumSharedComponents; i++)
            {
                dstManagedComponentStore.RemoveReference(dstSharedIndices[i]);
            }

            Assert.AreEqual(0, dstChunk->Count);
            Assert.IsTrue(dstChunk->Capacity >= srcChunk->Count);

            EntityManagerCreateDestroyEntitiesUtility.SetChunkCount(
                dstChunk,
                srcChunk->Count,
                dstEntityComponentStore,
                dstManagedComponentStore);

            dstChunk->Archetype->EntityCount += srcChunk->Count;

            var copySize = Chunk.GetChunkBufferSize();
            UnsafeUtility.MemCpy(dstChunk->Buffer, srcChunk->Buffer, copySize);

            BufferHeader.PatchAfterCloningChunk(dstChunk);

            return dstChunk;
        }

        private static unsafe NativeArray<TDestination> ToArray<TDestination, TSource>(NativeArray<TSource> source, Allocator allocator)
            where TDestination : unmanaged
            where TSource : unmanaged
        {
            Assert.AreEqual(sizeof(TDestination), sizeof(TSource));
            var destination = new NativeArray<TDestination>(source.Length, allocator, NativeArrayOptions.UninitializedMemory);
            UnsafeUtility.MemCpy(destination.GetUnsafePtr(), source.GetUnsafePtr(), sizeof(TDestination) * source.Length);
            return destination;
        }
    }
}
#endif
