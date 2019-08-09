using System;
using Unity.Entities;
using Unity.Properties;
using Unity.Serialization.Json;

namespace Unity.Editor.Serialization
{
    internal class EntityJsonVisitor : JsonVisitor
    {
        public EntityJsonVisitor(EntityManager entityManager)
        {
            AddAdapter(new EntityAdapter(this, entityManager));
            AddAdapter(new NativeStringAdapter(this));
        }

        public override bool IsExcluded<TProperty, TContainer, TValue>(TProperty property, ref TContainer container)
        {
            if (property.Attributes?.HasAttribute<NonSerializedAttribute>() ?? false)
            {
                return true;
            }

            return typeof(TValue).IsPrimitive && CustomEquality.Equals(property.GetValue(ref container), default(TValue));
        }

        protected override string GetTypeInfo<TProperty, TContainer, TValue>()
        {
            if (typeof(TContainer) != typeof(EntityContainer))
            {
                return null;
            }

            if (typeof(IComponentData).IsAssignableFrom(typeof(TValue)) ||
                typeof(ISharedComponentData).IsAssignableFrom(typeof(TValue)))
            {
                return GetFullyQualifiedTypeName(typeof(TValue));
            }

            if (typeof(IDynamicBufferContainer).IsAssignableFrom(typeof(TValue)))
            {
                return GetFullyQualifiedTypeName(typeof(TValue).GetGenericArguments()[0]);
            }

            return null;
        }

        private static string GetFullyQualifiedTypeName(Type type)
        {
            var name = type.AssemblyQualifiedName;

            if (null == name)
            {
                return null;
            }

            var index = name.IndexOf(", Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", StringComparison.InvariantCulture);
            return index != -1 ? name.Substring(0, index) : name;
        }
    }
}
