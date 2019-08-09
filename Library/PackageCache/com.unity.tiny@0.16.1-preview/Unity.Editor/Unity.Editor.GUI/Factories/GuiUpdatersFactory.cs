using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Editor
{
    internal static class GuiUpdaters
    {
        public readonly struct GenericFieldUpdater<TFieldType, TValue> : IDataUpdater
            where TValue : struct
        {
            private readonly int m_Offset;
            private readonly IOffsetDataProvider provider;
            private readonly BaseField<TFieldType> m_Element;
            private readonly Translator<TFieldType, TValue> m_Translator;

            public GenericFieldUpdater(
                IOffsetDataProvider provider,
                int offset,
                BaseField<TFieldType> element,
                Translator<TFieldType, TValue> translator)
            {
                m_Offset = offset;
                this.provider = provider;
                m_Element = element;
                m_Translator = translator;
            }

            public void Update()
            {
                var value = provider.GetDataAtOffset<TValue>(m_Offset);
                m_Element.SetValueWithoutNotify(m_Translator.ToField(value));
            }
        }

        public readonly struct GenericBufferFieldUpdater<TFieldType, TValue> : IDataUpdater
            where TValue : struct
        {
            private readonly int m_Index;
            private readonly int m_Offset;
            private readonly IOffsetDataProvider provider;
            private readonly BaseField<TFieldType> m_Element;
            private readonly Translator<TFieldType, TValue> m_Translator;

            public GenericBufferFieldUpdater(
                IOffsetDataProvider provider,
                int index,
                int offset,
                BaseField<TFieldType> element,
                Translator<TFieldType, TValue> translator)
            {
                m_Index = index;
                m_Offset = offset;
                this.provider = provider;
                m_Element = element;
                m_Translator = translator;
            }

            public void Update()
            {
                var value = provider.GetDataAtOffset<TValue>(m_Index, m_Offset);
                m_Element.SetValueWithoutNotify(m_Translator.ToField(value));
            }
        }

        public readonly struct EnumFieldUpdater<TEnumType, TValue> : IDataUpdater
            where TValue : struct
        {
            private readonly int m_Offset;
            private readonly IOffsetDataProvider provider;
            private readonly EnumField m_Element;
            private readonly Translator<Enum, TValue> m_Translator;

            public EnumFieldUpdater(
                IOffsetDataProvider provider,
                int offset,
                EnumField element,
                Translator<Enum, TValue> translator)
            {
                m_Offset = offset;
                this.provider = provider;
                m_Element = element;
                m_Translator = translator;
            }

            public void Update()
            {
                var value = provider.GetDataAtOffset<TValue>(m_Offset);
                m_Element.SetValueWithoutNotify((Enum)Enum.ToObject(typeof(TEnumType) , m_Translator.ToField(value)));
            }
        }
    }
}
