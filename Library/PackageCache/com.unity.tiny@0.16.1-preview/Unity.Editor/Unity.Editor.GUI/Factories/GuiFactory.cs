using System;
using Unity.Properties;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.Authoring.Core;

namespace Unity.Editor
{
    internal static class GuiFactory
    {
        public static Foldout Foldout<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, InspectorContext context, string name = null)
            where TProperty : IProperty<TContainer, TValue>
        {
            var hasTooltip = property.Attributes?.HasAttribute<TooltipAttribute>() ?? false;
            var propertyName = property.GetName();
            var foldout = new Foldout
            {
                name = propertyName,
                text = name ?? propertyName,
                bindingPath = propertyName,
                tooltip = hasTooltip ? property.Attributes.GetAttribute<TooltipAttribute>().Tooltip : string.Empty,
            };
            GuiConstructFactory.SetTooltip(property, foldout);

            if (!context.GetParent(out var parent))
            {
                return foldout;
            }

            if (property.IsContainer)
            {
                parent.Add(foldout);
                context.PushParent(foldout);
            }

            return foldout;
        }

        public static Label Label<TProperty, TContainer, TValue>(TProperty property, ref TContainer container,
            ref TValue value, InspectorContext context)
        where TProperty : IProperty<TContainer, TValue>
        {
            var name = property.GetName();
            var label = new Label {text = name};
            if (context.GetParent(out var parent))
            {
                parent.Add(label);
            }

            if (property.IsContainer)
            {
                context.PushParent(label);
            }
            return label;
        }

        public static Toggle Toggle<TProperty, TContainer>(TProperty property, ref TContainer container, ref bool value, InspectorContext context)
            where TProperty : IProperty<TContainer, bool>
        {
            return GuiConstructFactory.Construct<TProperty, Toggle, TContainer, bool>(property, ref container, ref value, context);
        }

        public static IntegerField SByteField<TProperty, TContainer>(TProperty property, ref TContainer container, ref sbyte value, InspectorContext context)
            where TProperty : IProperty<TContainer, sbyte>
            => GuiConstructFactory.Construct<TProperty, IntegerField, int, TContainer, sbyte>(property, ref container, ref value, context);

        public static IntegerField ByteField<TProperty, TContainer>(TProperty property, ref TContainer container, ref byte value, InspectorContext context)
            where TProperty : IProperty<TContainer, byte>
            => GuiConstructFactory.Construct<TProperty, IntegerField, int, TContainer, byte>(property, ref container, ref value, context);

        public static IntegerField UShortField<TProperty, TContainer>(TProperty property, ref TContainer container, ref ushort value, InspectorContext context)
            where TProperty : IProperty<TContainer, ushort>
            => GuiConstructFactory.Construct<TProperty, IntegerField, int, TContainer, ushort>(property, ref container, ref value, context);

        public static IntegerField ShortField<TProperty, TContainer>(TProperty property, ref TContainer container, ref short value, InspectorContext context)
            where TProperty : IProperty<TContainer, short>
                => GuiConstructFactory.Construct<TProperty, IntegerField, int, TContainer, short>(property, ref container, ref value, context);

        public static IntegerField IntField<TProperty, TContainer>(TProperty property, ref TContainer container, ref int value, InspectorContext context)
            where TProperty : IProperty<TContainer, int>
        {
            return GuiConstructFactory.Construct<TProperty, IntegerField, TContainer, int>(property, ref container, ref value, context);
        }

        public static LongField UIntField<TProperty, TContainer>(TProperty property, ref TContainer container, ref uint value, InspectorContext context)
            where TProperty : IProperty<TContainer, uint>
            => GuiConstructFactory.Construct<TProperty, LongField, long, TContainer, uint>(property, ref container, ref value, context);

        public static LongField LongField<TProperty, TContainer>(TProperty property, ref TContainer container, ref long value, InspectorContext context)
            where TProperty : IProperty<TContainer, long>
        {
            return GuiConstructFactory.Construct<TProperty, LongField, TContainer, long>(property, ref container, ref value, context);
        }

        public static TextField ULongField<TProperty, TContainer>(TProperty property, ref TContainer container, ref ulong value, InspectorContext context)
            where TProperty : IProperty<TContainer, ulong>
            => GuiConstructFactory.Construct<TProperty, TextField, string, TContainer, ulong>(property, ref container, ref value, context);

        public static FloatField FloatField<TProperty, TContainer>(TProperty property, ref TContainer container,
            ref float value, InspectorContext context)
            where TProperty : IProperty<TContainer, float>
            => GuiConstructFactory.Construct<TProperty, FloatField, TContainer, float>(property, ref container,
                ref value, context);

        public static DoubleField DoubleField<TProperty, TContainer>(TProperty property, ref TContainer container, ref double value, InspectorContext context)
            where TProperty : IProperty<TContainer, double>
        {
            return GuiConstructFactory.Construct<TProperty, DoubleField, TContainer, double>(property, ref container, ref value, context);
        }

        public static TextField CharField<TProperty, TContainer>(TProperty property, ref TContainer container, ref char value, InspectorContext context)
            where TProperty : IProperty<TContainer, char>
        {
            return GuiConstructFactory.Construct<TProperty, TextField, string, TContainer, char>(property, ref container, ref value, context);
        }

        public static EnumField EnumField<TProperty, TContainer, TValue>(TProperty property, ref TContainer container,
            ref TValue value, InspectorContext context) where TProperty : IProperty<TContainer, TValue>
        {
            if (typeof(TValue).IsEnum)
            {
                var name = property.GetName();
                var element = new EnumField(value as Enum);
                GuiConstructFactory.SetNames(property, element);
                if (context.GetParent(out var parent))
                {
                    parent.contentContainer.Add(element);
                }
                GuiConstructFactory.SetDataUpdater<TProperty, TContainer, TValue>(property, element, context, name);

                return element;
            }

            return null;
        }

//        public static ObjectField EntityReferenceField<TContainer>(ref TContainer container, ref UIVisitContext<TinyEntity.Reference> context, IRegistry registry)
//            where TContainer : IPropertyContainer
//        {
//            var field = GuiConstructFactory.Construct<ObjectField, Object, TContainer, TinyEntity.Reference>(
//                ref container,
//                ref context,
//                new Translator<Object, TinyEntity.Reference>()
//                {
//                    ToValue = value => ((TinyEntityView)value)?.EntityRef ?? TinyEntity.Reference.None,
//                    ToField = value => value.Dereference(registry)?.View
//                });
//
//            field.objectType = typeof(TinyEntityView);
//            return field;
//        }

//        public static ObjectField UnityObjectField<TProperty, TContainer, TObject>(TProperty property, ref TContainer container,
//            ref TObject value, InspectorAdapterContext context)
//            where TProperty : IProperty<TContainer, TObject>
//
//            where TObject : Object
//        {
//            var field = GuiConstructFactory.Construct<TProperty, ObjectField, Object, TContainer, TObject>(
//                property,
//                ref container,
//                ref value,
//                context,
//                new Translator<Object, TObject>()
//                {
//                    ToValue = v => (TObject) v,
//                    ToField = Translators.Identity
//                });
//
//            field.objectType = typeof(TObject);
//            return field;
//        }
    }
}
