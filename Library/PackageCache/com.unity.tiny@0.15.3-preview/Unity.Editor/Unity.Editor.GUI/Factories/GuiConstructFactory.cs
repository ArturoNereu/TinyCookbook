using System;
using Unity.Authoring.Core;
using Unity.Properties;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal static class GuiConstructFactory
    {
        private static TElement ConstructBase<TProperty, TContainer, TElement, TFieldValue, TValue>(
            TProperty property)
            where TProperty : IProperty<TContainer, TValue>
            where TElement : BaseField<TFieldValue>, new()
        {
            var element = new TElement();
            SetNames(property, element);
            SetTooltip(property, element);
            return element;
        }

        public static void SetNames<TProperty, TValue>(TProperty property, BaseField<TValue> element)
            where TProperty : IProperty
        {
            var name = property.GetName();
            element.name = name;
            element.label = name;
            element.bindingPath = name;
            element.AddToClassList(name);
        }

        public static void SetTooltip<TProperty>(TProperty property, VisualElement element)
            where TProperty : IProperty
        {
            if(property.Attributes?.HasAttribute<TooltipAttribute>() ?? false)
            {
                element.tooltip = property.Attributes.GetAttribute<TooltipAttribute>().Tooltip;
            }
        }

        public static TElement Construct<TProperty, TElement, TContainer, TValue>(
            TProperty property,
            ref TContainer container,
            ref TValue value,
            InspectorContext context
        )
            where TProperty : IProperty<TContainer, TValue>
            where TElement : BaseField<TValue>, new()
            where TValue : struct
        {
            return Construct<TProperty, TElement, TValue, TContainer, TValue>(property, ref container, ref value, context);
        }

        public static TElement Construct<TProperty, TElement, TFieldType, TContainer, TValue>(
            TProperty property,
            ref TContainer container,
            ref TValue value,
            InspectorContext context
        )
            where TProperty : IProperty<TContainer, TValue>
            where TElement : BaseField<TFieldType>, new()
            where TValue : struct
        {
            var element = ConstructBase<TProperty, TContainer, TElement, TFieldType, TValue>(property);

            if (TranslatorFactory<TValue>.TryGetUpdater(element, out var t))
            {
                var offset = context.GetOffsetOfField(property.GetName());
                var index = context.GetCurrentIndex();
                var setter = context.DataProvider;
                t.SetUpdaters(element, setter, offset, index);
            }
            else
            {
                UnityEngine.Debug.Log($"No data translator registered between {typeof(TValue).Name} and {typeof(TFieldType).Name}");
            }

            if (context.GetParent(out var parent))
            {
                parent.contentContainer.Add(element);
            }

            if (property.IsContainer)
            {
                context.PushParent(element);
            }
            return element;
        }

        internal static void SetDataUpdater<TProperty, TContainer, TValue>(TProperty property, EnumField element, InspectorContext context, string name)
            where TProperty : IProperty<TContainer, TValue>
        {
            var type = Enum.GetUnderlyingType(typeof(TValue));
            if (type == typeof(byte))
            {
                SetDataUpdater<TValue, byte>(element, name, context);
            }
            else if (type == typeof(sbyte))
            {
                SetDataUpdater<TValue, sbyte>(element, name, context);
            }
            else if (type == typeof(short))
            {
                SetDataUpdater<TValue, short>(element, name, context);
            }
            else if (type == typeof(ushort))
            {
                SetDataUpdater<TValue, ushort>(element, name, context);
            }
            else if (type == typeof(int))
            {
                SetDataUpdater<TValue, int>(element, name, context);
            }
            else if (type == typeof(uint))
            {
                SetDataUpdater<TValue, uint>(element, name, context);
            }
            else if (type == typeof(long))
            {
                SetDataUpdater<TValue, long>(element, name, context);
            }
            else if (type == typeof(ulong))
            {
                SetDataUpdater<TValue, ulong>(element, name, context);
            }
        }

        private static void SetDataUpdater<TEnumType, TValue>(EnumField element, string name, InspectorContext context)
        where TValue : struct
        {
            var offset = context.GetOffsetOfField(name);
            var setter = context.DataProvider;

            if (null == setter)
            {
                element.SetEnabled(false);
            }
            else
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    setter.SetDataAtOffset((TValue) (object) evt.newValue, offset);
                });

                element.userData = new GuiUpdaters.EnumFieldUpdater<TEnumType, TValue>(
                    setter,
                    offset,
                    element,
                    new Translator<Enum, TValue>
                    {
                        ToField = v => (Enum) Enum.ToObject(typeof(TEnumType), v),
                        ToValue = v => (TValue) (v as IConvertible).ToType(typeof(TValue), null)
                    });
            }
        }
    }
}
