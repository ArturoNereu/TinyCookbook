using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Unity.Tiny.Debugging
{
    public class StringRingHistory
    {
        public StringRingHistory(int count)
        {
            data = new string[count];
            nextIndex = 0;
        }

        public bool Contains(string s)
        {
            foreach(var d in data) {
                // Temporary fix by validating d != null to bypass IL2CPP bug where string equality does not handle null strings
                if (d != null && d == s)
                    return true;
            }
            return false;
        }

        public void Add(string s) 
        {
            data[nextIndex] = s;
            nextIndex++;
            if (nextIndex == data.Length)
                nextIndex = 0;
        }

        int nextIndex;
        string[] data;
    }

    public static class Debug
    {
        [Conditional("DEBUG")]
        public static void Log(string log)
        {
            if (!history.Contains(log)) {
                Console.WriteLine(log);
                history.Add(log);
            }
        }

        [Conditional("DEBUG")]
        // bypass history de-duplication
        public static void LogAlways(string log)
        {
            Console.WriteLine(log);
        }

        [Conditional("DEBUG")]
        public static void LogFormat(string format, params object[] args)
        {
            var result = StringFormatter.Format(format, args);
            Log(result);
        }

        [Conditional("DEBUG")]
        // bypass history de-duplication
        public static void LogFormatAlways(string format, params object[] args)
        {
            var result = StringFormatter.Format(format, args);
            LogAlways(result);
        }

        static StringRingHistory history = new StringRingHistory(32); // keep last n-strings 
    }
}
