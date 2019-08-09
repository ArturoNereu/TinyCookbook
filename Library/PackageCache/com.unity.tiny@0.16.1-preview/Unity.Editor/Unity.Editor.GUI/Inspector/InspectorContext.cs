using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal class InspectorContext
    {
        internal struct DataProviderScope : IDisposable
        {
            private InspectorContext Context { get; }

            public DataProviderScope(InspectorContext context, IComponentDataElement componentElement)
            {
                Context = context;
                Context.DataProvider = new ComponentOffsetDataProvider(componentElement);
            }

            public void Dispose()
            {
                Context.DataProvider = null;
            }
        }

        internal struct OffsetInfo
        {
            public Type Type;
            public string Name;
        }

        internal struct OffsetScope : IDisposable
        {
            private readonly List<OffsetInfo> m_Offsets;
            private readonly bool m_IsBuffer;

            internal OffsetScope(List<OffsetInfo> offsets, Type type, string fieldName)
            {
                m_Offsets = offsets;
                m_IsBuffer = typeof(IDynamicBufferContainer).IsAssignableFrom(type);
                if (!m_IsBuffer)
                {
                    m_Offsets.Add(new OffsetInfo {Type = type, Name = fieldName});
                }
            }

            public void Dispose()
            {
                if (!m_IsBuffer)
                {
                    m_Offsets.RemoveAt(m_Offsets.Count - 1);
                }
            }
        }

        private class ComponentOffsetDataProvider : IOffsetDataProvider
        {
            private IComponentDataElement Component { get; }

            public ComponentOffsetDataProvider(IComponentDataElement component)
            {
                Component = component;
            }

            public void SetDataAtOffset<TData>(TData data, int offset) where TData : struct
            {
                Component.SetDataAtOffset(data, offset);
            }

            public TData GetDataAtOffset<TData>(int offset) where TData : struct
            {
                return Component.GetDataAtOffset<TData>(offset);
            }

            public void SetDataAtOffset<TData>(TData data, int index, int offset) where TData : struct
            {
                if (Component is IBufferData buffer)
                {
                    var size = UnsafeUtility.SizeOf(buffer.ElementType);
                    SetDataAtOffset(data, index * size + offset);
                }
            }

            public TData GetDataAtOffset<TData>(int index, int offset) where TData : struct
            {
                if (Component is IBufferData buffer)
                {
                    var size = UnsafeUtility.SizeOf(buffer.ElementType);
                    return GetDataAtOffset<TData>(index * size + offset);
                }
                throw new NotImplementedException();
            }
        }

        internal struct StructInspectorFactoryScope<T> : IDisposable
        {
            private InspectorContext Context { get; }

            public StructInspectorFactoryScope(InspectorContext context, IComponentDataElement root, CustomInspectorManager customInspectors)
            {
                Context = context;
                Context.StructElementFactory = new StructDataElementFactory<T>(root, customInspectors);
            }

            public void Dispose()
            {
                Context.StructElementFactory = null;
            }
        }

        internal interface IStructDataElementFactory
        {
            IStructDataElement CreateForType<TValue>(string name, int index, int offset);
            IStructDataElement CreateItemForType<TValue>(string name, int index, int offset);
        }

        private class StructDataElementFactory<TComponentData> : IStructDataElementFactory
        {
            private IComponentDataElement Root { get; }
            private CustomInspectorManager CustomInspectors { get; }

            public StructDataElementFactory(IComponentDataElement root, CustomInspectorManager customInspectors)
            {
                Root = root;
                CustomInspectors = customInspectors;
            }

            public IStructDataElement CreateForType<TValue>(string name, int index, int offset)
                => CustomInspectors.CreateStructDataElement<TComponentData, TValue>(Root, name, index, offset);

            public IStructDataElement CreateItemForType<TValue>(string name, int index, int offset)
                => CustomInspectors.CreateStructDataElement<TValue>(Root, name, index, offset);
        }

        private readonly Stack<VisualElement> m_Parents;
        private readonly List<OffsetInfo> m_StructTypes;
        internal IOffsetDataProvider DataProvider { get; private set; }
        internal IStructDataElementFactory StructElementFactory { get; private set; }

        internal InspectorContext()
        {
            m_Parents = new Stack<VisualElement>();
            m_StructTypes = new List<OffsetInfo>();
        }

        public void PushParent(VisualElement parent)
        {
            m_Parents.Push(parent);
        }

        public void PopParent(VisualElement parent)
        {
            if (m_Parents.Peek() == parent)
            {
                m_Parents.Pop();
            }
            else
            {
                Debug.Log("Arrrrgh");
            }
        }

        public bool GetParent(out VisualElement parent)
        {
            if (m_Parents.Count > 0)
            {
                parent = m_Parents.Peek();
                return true;
            }

            parent = null;
            return false;
        }

        internal DataProviderScope NewDataProviderScope(IComponentDataElement componentElement)
        {
            return new DataProviderScope(this, componentElement);
        }

        internal OffsetScope NewOffsetScope(Type type, string fieldName)
        {
            return new OffsetScope(m_StructTypes, type, fieldName);
        }

        internal StructInspectorFactoryScope<T> NewStructInspectorFactoryScope<T>(IComponentDataElement root, CustomInspectorManager customInspectors)
        {
            return new StructInspectorFactoryScope<T>(this, root, customInspectors);
        }

        internal IDisposable NewStructItemInspectorFactoryScope(IComponentDataElement root, CustomInspectorManager customInspectors, Type elementType)
        {
            return (IDisposable) Activator.CreateInstance(
                typeof(StructInspectorFactoryScope<>).MakeGenericType(elementType),
                this, root, customInspectors);
        }

        public int GetCurrentOffset()
        {
            var offset = 0;

            var type = m_StructTypes[0].Type;
            for (var i = 1; i < m_StructTypes.Count; ++i)
            {
                var info = m_StructTypes[i];
                var fieldInfo = type.GetField(info.Name);
                    offset += UnsafeUtility.GetFieldOffset(fieldInfo);
                type = info.Type;
            }
            return offset;
        }

        public bool IsCurrentList()
        {
            var info = m_StructTypes[m_StructTypes.Count - 1];
            if (string.IsNullOrEmpty(info.Name))
            {
                return false;
            }

            if (info.Name.StartsWith("[") && info.Name.EndsWith("]"))
            {
                return true;
            }

            return false;
        }

        public int GetCurrentIndex()
        {
            for (var i = m_StructTypes.Count - 1; i >= 0; --i)
            {
                var info = m_StructTypes[i];
                if (string.IsNullOrEmpty(info.Name))
                {
                    return -1;
                }

                if (info.Name.StartsWith("[") && info.Name.EndsWith("]"))
                {
                    return int.Parse(info.Name.Substring(1, info.Name.Length - 2));
                }
            }

            return -1;
        }

        public int GetOffsetOfField(string fieldName)
        {
            var currentBase = GetCurrentOffset();
            var fieldInfo = m_StructTypes[m_StructTypes.Count - 1].Type.GetField(fieldName);
            currentBase += UnsafeUtility.GetFieldOffset(fieldInfo);
            return currentBase;
        }
    }
}
