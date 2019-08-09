using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Authoring;
using Unity.Authoring.Core;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Editor.Conversion;
using Unity.Editor.Extensions;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Tiny.Codec;
using Unity.Tiny.Scenes;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Editor.Build
{
    internal partial class BuildStep
    {
        internal interface IBuildStep
        {
            string Description { get; }
            bool IsEnabled(BuildPipeline.BuildContext context);
            bool Run(BuildPipeline.BuildContext context);
        }

        private static bool ExportWorld(FileInfo outputFile, Project project, string scenePath, World world, bool performConversion = true)
        {
            if (performConversion)
            {
                SceneConversion.Convert(world);
            }

            // Null out any references to avoid the SerializeUtility from trying to patch asset entities.
            world.GetOrCreateSystem<ClearRemappedEntityReferenceSystem>().Update();

            // Remove non-exported components
            var nonExportedComponentTypes = UnityEditor.TypeCache.GetTypesWithAttribute<NonExportedAttribute>().Select(t => new ComponentType(t));
            world.EntityManager.RemoveComponent(world.EntityManager.UniversalQuery, new ComponentTypes(nonExportedComponentTypes.ToArray()));

            // Check for missing assembly references
            var unresolvedComponentTypes = GetAllUsedComponentTypes(world).Where(t => !DomainCache.IsIncludedInProject(project, t.GetManagedType())).ToArray();
            if (unresolvedComponentTypes.Length > 0)
            {
                foreach (var unresolvedComponentType in unresolvedComponentTypes)
                {
                    var type = unresolvedComponentType.GetManagedType();
                    Debug.LogError($"Could not resolve component type '{type.FullName}' while exporting {scenePath.HyperLink()}. Are you missing an assembly reference to '{type.Assembly.GetName().Name}' ?");
                }
                return false;
            }

            // Merges the entities and shared component streams, (optionally) compresses them, and finally serializes to disk with a small header in front
            using (var fileStream = new StreamBinaryWriter(outputFile.FullName))
            using (var entitiesWriter = new MemoryBinaryWriter())
            using (var sharedComponentWriter = new MemoryBinaryWriter())
            using (var combinedDataWriter = new MemoryBinaryWriter())
            {
                SerializeUtility.SerializeWorld(world.EntityManager, entitiesWriter, out var sharedComponentsToSerialize);
                if (sharedComponentsToSerialize.Length > 0)
                {
                    SerializeUtility.SerializeSharedComponents(world.EntityManager, sharedComponentWriter, sharedComponentsToSerialize);
                }

                unsafe
                {
                    combinedDataWriter.WriteBytes(sharedComponentWriter.Data, sharedComponentWriter.Length);
                    combinedDataWriter.WriteBytes(entitiesWriter.Data, entitiesWriter.Length);

                    var worldHeader = new SceneHeader();
                    worldHeader.SharedComponentCount = sharedComponentsToSerialize.Length;
                    worldHeader.DecompressedSize = entitiesWriter.Length + sharedComponentWriter.Length;
                    worldHeader.Codec = Codec.LZ4;
                    worldHeader.SerializeHeader(fileStream);

                    if (worldHeader.Codec != Codec.None)
                    {
                        int compressedSize = CodecService.Compress(worldHeader.Codec, combinedDataWriter.Data, combinedDataWriter.Length, out var compressedData);
                        fileStream.WriteBytes(compressedData, compressedSize);
                    }
                    else
                    {
                        fileStream.WriteBytes(combinedDataWriter.Data, combinedDataWriter.Length);
                    }
                }
            }

            return true;
        }

        private static unsafe Entity CopyEntity(Entity srcEntity, World srcWorld, World dstWorld)
        {
            Assert.AreNotEqual(Entity.Null, srcEntity);

            using (var entityReferences = new NativeList<EntityReferenceRemap>(8, Allocator.Temp))
            using (var componentTypes = srcWorld.EntityManager.GetComponentTypes(srcEntity))
            {
                var archetype = dstWorld.EntityManager.CreateArchetype(componentTypes.ToArray());
                var dstEntity = dstWorld.EntityManager.CreateEntity(archetype);

                if (componentTypes.Any(x => x.HasEntityReferences) && !dstWorld.EntityManager.HasComponent<EntityReferenceRemap>(dstEntity))
                {
                    dstWorld.EntityManager.AddBuffer<EntityReferenceRemap>(dstEntity);
                }

                foreach (var componentType in componentTypes)
                {
                    var typeInfo = TypeManager.GetTypeInfo(componentType.TypeIndex);

                    if (typeInfo.SizeInChunk == 0)
                    {
                        continue;
                    }

                    if (componentType.IsSharedComponent)
                    {
                        // @TODO For now we assume that all shared component data is blittable
                        var srcComponent = srcWorld.EntityManager.GetSharedComponentData(srcEntity, componentType.TypeIndex);

                        var ptr = Unsafe.AsPointer(ref srcComponent);

                        // Pull out all references into the `entityReferences` list
                        ExtractEntityReferences(entityReferences, typeInfo, srcWorld.EntityManager, ptr);

                        // Zero out entity references
                        ClearEntityReferences(typeInfo, ptr);

                        dstWorld.EntityManager.SetSharedComponentDataBoxed(dstEntity, componentType.TypeIndex, srcComponent);
                        continue;
                    }

                    if (componentType.IsBuffer)
                    {
                        var srcBuffer = (BufferHeader*)srcWorld.EntityManager.GetComponentDataRawRW(srcEntity, componentType.TypeIndex);
                        var dstBuffer = (BufferHeader*)dstWorld.EntityManager.GetComponentDataRawRW(dstEntity, componentType.TypeIndex);

                        dstBuffer->Length = srcBuffer->Length;
                        BufferHeader.EnsureCapacity(dstBuffer, srcBuffer->Length, typeInfo.ElementSize, 4, BufferHeader.TrashMode.RetainOldData);

                        // Copy all blittable data
                        UnsafeUtility.MemCpy(BufferHeader.GetElementPointer(dstBuffer), BufferHeader.GetElementPointer(srcBuffer), typeInfo.ElementSize * srcBuffer->Length);

                        for (var i = 0; i < srcBuffer->Length; i++)
                        {
                            var baseOffset = i * typeInfo.ElementSize;

                            // Pull out all references into the `entityReferences` list
                            ExtractEntityReferences(entityReferences, typeInfo, srcWorld.EntityManager, BufferHeader.GetElementPointer(dstBuffer), baseOffset);

                            // Zero out entity references
                            ClearEntityReferences(typeInfo, BufferHeader.GetElementPointer(dstBuffer), baseOffset);
                        }

                        continue;
                    }

                    var componentData = srcWorld.EntityManager.GetComponentDataRawRW(srcEntity, componentType.TypeIndex);

                    // Copy all blittable data
                    dstWorld.EntityManager.SetComponentDataRaw(dstEntity, componentType.TypeIndex, componentData, typeInfo.SizeInChunk);

                    // Pull out all references into the `entityReferences` list
                    ExtractEntityReferences(entityReferences, typeInfo, srcWorld.EntityManager, componentData);

                    // Zero out entity references
                    ClearEntityReferences(typeInfo, dstWorld.EntityManager.GetComponentDataRawRW(dstEntity, componentType.TypeIndex));
                }

                if (entityReferences.Length > 0)
                {
                    dstWorld.EntityManager.GetBuffer<EntityReferenceRemap>(dstEntity).AddRange(entityReferences);
                }

                return dstEntity;
            }
        }

        private static unsafe void ExtractEntityReferences(NativeList<EntityReferenceRemap> references, TypeManager.TypeInfo typeInfo, EntityManager manager, void* component, int baseOffset = 0)
        {
            if (!TypeManager.HasEntityReferences(typeInfo.TypeIndex))
            {
                return;
            }

            for (var i = 0; i < typeInfo.EntityOffsetCount; i++)
            {
                var offset = typeInfo.EntityOffsets[i].Offset + baseOffset;

                var target = *(Entity*)((byte*)component + offset);

                if (!manager.Exists(target))
                {
                    continue;
                }

                if (!manager.HasComponent<EntityGuid>(target))
                {
                    continue;
                }

                var guid = manager.GetComponentData<EntityGuid>(target);

                references.Add(new EntityReferenceRemap
                {
                    Guid = guid,
                    Offset = offset,
                    TypeHash = typeInfo.StableTypeHash
                });
            }
        }

        private static unsafe void ClearEntityReferences(TypeManager.TypeInfo typeInfo, void* component, int baseOffset = 0)
        {
            if (!TypeManager.HasEntityReferences(typeInfo.TypeIndex))
            {
                return;
            }

            for (var i = 0; i < typeInfo.EntityOffsetCount; i++)
            {
                var offset = typeInfo.EntityOffsets[i].Offset + baseOffset;
                *(Entity*)((byte*)component + offset) = Entity.Null;
            }
        }

        private static unsafe EntityArchetype[] GetAllArchetypes(World world)
        {
            using (var entityArchetypes = new NativeList<EntityArchetype>(Allocator.Temp))
            {
                world.EntityManager.GetAllArchetypes(entityArchetypes);
                return entityArchetypes.ToArray();
            }
        }

        private static unsafe ComponentType[] GetAllComponentTypes(EntityArchetype entityArchetype)
        {
            var archetype = entityArchetype.Archetype;
            using (var pool = ListPool<ComponentType>.GetDisposable())
            {
                var componentTypes = pool.List;
                for (var i = 0; i < archetype->TypesCount; ++i)
                {
                    componentTypes.Add(archetype->Types[i].ToComponentType());
                }
                return componentTypes.ToArray();
            }
        }

        private static ComponentType[] GetAllUsedComponentTypes(World world)
        {
            var componentTypes = new HashSet<ComponentType>();
            foreach (var archetype in GetAllArchetypes(world))
            {
                if (archetype.ChunkCount == 0)
                {
                    continue;
                }

                foreach (var componentType in GetAllComponentTypes(archetype))
                {
                    componentTypes.Add(componentType);
                }
            }
            return componentTypes.ToArray();
        }
    }
}
