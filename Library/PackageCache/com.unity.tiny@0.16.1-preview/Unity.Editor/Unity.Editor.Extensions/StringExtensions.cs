using System.IO;
using System.Text.RegularExpressions;

namespace Unity.Editor.Extensions
{
    internal static class StringExtensions
    {
        private static Regex s_ToWordRegex = new Regex(@"[^\w]", RegexOptions.Compiled);

        public static string SingleQuoted(this string value)
        {
            return $"'{value.Trim('\'')}'";
        }

        public static string DoubleQuoted(this string value)
        {
            return $"\"{value.Trim('\"')}\"";
        }

        public static string HyperLink(this string value)
        {
            return $"<a href={value}>{value}</a>";
        }

        public static string ToIdentifier(this string value)
        {
            return s_ToWordRegex.Replace(value, "_");
        }

        public static string ToForwardSlash(this string value)
        {
            return value.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
