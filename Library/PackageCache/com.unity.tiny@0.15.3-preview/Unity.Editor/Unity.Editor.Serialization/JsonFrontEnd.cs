using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Authoring.Hashing;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Properties;
using Unity.Serialization;
using Unity.Tiny.Scenes;

namespace Unity.Editor.Serialization
{
    internal static class JsonFrontEnd
    {
        public const int EntityBatchSize = 100;

        static JsonFrontEnd()
        {
            TypeConversion.Register<SerializedStringView, NativeString64>(v => new NativeString64(v.ToString()));
            TypeConversion.Register<SerializedStringView, NativeString512>(v => new NativeString512(v.ToString()));
            TypeConversion.Register<SerializedStringView, NativeString4096>(v => new NativeString4096(v.ToString()));
        }

        private struct TransferComponentData : IContainerTypeCallback
        {
            public SerializedObjectView Source;
            public unsafe void* Destination;
            public NativeList<EntityReferenceRemap> EntityReferenceRemap;

            public unsafe void Invoke<TComponent>()
            {
                ref var component = ref System.Runtime.CompilerServices.Unsafe.AsRef<TComponent>(Destination);
                PropertyContainer.Transfer(ref component, ref Source);
                ExtractEntityReferenceRemap(EntityReferenceRemap, ref component, ref Source);
            }
        }

        private struct TransferSharedComponentData : IContainerTypeCallback
        {
            public SerializedObjectView Source;
            public EntityManager EntityManager;
            public Entity Entity;
            public int TypeIndex;
            public NativeList<EntityReferenceRemap> EntityReferenceRemap;

            public void Invoke<TComponent>()
            {
                var component = (TComponent)EntityManager.GetSharedComponentData(Entity, TypeIndex);
                PropertyContainer.Transfer(ref component, ref Source);
                ExtractEntityReferenceRemap(EntityReferenceRemap, ref component, ref Source);
                EntityManager.SetSharedComponentDataBoxed(Entity, TypeIndex, component);
            }
        }

        private unsafe struct TransferBufferElementData : IContainerTypeCallback
        {
            public SerializedObjectView Source;
            public BufferHeader* Buffer;
            public NativeList<EntityReferenceRemap> EntityReferenceRemap;

            public void Invoke<TBuffer>()
            {
                var elements = Source["Elements"].AsArrayView();

                // this is not ideal since we must enumerate the entire collection...
                var length = elements.Count();

                var size = Unsafe.SizeOf<TBuffer>();

                Buffer->Length = length;
                BufferHeader.EnsureCapacity(Buffer, length, size, 4, BufferHeader.TrashMode.RetainOldData);

                var i = 0;
                var remapOffset = 0;
                foreach (var element in elements)
                {
                    var src = element.AsObjectView();
                    var ptr = BufferHeader.GetElementPointer(Buffer) + size * i;
                    UnsafeUtility.MemClear(ptr, size);
                    ref var dst = ref Unsafe.AsRef<TBuffer>(ptr);
                    PropertyContainer.Transfer(ref dst, ref src);
                    ExtractEntityReferenceRemap(EntityReferenceRemap, ref dst, ref src, remapOffset);
                    remapOffset += Unsafe.SizeOf<TBuffer>();
                    i++;
                }
            }
        }

        /// <summary>
        /// This visitor is responsible for extracting all serialized entity references and transforming them into <see cref="T:Unity.Tiny.Scenes.EntityReferenceRemap" /> structs.
        /// </summary>
        private struct SerializedEntityReferenceVisitor : IPropertyVisitor
        {
            public TypeManager.TypeInfo TypeInfo;
            public int BaseOffset;
            public int EntityReferenceRemapStartIndex;
            public NativeList<EntityReferenceRemap> EntityReferenceRemap;
            public SerializedObjectView Source;

            public VisitStatus VisitProperty<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref ChangeTracker propertyChangeTracker)
                where TProperty : IProperty<TContainer, TValue>
            {
                if (typeof(TValue) == typeof(Entity))
                {
                    // We have found an entity reference.
                    if (!Source.TryGetValue(property.GetName(), out var srcValue) || srcValue.Type != TokenType.String)
                    {
                        // We are expecting a string at this point.
                        return VisitStatus.Override;
                    }

                    var guid = new Guid(srcValue.AsStringView().ToString());
                    var offsetIndex = EntityReferenceRemap.Length - EntityReferenceRemapStartIndex;

                    if (offsetIndex >= TypeInfo.EntityOffsetCount)
                    {
                        throw new Exception("EntityOffsetCount mismatch");
                    }

                    var offset = BaseOffset + TypeInfo.EntityOffsets[offsetIndex].Offset;

                    EntityReferenceRemap.Add(new EntityReferenceRemap
                    {
                        Guid = guid.ToEntityGuid(),
                        TypeHash = TypeInfo.StableTypeHash,
                        Offset = offset
                    });

                    return VisitStatus.Override;
                }

                if (property.IsContainer)
                {
                    if (!Source.TryGetValue(property.GetName(), out var srcValue) || srcValue.Type != TokenType.Object)
                    {
                        // Skip this branch since the structures do not match.
                        return VisitStatus.Override;
                    }

                    var dstValue = property.GetValue(ref container);

                    PropertyContainer.Visit(ref dstValue, new SerializedEntityReferenceVisitor
                    {
                        TypeInfo = TypeInfo,
                        BaseOffset = BaseOffset,
                        EntityReferenceRemapStartIndex = EntityReferenceRemapStartIndex,
                        EntityReferenceRemap = EntityReferenceRemap,
                        Source = srcValue.AsObjectView()
                    });
                }

                return VisitStatus.Override;
            }

            public VisitStatus VisitCollectionProperty<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref ChangeTracker changeTracker) where TProperty : ICollectionProperty<TContainer, TValue>
            {
                throw new NotSupportedException();
            }
        }

        private static void ExtractEntityReferenceRemap<TDestination>(NativeList<EntityReferenceRemap> entityReferenceRemap, ref TDestination dst, ref SerializedObjectView src, int baseOffset = 0)
        {
            var typeIndex = TypeManager.GetTypeIndex(typeof(TDestination));

            if (!TypeManager.HasEntityReferences(typeIndex))
            {
                return;
            }

            var typeInfo = TypeManager.GetTypeInfo(typeIndex);

            PropertyContainer.Visit(ref dst, new SerializedEntityReferenceVisitor
            {
                TypeInfo = typeInfo,
                BaseOffset = baseOffset,
                EntityReferenceRemapStartIndex = entityReferenceRemap.Length,
                EntityReferenceRemap = entityReferenceRemap,
                Source = src
            });
        }

        /// <summary>
        /// Reads a collection of entities from the given object reader and adds them to the given world.
        /// </summary>
        public static unsafe void Accept(EntityManager entityManager, SerializedObjectReader reader)
        {
            var views = stackalloc SerializedValueView[EntityBatchSize];

            NodeType node;
            if ((node = reader.Step()) != NodeType.BeginArray)
            {
                throw new Exception($"Error trying to read entity collection. Expected NodeType=[BeginArray] but received NodeType=[{node}]");
            }

            using (var componentTypes = new NativeList<ComponentType>(32, Allocator.Temp))
            using (var componentIndices = new NativeList<int>(32, Allocator.Temp))
            using (var entityReferenceRemap = new NativeList<EntityReferenceRemap>(Allocator.TempJob))
            {
                int count;
                while ((count = reader.ReadArrayElementBatch(views, EntityBatchSize)) != 0)
                {
                    for (var i = 0; i < count; i++)
                    {
                        entityReferenceRemap.Clear();
                        componentIndices.Clear();
                        componentTypes.Clear();

                        var view = views[i].AsObjectView();
                        var components = view["Components"].AsArrayView();

                        var componentIndex = 0;

                        foreach (var element in components)
                        {
                            var component = element.AsObjectView();
                            var typeName = component["$type"].AsStringView().ToString();

                            // @TODO This should be more robust
                            if (!typeName.Contains(", Version="))
                            {
                                typeName += ", Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
                            }

                            var type = Type.GetType(typeName);

                            if (null != type)
                            {
                                componentTypes.Add(new ComponentType(type, ComponentType.AccessMode.ReadWrite));
                                componentIndices.Add(componentIndex++);
                            }
                            else
                            {
                                // Failed to resolve this component. Just skip over it.
                                Debug.LogWarning($"Failed to resolve component TypeName=[{typeName}]");
                                componentIndices.Add(-1);
                            }
                        }

                        var entity = entityManager.CreateEntity(entityManager.CreateArchetype((ComponentType*)componentTypes.GetUnsafePtr(), componentTypes.Length));

                        componentIndex = 0;

                        foreach (var element in components)
                        {
                            var index = componentIndices[componentIndex++];

                            if (index == -1)
                            {
                                continue;
                            }

                            var type = componentTypes[index];

                            var propertyBag = PropertyBagResolver.Resolve(type.GetManagedType());

                            if (type.IsBuffer)
                            {
                                propertyBag?.Cast(new TransferBufferElementData
                                {
                                    Source = element.AsObjectView(),
                                    Buffer = (BufferHeader*)entityManager.GetComponentDataRawRW(entity, type.TypeIndex),
                                    EntityReferenceRemap = entityReferenceRemap
                                });

                                continue;
                            }

                            if (type.IsSharedComponent)
                            {
                                propertyBag?.Cast(new TransferSharedComponentData
                                {
                                    Source = element.AsObjectView(),
                                    EntityManager = entityManager,
                                    Entity = entity,
                                    TypeIndex = type.TypeIndex,
                                    EntityReferenceRemap = entityReferenceRemap
                                });

                                continue;
                            }

                            if (type.IsZeroSized)
                            {
                                continue;
                            }

                            propertyBag?.Cast(new TransferComponentData
                            {
                                Source = element.AsObjectView(),
                                Destination = entityManager.GetComponentDataRawRW(entity, type.TypeIndex),
                                EntityReferenceRemap = entityReferenceRemap
                            });
                        }

                        if (entityReferenceRemap.Length > 0)
                        {
                            var buffer = entityManager.AddBuffer<EntityReferenceRemap>(entity);
                            buffer.ResizeUninitialized(entityReferenceRemap.Length);
                            UnsafeUtility.MemCpy(buffer.GetUnsafePtr(), entityReferenceRemap.GetUnsafePtr(), entityReferenceRemap.Length * UnsafeUtility.SizeOf<EntityReferenceRemap>());
                        }
                    }

                    reader.DiscardCompleted();
                }

                // Read the ']' brace
                reader.Step();
            }
        }
    }
}
