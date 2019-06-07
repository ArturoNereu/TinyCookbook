using System;
using System.Reflection;
using Unity.Authoring;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal interface IBufferData
    {
        Type ElementType { get; }
    }

    internal class BufferDataElement<TBufferElementData> : DataElement<DynamicBuffer<TBufferElementData>>, IBufferData
        where TBufferElementData : struct, IBufferElementData
    {
        public Type ElementType { get; }
        public override Type ComponentType { get; } = typeof(TBufferElementData);

        public BufferDataElement(Session session, NativeArray<Entity> targets, IInspector<DynamicBuffer<TBufferElementData>> inspector) : base(session, targets, inspector)
        {
            ElementType = typeof(TBufferElementData);
        }

        protected override bool HasData(Entity entity)
        {
            return EntityManager.HasComponent<TBufferElementData>(entity);
        }

        protected override DynamicBuffer<TBufferElementData> GetData(Entity entity)
        {
            return EntityManager.GetBuffer<TBufferElementData>(entity);
        }

        public void RemoveDataAtIndex(int index)
        {
            var buffer = GetData(MainTarget);
            buffer.RemoveAt(index);
            ResetIndices(index);
            EntityInspector.ForceRebuildAll();
        }

        public void AddNewItem()
        {
            var buffer = GetData(MainTarget);
            buffer.Add(new TBufferElementData());
            EntityInspector.ForceRebuildAll();
        }

        private void ResetIndices(int removed)
        {
            using (var pooled = ListPool<StructItemDataElement<TBufferElementData>>.GetDisposable())
            {
                var list = pooled.List;
                contentContainer.Query<StructItemDataElement<TBufferElementData>>().ToList(list);
                contentContainer.Q<Foldout>().contentContainer.Remove(list[removed]);
                for(var i = removed + 1; i < list.Count; ++i)
                {
                    var item = list[i];
                    item.Index -= 1;
                }
            }
        }

        protected override void SetData(Entity entity, DynamicBuffer<TBufferElementData> data)
        {
            // Nothing to do
        }

        protected override void RemoveComponent(Entity entity)
        {
            EntityManager.RemoveComponentRaw(entity, TypeManager.GetTypeIndex(typeof(TBufferElementData)));
        }

        protected override Assembly Assembly => typeof(TBufferElementData).Assembly;

        protected override bool IsTagComponent => false;

        public override unsafe T GetDataAtOffset<T>(int offset)
        {
            var component = Data;
            var size = UnsafeUtility.SizeOf<TBufferElementData>();
            var index = offset / size;
            offset %= size;
            var item = component[index];
            UnsafeUtility.CopyPtrToStructure((byte*) UnsafeUtility.AddressOf(ref item) + offset, out T data);
            return data;
        }

        public override unsafe void SetDataAtOffset<T>(T data, int offset)
        {
            var current = Data;
            var size = UnsafeUtility.SizeOf<TBufferElementData>();
            var index = offset / size;
            offset %= size;
            var item = current[index];
            var apply = (byte*) UnsafeUtility.AddressOf(ref item) + offset;
            UnsafeUtility.CopyStructureToPtr(ref data, apply);
            current[index] = item;
            m_Data = GetData(MainTarget);
        }

        public override void BuildFromVisitor<TProperty, TContainer, TValue>(IPropertyVisitor visitor, TProperty property,
            ref TContainer container, ref TValue value, InspectorContext context)
        {
            base.BuildFromVisitor(visitor, property, ref container, ref value, context);

            if (null == Inspector)
            {
                var addNewItemButton = new Button(AddNewItem)
                {
                    text = "Add New Item"
                };
                addNewItemButton.AddToClassList("unity-ecs-add-buffer-item");
                Add(addNewItemButton);
            }
        }
    }
}
