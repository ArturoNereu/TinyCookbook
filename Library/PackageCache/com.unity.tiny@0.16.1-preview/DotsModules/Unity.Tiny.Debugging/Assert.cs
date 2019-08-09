using System;
using System.Diagnostics;

namespace Unity.Tiny.Debugging
{
    public static class Assert
    {
        [Conditional("DEBUG")]
        public static void IsTrue(bool condition)
        {
            if (condition)
                return;

            throw new InvalidOperationException();
        }

        [Conditional("DEBUG")]
        public static void IsTrue(bool condition, string message)
        {
            if (condition)
                return;

            throw new InvalidOperationException(message);
        }

        [Conditional("DEBUG")]
        public static void IsTrue(bool condition, string formatString, params object[] args)
        {
            if (condition)
                return;

            var message = StringFormatter.Format(formatString, args);
            throw new InvalidOperationException(message);
        }
    }
}
