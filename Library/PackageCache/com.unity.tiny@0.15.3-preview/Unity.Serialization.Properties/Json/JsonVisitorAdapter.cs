using System;
using System.Text;
using Unity.Properties;

namespace Unity.Serialization.Json
{
    public abstract class JsonVisitorAdapter : IPropertyVisitorAdapter
    {
        private readonly JsonVisitor m_Visitor;

        protected JsonVisitorAdapter(JsonVisitor visitor)
        {
            m_Visitor = visitor;
        }

        protected void Append<TProperty, TValue>(TProperty property, TValue value, Action<StringBuilder, TValue> write)
            where TProperty : IProperty
        {
            if (property is ICollectionElementProperty)
            {
                m_Visitor.Builder.Append(' ', JsonVisitor.Style.Space * m_Visitor.Indent);
                write(m_Visitor.Builder, value);
                m_Visitor.Builder.Append(",\n");
            }
            else
            {
                m_Visitor.Builder.Append(' ', JsonVisitor.Style.Space * m_Visitor.Indent);
                m_Visitor.Builder.Append("\"");
                m_Visitor.Builder.Append(property.GetName());
                m_Visitor.Builder.Append("\": ");
                write(m_Visitor.Builder, value);
                m_Visitor.Builder.Append(",\n");
            }
        }

        private static readonly StringBuilder s_Builder = new StringBuilder(64);

        protected static string EncodeJsonString(string s)
        {
            if (s == null)
            {
                return "null";
            }

            var b = s_Builder;
            b.Clear();
            b.Append("\"");

            foreach (var c in s)
            {
                switch (c)
                {
                    case '\\':
                        b.Append("\\\\");
                        break; // @TODO Unicode look-ahead \u1234
                    case '\"':
                        b.Append("\\\"");
                        break;
                    case '\t':
                        b.Append("\\t");
                        break;
                    case '\r':
                        b.Append("\\r");
                        break;
                    case '\n':
                        b.Append("\\n");
                        break;
                    case '\b':
                        b.Append("\\b");
                        break;
                    default:
                        b.Append(c);
                        break;
                }
            }

            b.Append("\"");
            return s_Builder.ToString();
        }
    }
}
