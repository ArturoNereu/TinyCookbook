using System;
using System.IO;
using System.Text;
using Unity.Properties;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// Helper class that generically writes any property container as a JSON string.
    ///
    /// @NOTE This class makes heavy use of `StringBuilder` and `.ToString` on primitives, which allocates large amounts of memory. Use it sparingly.
    ///
    /// @TODO
    ///     - Optimization.
    /// </summary>
    public static class JsonSerialization
    {
        private static readonly JsonVisitor s_DefaultVisitor = new JsonVisitor();

        /// <summary>
        /// Writes a property container to a file path.
        /// </summary>
        public static void Serialize<TContainer>(string path, TContainer target)
        {
            File.WriteAllText(path, Serialize(target));
        }

        /// <summary>
        /// Deserializes a file and returns a new instance of the container.
        /// </summary>
        public static TContainer Deserialize<TContainer>(string path)
            where TContainer : new() => Deserialize<TContainer>(new SerializedObjectReader(path));

        /// <summary>
        /// Deserializes a JSON string and returns a new instance of the container.
        /// </summary>
        public static TContainer DeserializeFromString<TContainer>(string jsonString)
            where TContainer : new()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return Deserialize<TContainer>(new SerializedObjectReader(ms));
            }
        }

        private static TContainer Deserialize<TContainer>(SerializedObjectReader reader)
            where TContainer : new()
        {
            using (reader)
            {
                var target = new TContainer();
                PropertyContainer.Transfer(target, reader.ReadObject());
                return target;
            }
        }

        /// <summary>
        /// Writes a property container to a JSON string.
        /// </summary>
        /// <returns></returns>
        public static string Serialize<TContainer>(TContainer container, JsonVisitor visitor = null)
        {
            if (null == visitor)
            {
                visitor = s_DefaultVisitor;
            }

            visitor.Builder.Clear();

            WritePrefix(visitor);
            PropertyContainer.Visit(container, visitor);
            WriteSuffix(visitor);

            return visitor.Builder.ToString();
        }

        private static void WritePrefix(JsonVisitor visitor)
        {
            visitor.Builder.Append(' ', JsonVisitor.Style.Space * visitor.Indent);
            visitor.Builder.Append("{\n");
            visitor.Indent++;
        }

        private static void WriteSuffix(JsonVisitor visitor)
        {
            visitor.Indent--;

            if (visitor.Builder[visitor.Builder.Length - 2] == '{')
            {
                visitor.Builder.Length -= 1;
            }
            else
            {
                visitor.Builder.Length -= 2;
            }

            visitor.Builder.Append("\n");
            visitor.Builder.Append(' ', JsonVisitor.Style.Space * visitor.Indent);
            visitor.Builder.Append("}");
        }
    }
}
