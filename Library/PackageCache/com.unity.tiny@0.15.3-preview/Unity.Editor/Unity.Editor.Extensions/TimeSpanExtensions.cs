using System;
using System.Globalization;

namespace Unity.Editor.Extensions
{
    internal static class TimeSpanExtensions
    {
        public static string ToShortString(this TimeSpan timeSpan, uint decimals = 2)
        {
            if (timeSpan.TotalSeconds < 1.0)
            {
                return timeSpan.Milliseconds.ToString(CultureInfo.InvariantCulture) + "ms";
            }
            else if (timeSpan.TotalMinutes < 1.0)
            {
                return timeSpan.TotalSeconds.ToString("F" + decimals, CultureInfo.InvariantCulture) + "s";
            }
            else if (timeSpan.TotalHours < 1.0)
            {
                return timeSpan.TotalMinutes.ToString("F" + decimals, CultureInfo.InvariantCulture) + "m";
            }
            else if (timeSpan.TotalDays < 1.0)
            {
                return timeSpan.TotalHours.ToString("F" + decimals, CultureInfo.InvariantCulture) + "h";
            }
            else
            {
                return timeSpan.TotalDays.ToString("F" + decimals, CultureInfo.InvariantCulture) + "d";
            }
        }
    }
}
