using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Color = Unity.Tiny.Core2D.Color;
using Rect = Unity.Tiny.Core2D.Rect;

namespace Unity.Editor
{
    internal static class Translators
    {
        public static TValue Identity<TValue>(TValue value) => value;
        public static TValue Cast<TBase, TValue>(TBase value) where TValue : TBase => (TValue)value;
        public static TValue UnsafeCast<TBase, TValue>(TBase value) => (TValue)(object)value;
        public static TValue NoOp<TBase, TValue>(TBase value) => default;
    }

    public struct Translator<TFieldValue, TValue>
    {
        public delegate TValue ToValueDelegate(TFieldValue fieldValue);
        public delegate TFieldValue ToFieldDelegate(TValue value);

        public ToValueDelegate ToValue;
        public ToFieldDelegate ToField;
    }

    public static class TranslatorFactory<TValueType>
        where TValueType : struct
    {
        private static readonly List<IOffsetDataUpdater> m_AvailableTranslators = new List<IOffsetDataUpdater>();

        public interface IOffsetDataUpdater
        {
            void SetUpdaters<T>(VisualElement element, T dataProvider, int offset, int index)
                where T : IOffsetDataProvider;
            bool MatchesUpdaterType(VisualElement element);
        }

        private class OffsetDataUpdater<TElement, TFieldType> : IOffsetDataUpdater
            where TElement : BaseField<TFieldType>, INotifyValueChanged<TFieldType>
        {
            private readonly Translator<TFieldType, TValueType> m_Translator;

            public OffsetDataUpdater(Translator<TFieldType, TValueType> translator)
            {
                m_Translator = translator;
            }

            public void SetUpdaters<T>(VisualElement element, T dataProvider, int offset, int index)
                where T : IOffsetDataProvider
            {
                if (element is TElement baseField)
                {
                    if (null == dataProvider)
                    {
                        element.SetEnabled(false);
                        return;
                    }

                    if (m_Translator.ToValue == Translators.NoOp<TFieldType, TValueType>)
                    {
                        element.SetEnabled(false);
                    }
                    baseField.RegisterValueChangedCallback(evt =>
                    {
                        var newValue = m_Translator.ToValue(evt.newValue);
                        if (index < 0)
                        {
                            dataProvider.SetDataAtOffset(newValue, offset);
                        }
                        else
                        {
                            dataProvider.SetDataAtOffset(newValue, index, offset);
                        }

                        baseField.SetValueWithoutNotify(m_Translator.ToField(newValue));
                    });

                    if (index < 0)
                    {
                        if (dataProvider is IComponentDataElement e)
                        {
                            e.RegisterUpdater(new GuiUpdaters.GenericFieldUpdater<TFieldType, TValueType>(
                                dataProvider,
                                offset,
                                baseField,
                                m_Translator));
                        }
                    }
                    else
                    {
                        if (dataProvider is IComponentDataElement e)
                        {
                            e.RegisterUpdater(new GuiUpdaters.GenericBufferFieldUpdater<TFieldType, TValueType>(
                                dataProvider,
                                index,
                                offset,
                                baseField,
                                m_Translator));
                        }
                    }

                    if (index < 0)
                    {
                        baseField.value = m_Translator.ToField(dataProvider.GetDataAtOffset<TValueType>(offset));
                    }
                    else
                    {
                        baseField.value = m_Translator.ToField(dataProvider.GetDataAtOffset<TValueType>(index, offset));
                    }
                }
                else
                {
                    Debug.Log("Invalid data conversion.");
                }
            }

            public bool MatchesUpdaterType(VisualElement element)
            {
                return element is TElement;
            }
        }


        public static void Register<TElement, TFieldType>(Translator<TFieldType, TValueType> translator)
            where TElement : BaseField<TFieldType>, INotifyValueChanged<TFieldType>
        {
            m_AvailableTranslators.Add(new OffsetDataUpdater<TElement, TFieldType>(translator));
        }

        public static bool TryGetUpdater(VisualElement element, out IOffsetDataUpdater translator)
        {
            foreach (var t in m_AvailableTranslators)
            {
                if (!t.MatchesUpdaterType(element))
                {
                    continue;
                }
                translator = t;
                return true;
            }

            translator = null;
            return false;
        }
    }

    [InitializeOnLoad]
    public static class TranslatorFactory
    {
        static TranslatorFactory()
        {
            TranslatorFactory<bool>.Register<Toggle, bool>(new Translator<bool, bool>
            {
                ToValue = Translators.Identity,
                ToField = Translators.Identity
            });

            TranslatorFactory<sbyte>.Register<IntegerField, int>(new Translator<int, sbyte>()
            {
                ToValue = v => (sbyte) Mathf.Clamp(v, sbyte.MinValue, sbyte.MaxValue),
                ToField = v => (int) v
            });

            TranslatorFactory<byte>.Register<IntegerField, int>(new Translator<int, byte>()
            {
                ToValue = v => (byte) Mathf.Clamp(v, byte.MinValue, byte.MaxValue),
                ToField = v => (int) v
            });

            TranslatorFactory<ushort>.Register<IntegerField, int>(new Translator<int, ushort>()
            {
                ToValue = v => (ushort) Mathf.Clamp(v, ushort.MinValue, ushort.MaxValue),
                ToField = v => (int) v
            });

            TranslatorFactory<short>.Register<IntegerField, int>(new Translator<int, short>()
            {
                ToValue = v => (short) Mathf.Clamp(v, short.MinValue, short.MaxValue),
                ToField = v => (int) v
            });

            TranslatorFactory<int>.Register<IntegerField, int>(new Translator<int, int>()
            {
                ToValue = Translators.Identity,
                ToField = Translators.Identity
            });

            TranslatorFactory<int>.Register<SliderInt, int>(new Translator<int, int>()
            {
                ToValue = Translators.Identity,
                ToField = Translators.Identity
            });

            TranslatorFactory<uint>.Register<LongField, long>(new Translator<long, uint>()
            {
                ToValue = v => (uint) Mathf.Clamp(v, uint.MinValue, uint.MaxValue),
                ToField = v => (long)v
            });

            TranslatorFactory<long>.Register<LongField, long>(new Translator<long, long>()
            {
                ToValue = Translators.Identity,
                ToField = Translators.Identity
            });

            TranslatorFactory<ulong>.Register<TextField, string>(new Translator<string, ulong>()
            {
                ToValue = v =>
                {
                    ulong.TryParse(v, out var num);
                    return num;
                },
                ToField = v => v.ToString()
            });

            TranslatorFactory<float>.Register<FloatField, float>(new Translator<float, float>()
            {
                ToValue = Translators.Identity,
                ToField = Translators.Identity
            });

            TranslatorFactory<float>.Register<Slider, float>(new Translator<float, float>()
            {
                ToValue = Translators.Identity,
                ToField = Translators.Identity
            });

            TranslatorFactory<double>.Register<DoubleField, double>(new Translator<double, double>()
            {
                ToValue = Translators.Identity,
                ToField = Translators.Identity
            });

            TranslatorFactory<char>.Register<TextField, string>(new Translator<string, char>()
            {
                ToValue = v =>
                {
                    if (string.IsNullOrEmpty(v))
                    {
                        return '\0';
                    }
                    return v[0];
                },
                ToField = v => v.ToString()
            });


            // TODO: Add more registered types
            TranslatorFactory<Rect>.Register<RectField, UnityEngine.Rect>(new Translator<UnityEngine.Rect, Rect>()
            {
                ToValue = v => new Rect(v.x, v.y, v.width, v.height),
                ToField = v => new UnityEngine.Rect(v.x, v.y, v.width, v.height),
            });

            TranslatorFactory<Color>.Register<ColorField, UnityEngine.Color>(new Translator<UnityEngine.Color, Color>()
            {
                ToValue = v => new Color(v.r, v.g, v.b, v.a),
                ToField = v => new UnityEngine.Color(v.r, v.g, v.b, v.a),
            });


            TranslatorFactory<float3>.Register<Vector3Field, Vector3>(new Translator<Vector3, float3>()
            {
                ToValue = v => v,
                ToField = v => v
            });
        }
    }
}
