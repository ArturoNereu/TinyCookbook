using System;

namespace Unity.Tiny.Debugging
{
    public static class NumberConverter
    {
        public static string FloatToString(float value, uint precision = 3)
        {
            return DoubleToString(value, precision);
        }

        public static string DoubleToString(double value, uint precision = 3)
        {
            int dec = (int)value;
            string str = dec.ToString();

            if (precision == 0)
                return str;

            if (precision > 9)
                precision = 9;

            value -= dec;
            value = value < 0.0f ? -value : value;
            value *= Math.Pow(10, precision);
            str += "." + ((int)value).ToString();

            return str;
        }
    }
}
