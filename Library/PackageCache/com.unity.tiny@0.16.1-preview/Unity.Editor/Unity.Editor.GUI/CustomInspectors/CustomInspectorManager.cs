using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Authoring;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Assertions;

namespace Unity.Editor
{
    [UsedImplicitly]
    internal class CustomInspectorManager : ISessionManagerInternal
    {
        private Session m_Session;

        public void Load(Session session)
        {
            m_Session = session;
        }

        public void Unload(Session session) { }

        internal IComponentDataElement CreateComponentDataElement<TValue>(NativeArray<Entity> targets, ref TValue value)
        {
            if (typeof(IComponentData).IsAssignableFrom(typeof(TValue)))
            {
                var inspector = GetCodeInspectorForType<TValue>();
                var type = typeof(ComponentDataElement<>).MakeGenericType(typeof(TValue));
                return (IComponentDataElement) Activator.CreateInstance(type, m_Session, targets, inspector);
            }
            if (typeof(ISharedComponentData).IsAssignableFrom(typeof(TValue)))
            {
                var inspector = GetSharedComponentInspectorForType<TValue>();
                var type = typeof(SharedComponentDataElement<>).MakeGenericType(typeof(TValue));
                return (IComponentDataElement) Activator.CreateInstance(type, m_Session, targets, inspector);
            }

            if (typeof(IDynamicBufferContainer).IsAssignableFrom(typeof(TValue)))
            {
                var elementType = (value as IDynamicBufferContainer).ElementType;
                var inspector = GetBufferInspectorForType(elementType);
                var type = typeof(BufferDataElement<>).MakeGenericType(elementType);
                return (IComponentDataElement) Activator.CreateInstance(type, m_Session, targets, inspector);
            }
            throw new NotImplementedException();
        }

        internal IStructDataElement CreateStructDataElement<TComponentData, TValue>(
            IComponentDataElement root,
            string name,
            int index,
            int offset)
        {
            Assert.IsTrue(
                typeof(IComponentData).IsAssignableFrom(typeof(TComponentData)) ||
                typeof(ISharedComponentData).IsAssignableFrom(typeof(TComponentData)) ||
                typeof(IBufferElementData).IsAssignableFrom(typeof(TComponentData)));

            var elementSize = Unsafe.SizeOf<TComponentData>();

            if (typeof(IComponentData).IsAssignableFrom(typeof(TComponentData)) ||
                typeof(ISharedComponentData).IsAssignableFrom(typeof(TComponentData)))
            {
                var inspector = GetStructInspectorForType<TValue>();
                var type = typeof(StructDataElement<,>).MakeGenericType(typeof(TComponentData), typeof(TValue));
                return (IStructDataElement) Activator.CreateInstance(type, inspector, root, index, offset, elementSize, name);
            }
            if (typeof(IBufferElementData).IsAssignableFrom(typeof(TComponentData)))
            {
                var inspector = GetStructInspectorForType<TValue>();
                var type = typeof(StructDataElement<,>).MakeGenericType(typeof(DynamicBuffer<>).MakeGenericType(typeof(TComponentData)), typeof(TValue));
                return (IStructDataElement) Activator.CreateInstance(type, inspector, root, index, offset, elementSize, name);
            }
            throw new NotImplementedException();
        }

        internal IStructDataElement CreateStructDataElement<TValue>(
            IComponentDataElement root,
            string name,
            int index, int offset)
        {
            var inspector = GetStructInspectorForType<TValue>();
            var type = typeof(StructItemDataElement<>).MakeGenericType(typeof(TValue));
            return (IStructDataElement) Activator.CreateInstance(type, inspector, root, index, offset, name);
        }

        private static IInspector<TComponentData> GetCodeInspectorForType<TComponentData>()
        {
            return (IInspector<TComponentData>)CustomInspectorDatabase.GetCustomInspectorForType(typeof(TComponentData));
        }

        private static IInspector<TSharedComponentData> GetSharedComponentInspectorForType<TSharedComponentData>()
        {
            return (IInspector<TSharedComponentData>)CustomInspectorDatabase.GetSharedComponentInspectorForType(typeof(TSharedComponentData));
        }

        private static IInspector GetCodeInspectorForType(Type type)
        {
            return CustomInspectorDatabase.GetCustomInspectorForType(type);
        }

        private static IInspector<T> GetStructInspectorForType<T>()
        {
            return (IInspector<T>)CustomInspectorDatabase.GetCustomStructInspectorForType(typeof(T));
        }

        private static IInspector<T> GetBufferInspectorForType<T>()
        {
            return (IInspector<T>)CustomInspectorDatabase.GetCustomBufferInspectorForType(typeof(T));
        }

        private static IInspector GetBufferInspectorForType(Type type)
        {
            return CustomInspectorDatabase.GetCustomBufferInspectorForType(type);
        }
    }
}
