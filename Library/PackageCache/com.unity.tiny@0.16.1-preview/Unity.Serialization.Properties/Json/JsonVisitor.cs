using System.Text;
using Unity.Properties;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// A visitor that traverses a property container and outputs a JSON string.
    ///
    /// You can extend or adapt this class to handle custom types.
    /// </summary>
    public class JsonVisitor : PropertyVisitor
    {
        public static class Style
        {
            public const string TypeInfoKey = "$type";
            public const int Space = 4;
        }

        public StringBuilder Builder { get; } = new StringBuilder(1024);

        public int Indent { get; set; }

        public JsonVisitor()
        {
            AddAdapter(new JsonPrimitiveAdapter(this));
        }

        protected override VisitStatus BeginContainer<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
        {
            if (property is ICollectionElementProperty)
            {
                Indent--;
                Builder.Length -= 1;
                Builder.Append(Builder[Builder.Length - 1] == ',' ? " {\n" : "{\n");
            }
            else
            {
                Builder.Append(' ', Style.Space * Indent);
                Builder.Append("\"");
                Builder.Append(property.GetName());
                Builder.Append("\": {\n");
            }

            Indent++;

            var typeInfo = GetTypeInfo<TProperty, TContainer, TValue>();

            if (null != typeInfo)
            {
                Builder.Append(' ', Style.Space * Indent);
                Builder.Append($"\"{Style.TypeInfoKey}\": \"");
                Builder.Append(typeInfo);
                Builder.Append("\",\n");
            }

            return VisitStatus.Handled;
        }

        protected override void EndContainer<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
        {
            Indent--;

            if (Builder[Builder.Length - 2] == ',')
            {
                Builder.Length -= 2;
                Builder.Append('\n');
                Builder.Append(' ', Style.Space * Indent);
            }
            else
            {
                Builder.Length -= 1;
            }

            if (property is ICollectionElementProperty)
            {
                Indent++;
            }

            Builder.Append("},\n");
        }

        protected override VisitStatus BeginCollection<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
        {
            Builder.Append(' ', Style.Space * Indent);
            Builder.Append('\"');
            Builder.Append(property.GetName());
            Builder.Append("\": [\n");
            Indent++;
            return VisitStatus.Handled;
        }

        protected override void EndCollection<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
        {
            Indent--;

            if (Builder[Builder.Length - 2] == ',')
            {
                Builder.Length -= 2;
            }
            else
            {
                Builder.Length -= 1;
            }

            var skipNewline = Builder[Builder.Length - 1] == '}' &&
                              Builder[Builder.Length - 3] == ' ';

            skipNewline = skipNewline | Builder[Builder.Length - 1] == '[';

            if (!skipNewline)
            {
                Builder.Append("\n");
                Builder.Append(' ', Style.Space * Indent);
            }

            Builder.Append("],\n");
        }

        /// <summary>
        /// Override this method to provide your own `TypeInfo` string.
        ///
        /// This can be used during deserialization to reconstruct the actual type.
        /// </summary>
        protected virtual string GetTypeInfo<TProperty, TContainer, TValue>()
            where TProperty : IProperty<TContainer, TValue>
        {
            return null;
        }
    }
}
