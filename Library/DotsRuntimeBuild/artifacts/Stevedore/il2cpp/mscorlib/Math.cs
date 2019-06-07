// Decompiled with JetBrains decompiler
// Type: System.Math
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: 016F22B5-9BD6-4951-B06E-38B9F5A0506C
// Assembly location: /Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5/mscorlib.dll

using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace System
{
    public static class Math
    {
        //private static double doubleRoundLimit = 1E+16;
        private static double[] roundPower10Double = new double[16]
        {
            1.0,
            10.0,
            100.0,
            1000.0,
            10000.0,
            100000.0,
            1000000.0,
            10000000.0,
            100000000.0,
            1000000000.0,
            10000000000.0,
            100000000000.0,
            1000000000000.0,
            10000000000000.0,
            100000000000000.0,
            1E+15
        };
        private const int maxRoundingDigits = 15;
        public const double PI = 3.14159265358979;
        public const double E = 2.71828182845905;

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Acos(double d);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Asin(double d);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Atan(double d);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Atan2(double y, double x);
/*
    public static Decimal Ceiling(Decimal d)
    {
      return Decimal.Ceiling(d);
    }
*/
        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Ceiling(double a);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Cos(double d);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Cosh(double value);
/*
    public static Decimal Floor(Decimal d)
    {
      return Decimal.Floor(d);
    }
*/
        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Floor(double d);

        /*
        [SecuritySafeCritical]
        private static unsafe double InternalRound(double value, int digits, MidpointRounding mode)
        {
          if (Math.Abs(value) < Math.doubleRoundLimit)
          {
            double num1 = Math.roundPower10Double[digits];
            value *= num1;
            if (mode == MidpointRounding.AwayFromZero)
            {
              double num2 = Math.SplitFractionDouble(&value);
              if (Math.Abs(num2) >= 0.5)
                value += (double) Math.Sign(num2);
            }
            else
              value = Math.Round(value);
            value /= num1;
          }
          return value;
        }
    */
        [SecuritySafeCritical]
        private static unsafe double InternalTruncate(double d)
        {
            Math.SplitFractionDouble(&d);
            return d;
        }

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Sin(double a);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Tan(double a);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Sinh(double value);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Tanh(double value);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Round(double a);

        /*
        public static double Round(double value, int digits)
        {
          if (digits < 0 || digits > 15)
            throw new ArgumentOutOfRangeException(nameof (digits), Environment.GetResourceString("Rounding digits must be between 0 and 15, inclusive."));
          return Math.InternalRound(value, digits, MidpointRounding.ToEven);
        }

        public static double Round(double value, MidpointRounding mode)
        {
          return Math.Round(value, 0, mode);
        }

        public static double Round(double value, int digits, MidpointRounding mode)
        {
          if (digits < 0 || digits > 15)
            throw new ArgumentOutOfRangeException(nameof (digits), Environment.GetResourceString("Rounding digits must be between 0 and 15, inclusive."));
          switch (mode)
          {
            case MidpointRounding.ToEven:
            case MidpointRounding.AwayFromZero:
              return Math.InternalRound(value, digits, mode);
            default:
              throw new ArgumentException(Environment.GetResourceString("The value '{0}' is not valid for this usage of the type {1}.", (object) mode, (object) "MidpointRounding"), nameof (mode));
          }
        }

        public static Decimal Round(Decimal d)
        {
          return Decimal.Round(d, 0);
        }

        public static Decimal Round(Decimal d, int decimals)
        {
          return Decimal.Round(d, decimals);
        }

        public static Decimal Round(Decimal d, MidpointRounding mode)
        {
          return Decimal.Round(d, 0, mode);
        }

        public static Decimal Round(Decimal d, int decimals, MidpointRounding mode)
        {
          return Decimal.Round(d, decimals, mode);
        }
    */
        [SecurityCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern unsafe double SplitFractionDouble(double* value);
/*
    public static Decimal Truncate(Decimal d)
    {
      return Decimal.Truncate(d);
    }
*/

        public static double Truncate(double d)
        {
            return Math.InternalTruncate(d);
        }

        [SecuritySafeCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Sqrt(double d);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Log(double d);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Log10(double d);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Exp(double d);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Pow(double x, double y);

        /*
        public static double IEEERemainder(double x, double y)
        {
          if (double.IsNaN(x))
            return x;
          if (double.IsNaN(y))
            return y;
          double d = x % y;
          if (double.IsNaN(d))
            return double.NaN;
          if (d == 0.0 && double.IsNegative(x))
            return double.NegativeZero;
          double num = d - Math.Abs(y) * (double) Math.Sign(x);
          if (Math.Abs(num) == Math.Abs(d))
          {
            double a = x / y;
            if (Math.Abs(Math.Round(a)) > Math.Abs(a))
              return num;
            return d;
          }
          if (Math.Abs(num) < Math.Abs(d))
            return num;
          return d;
        }

        [CLSCompliant(false)]
        public static sbyte Abs(sbyte value)
        {
          if (value >= (sbyte) 0)
            return value;
          return Math.AbsHelper(value);
        }

        private static sbyte AbsHelper(sbyte value)
        {
          if (value == sbyte.MinValue)
            throw new OverflowException(Environment.GetResourceString("Negating the minimum value of a twos complement number is invalid."));
          return -value;
        }

        public static short Abs(short value)
        {
          if (value >= (short) 0)
            return value;
          return Math.AbsHelper(value);
        }

        private static short AbsHelper(short value)
        {
          if (value == short.MinValue)
            throw new OverflowException(Environment.GetResourceString("Negating the minimum value of a twos complement number is invalid."));
          return -value;
        }

        public static int Abs(int value)
        {
          if (value >= 0)
            return value;
          return Math.AbsHelper(value);
        }

        private static int AbsHelper(int value)
        {
          if (value == int.MinValue)
            throw new OverflowException(Environment.GetResourceString("Negating the minimum value of a twos complement number is invalid."));
          return -value;
        }

        public static long Abs(long value)
        {
          if (value >= 0L)
            return value;
          return Math.AbsHelper(value);
        }

        private static long AbsHelper(long value)
        {
          if (value == long.MinValue)
            throw new OverflowException(Environment.GetResourceString("Negating the minimum value of a twos complement number is invalid."));
          return -value;
        }

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern float Abs(float value);

        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Abs(double value);

        public static Decimal Abs(Decimal value)
        {
          return Decimal.Abs(value);
        }*/

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [CLSCompliant(false)]
        public static sbyte Max(sbyte val1, sbyte val2)
        {
            if ((int)val1 < (int)val2)
                return val2;
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static byte Max(byte val1, byte val2)
        {
            if ((int)val1 < (int)val2)
                return val2;
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static short Max(short val1, short val2)
        {
            if ((int)val1 < (int)val2)
                return val2;
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [CLSCompliant(false)]
        public static ushort Max(ushort val1, ushort val2)
        {
            if ((int)val1 < (int)val2)
                return val2;
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static int Max(int val1, int val2)
        {
            if (val1 < val2)
                return val2;
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [CLSCompliant(false)]
        public static uint Max(uint val1, uint val2)
        {
            if (val1 < val2)
                return val2;
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static long Max(long val1, long val2)
        {
            if (val1 < val2)
                return val2;
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [CLSCompliant(false)]
        public static ulong Max(ulong val1, ulong val2)
        {
            if (val1 < val2)
                return val2;
            return val1;
        }

/*
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    public static float Max(float val1, float val2)
    {
      if ((double) val1 > (double) val2 || float.IsNaN(val1))
        return val1;
      return val2;
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    public static double Max(double val1, double val2)
    {
      if (val1 > val2 || double.IsNaN(val1))
        return val1;
      return val2;
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    public static Decimal Max(Decimal val1, Decimal val2)
    {
      return Decimal.Max(val1, val2);
    }
*/
        [CLSCompliant(false)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static sbyte Min(sbyte val1, sbyte val2)
        {
            if ((int)val1 > (int)val2)
                return val2;
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static byte Min(byte val1, byte val2)
        {
            if ((int)val1 > (int)val2)
                return val2;
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static short Min(short val1, short val2)
        {
            if ((int)val1 > (int)val2)
                return val2;
            return val1;
        }

        [CLSCompliant(false)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static ushort Min(ushort val1, ushort val2)
        {
            if ((int)val1 > (int)val2)
                return val2;
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static int Min(int val1, int val2)
        {
            if (val1 > val2)
                return val2;
            return val1;
        }

        [CLSCompliant(false)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static uint Min(uint val1, uint val2)
        {
            if (val1 > val2)
                return val2;
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static long Min(long val1, long val2)
        {
            if (val1 > val2)
                return val2;
            return val1;
        }

        [CLSCompliant(false)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static ulong Min(ulong val1, ulong val2)
        {
            if (val1 > val2)
                return val2;
            return val1;
        }

/*
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    public static float Min(float val1, float val2)
    {
      if ((double) val1 < (double) val2 || float.IsNaN(val1))
        return val1;
      return val2;
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    public static double Min(double val1, double val2)
    {
      if (val1 < val2 || double.IsNaN(val1))
        return val1;
      return val2;
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    public static Decimal Min(Decimal val1, Decimal val2)
    {
      return Decimal.Min(val1, val2);
    }
*/
        public static double Log(double a, double newBase)
        {
            if (double.IsNaN(a))
                return a;
            if (double.IsNaN(newBase))
                return newBase;
            if (newBase == 1.0 || a != 1.0 && (newBase == 0.0 || double.IsPositiveInfinity(newBase)))
                return double.NaN;
            return Math.Log(a) / Math.Log(newBase);
        }

        [CLSCompliant(false)]
        public static int Sign(sbyte value)
        {
            if (value < (sbyte)0)
                return -1;
            return value > (sbyte)0 ? 1 : 0;
        }

        public static int Sign(short value)
        {
            if (value < (short)0)
                return -1;
            return value > (short)0 ? 1 : 0;
        }

        public static int Sign(int value)
        {
            if (value < 0)
                return -1;
            return value > 0 ? 1 : 0;
        }

        public static int Sign(long value)
        {
            if (value < 0L)
                return -1;
            return value > 0L ? 1 : 0;
        }

        public static int Sign(float value)
        {
            if ((double)value < 0.0)
                return -1;
            if ((double)value > 0.0)
                return 1;
            if ((double)value == 0.0)
                return 0;
            throw new ArithmeticException("Function does not accept floating point Not-a-Number values.");
        }

        public static int Sign(double value)
        {
            if (value < 0.0)
                return -1;
            if (value > 0.0)
                return 1;
            if (value == 0.0)
                return 0;
            throw new ArithmeticException("Function does not accept floating point Not-a-Number values.");
        }

/*
    public static int Sign(Decimal value)
    {
      if (value < Decimal.Zero)
        return -1;
      return value > Decimal.Zero ? 1 : 0;
    }
*/
        public static long BigMul(int a, int b)
        {
            return (long)a * (long)b;
        }

        public static int DivRem(int a, int b, out int result)
        {
            result = a % b;
            return a / b;
        }

        public static long DivRem(long a, long b, out long result)
        {
            result = a % b;
            return a / b;
        }
    }
}
