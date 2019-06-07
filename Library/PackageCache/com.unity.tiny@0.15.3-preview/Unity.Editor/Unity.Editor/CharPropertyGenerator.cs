using System;
using System.Linq;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Properties;
using Unity.Properties.Reflection;

namespace Unity.Editor
{
    internal class CharPropertyGenerator : IReflectedPropertyGenerator
    {
        public bool Generate<TContainer, TValue>(FieldInfo field, ReflectedPropertyBag<TContainer> propertyBag)
        {
            if (!typeof(TContainer).IsValueType)
            {
                return false;
            }

            if (field.FieldType != typeof(char))
            {
                return false;
            }

            var propertyType = typeof(UnmanagedProperty<,>).MakeGenericType(typeof(TContainer), field.FieldType);
            var property = Activator.CreateInstance(propertyType, field.Name, UnsafeUtility.GetFieldOffset(field), new PropertyAttributeCollection(field.GetCustomAttributes().ToArray()));
            propertyBag.AddProperty<IProperty<TContainer, TValue>, TValue>((IProperty<TContainer, TValue>) property);
            return true;
        }
    }
}
