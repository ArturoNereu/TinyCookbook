using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

[assembly: CLSCompliant(false)]

#pragma warning disable 0169, 0649

class PreserveAttribute : System.Attribute
{
}

namespace System.Collections
{
    public interface IEnumerable
    {
        IEnumerator GetEnumerator();
    }

    public interface IEnumerator
    {
        bool MoveNext();
        object Current { get; }
        void Reset();
    }
}

namespace System.Collections.ObjectModel
{
    internal struct Dummy
    {
    }
}

namespace System.IO
{
    internal struct Dummy
    {
    }
}

namespace System.Collections.Generic
{
    public interface IEnumerable<T> : IEnumerable
    {
        new IEnumerator<T> GetEnumerator();
    }

    public interface ICollection<T> : IEnumerable<T>
    {
        int Count { get; }

        bool IsReadOnly { get; }

        void Add(T item);

        void Clear();

        bool Contains(T item);

        void CopyTo(T[] array, int arrayIndex);

        bool Remove(T item);
    }

    public interface IEnumerator<T> : IEnumerator, IDisposable
    {
        new T Current { get; }
    }

    public interface IEqualityComparer<in T>
    {
        bool Equals(T x, T y);
        int GetHashCode(T obj);
    }

    public interface IComparer<in T>
    {
        int Compare(T x, T y);
    }

    public abstract class EqualityComparer<T> : IEqualityComparer<T>
    {
        private static volatile EqualityComparer<T> defaultComparer;

        public static EqualityComparer<T> Default
        {
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            get
            {
                EqualityComparer<T> comparer = defaultComparer;
                if (comparer == null)
                {
                    comparer = CreateComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        private static EqualityComparer<T> CreateComparer()
        {
            return new BasicComparer<T>();
        }

        public abstract bool Equals(T x, T y);

        public abstract int GetHashCode(T obj);

        internal virtual int IndexOf(T[] array, T value, int startIndex, int count)
        {
            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (Equals(array[i], value)) return i;
            }
            return -1;
        }
    }

    internal class BasicComparer<T> : EqualityComparer<T>
    {
        public override bool Equals(T x, T y)
        {
            var tt = typeof(T);
            if (x != null)
            {
                if (y != null)
                {
                    if (!tt.IsValueType)
                    {
                        if (typeof(IEquatable<T>).IsAssignableFrom(tt))
                            return ((IEquatable<T>)x).Equals(y);
                        else
                            return x.Equals(y);
                    }
                    else
                    {
                        return RuntimeHelpers.MemCmpRef(ref x, ref y) == 0;
                    }
                }

                return false;
            }

            if (y != null)
                return false;
            return true;
        }

        public override int GetHashCode(T obj)
        {
            var tt = typeof(T);
            if (!tt.IsValueType)
            {
                if (typeof(IEquatable<T>).IsAssignableFrom(tt))
                    return ((IEquatable<T>)obj).GetHashCode();
                else
                    return obj.GetHashCode();
            }
            else
            {
                return RuntimeHelpers.MemHashRef(ref obj);
            }
        }
    }

    public class List<T> : IEnumerable<T>
    {
        private const int _defaultCapacity = 4;

        private T[] _items;
        private int _size;
        private int _version;

        public int Capacity
        {
            get
            {
                return _items.Length;
            }
            set
            {
                if (value < _size)
                    throw new InvalidOperationException();

                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        T[] newItems = new T[value];
                        if (_size > 0)
                        {
                            Array.Copy<T>(_items, 0, newItems, 0, _size);
                        }
                        _items = newItems;
                    }
                    else
                    {
                        _items = new T[0];
                    }
                }
            }
        }

        //static readonly T[] _emptyArray = new T[0];

        public List()
        {
            // we could use the _emptyArray optimization, but something seems broken with
            // the static field initialization
            _items = new T[0];
        }

        public List(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity < 0");
            _items = new T[capacity];
        }

        public void Add(T item)
        {
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            _items[_size++] = item;
            _version++;
        }

        private void EnsureCapacity(int min)
        {
            if (_items.Length < min)
            {
                int newCapacity = _items.Length == 0 ? _defaultCapacity : _items.Length * 2;
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }
        }

        public int Count
        {
            get { return _size; }
        }

        public bool Contains(T item)
        {
            if ((Object)item == null)
            {
                for (int i = 0; i < _size; i++)
                    if ((Object)_items[i] == null)
                        return true;
                return false;
            }

            for (int i = 0; i < _size; i++)
            {
                if (_items[i].Equals(item))
                    return true;
            }

            return false;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(_items, item, 0, _size);
        }

        public void Reverse()
        {
            Reverse(0, this.Count);
        }

        public void Reverse(int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("List.Reverse 'index' less than zero.");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("List.Reverse 'count' less than zero.");
            }

            if (_size - index < count)
                throw new ArgumentException("List.Reverse 'index' and 'count' do not define a valid range.");

            int low = index;
            int high = index + count - 1;
            while (low < high)
            {
                T temp = this[low];
                this[low] = this[high];
                this[high] = temp;

                low++;
                high--;
            }
        }

        public void Clear()
        {
            _items = new T[0];
            _size = 0;
            _version++;
        }

        public T this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                _items[index] = value;
                _version++;
            }
        }

        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
                throw new InvalidOperationException();

            _size--;
            if (index < _size)
            {
                Array.Copy(_items, index + 1, _items, index, _size - index);
            }
            _items[_size] = default(T);
            _version++;
        }

        public T[] ToArray()
        {
            T[] array = new T[_size];
            Array.Copy(_items, 0, array, 0, _size);
            return array;
        }

        public Enumerator GetEnumerator()
            => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => new Enumerator(this);

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly List<T> _list;
            private int _index;
            private readonly int _version;
            private T _current;

            internal Enumerator(List<T> list)
            {
                _list = list;
                _index = 0;
                _version = list._version;
                _current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                List<T> localList = _list;

                if (_version == localList._version && ((uint)_index < (uint)localList._size))
                {
                    _current = localList._items[_index];
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _list._version)
                {
                    throw new InvalidOperationException("Enumerator invalid");
                }

                _index = _list._size + 1;
                _current = default(T);
                return false;
            }

            public T Current => _current;

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _list._size + 1)
                    {
                        throw new InvalidOperationException("Enumerator invalid");
                    }

                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _list._version)
                {
                    throw new InvalidOperationException("Enumerator invalid");
                }

                _index = 0;
                _current = default(T);
            }
        }
    }
}

namespace System
{
    public interface IDisposable
    {
        void Dispose();
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Enum, Inherited = false)]
    public class FlagsAttribute : Attribute
    {
        public FlagsAttribute()
        {
        }
    }

    public abstract class Array
    {
        [Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public extern int GetLength(int dimension);

        public int Length
        {
            get
            {
                return GetLength(0);
            }
        }

        public static int IndexOf<T>(T[] array, T value)
        {
            return IndexOf(array, value, 0, array.Length);
        }

        public static int IndexOf<T>(T[] array, T value, int startIndex, int count)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            const int lb = 0;
            if (startIndex < lb || startIndex > array.Length + lb)
                throw new ArgumentOutOfRangeException("startIndex");
            if (count < 0 || count > array.Length - startIndex + lb)
                throw new ArgumentOutOfRangeException("count");

            int endIndex = array.Length;
            if (value == null)
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    if (array[i] == null) return i;
                }
            }
            else
            {
                var comparer = System.Collections.Generic.EqualityComparer<T>.Default;
                for (int i = startIndex; i < endIndex; i++)
                {
                    if (comparer.Equals(value, array[i]))
                        return i;
                }
            }

            return -1;
        }

        public static void Resize<T>(ref T[] array, int newSize)
        {
            var oldArray = array;
            var newArray = new T[newSize];

            var copySize = Math.Min(oldArray.Length, newSize);

            for (int i = 0; i < copySize; i++)
                newArray[i] = oldArray[i];

            array = newArray;
        }

        public static T[] Empty<T>()
        {
            return new T[] {};
        }

        internal static void Copy<T>(T[] sourceArray, T[] destinationArray, int length)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");

            Copy<T>(sourceArray, 0, destinationArray, 0, length);
        }

        internal static void Copy<T>(T[] sourceArray, int sourceIndex, T[] destinationArray, int destinationIndex,
            int length)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");

            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            if (sourceIndex < 0)
                throw new ArgumentOutOfRangeException("sourceIndex");

            if (destinationIndex < 0)
                throw new ArgumentOutOfRangeException("destinationIndex");

            int source_pos = sourceIndex;
            int dest_pos = destinationIndex;

            if (dest_pos < 0)
                throw new ArgumentOutOfRangeException("destinationIndex");

            // re-ordered to avoid possible integer overflow
            if (source_pos > sourceArray.Length - length)
                throw new ArgumentException("length");

            if (dest_pos > destinationArray.Length - length)
            {
                throw new ArgumentException(
                    "Destination array was not long enough. Check destIndex and length, and the array's lower bounds",
                    nameof(destinationArray));
            }

            if (!Object.ReferenceEquals(sourceArray, destinationArray) || source_pos > dest_pos)
            {
                for (int i = 0; i < length; i++)
                {
                    T src = sourceArray[source_pos + i];
                    destinationArray[dest_pos + i] = src;
                }
            }
            else
            {
                for (int i = length - 1; i >= 0; i--)
                {
                    T src = sourceArray[source_pos + i];
                    destinationArray[dest_pos + i] = src;
                }
            }
        }
    }

    public class Object
    {
        [Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public extern Type GetType();

        public static bool ReferenceEquals(object left, object right)
        {
            return left == right;
        }

        //todo: add compiler support for not ever allowing invocations to these virtual methods
        public virtual bool Equals(object obj)
        {
            return this == obj;
        }

        public virtual int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public virtual string ToString()
        {
            throw new NotImplementedException();
        }
    }

    public struct Char
    {
        public override string ToString()
        {
            return string.ToString(this);
        }

#if IL2CPP_MONO_DEBUGGER
        private char m_value;
#endif
    }

    public class String
    {
        private int _length;
        private char _firstChar;

        public static readonly string Empty = "";

        public static bool IsNullOrEmpty(string value)
        {
            return value == null || value.Length == 0;
        }

        public int Length
        {
            get
            {
                return _length;
            }
        }

        public unsafe String(char* value)
        {
            CreateString(value);
        }

        public String(char[] value)
        {
            CreateString(value);
        }

        public unsafe String(char* value, int startIndex, int length)
        {
            CreateString(value, startIndex, length);
        }

        public String(char[] value, int startIndex, int length)
        {
            CreateString(value, startIndex, length);
        }

        [Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        extern private static string CreateString(char[] value, int startIndex, int length);

        [System.Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        extern internal static string CreateString(int length);

        private static unsafe string CreateString(char* value, int startIndex, int length)
        {
            var result = CreateString(length);

            fixed(char* ptr = result)
            {
                Runtime.CompilerServices.RuntimeHelpers.MemoryCopy(ptr, value + startIndex, sizeof(char) * length);
            }

            return result;
        }

        private static unsafe string CreateString(char* value)
        {
            return CreateString(value, 0, GetLength(value));
        }

        private static unsafe string CreateString(char[] value)
        {
            return CreateString(value, 0, value.Length);
        }

        private static unsafe int GetLength(char* value)
        {
            char* ptr = value;
            while (*ptr != '\0')
            {
                ptr++;
            }
            return (int)(ptr - value);
        }

        public static string Concat(object o1, object o2) => "Concat not supported";

        public static unsafe string Concat(string left, string right)
        {
            if (IsNullOrEmpty(left))
            {
                if (IsNullOrEmpty(right))
                    return string.Empty;

                return right;
            }

            if (IsNullOrEmpty(right))
                return left;

            int leftLength = left.Length;
            int rightLength = right.Length;
            string result = CreateString(leftLength + rightLength);

            fixed(char* resultPtr = result)
            {
                fixed(char* leftPtr = left)
                {
                    Runtime.CompilerServices.RuntimeHelpers.MemoryCopy(resultPtr, leftPtr, sizeof(char) * leftLength);
                }

                fixed(char* rightPtr = right)
                {
                    Runtime.CompilerServices.RuntimeHelpers.MemoryCopy(resultPtr + leftLength, rightPtr, sizeof(char) * rightLength);
                }
            }

            return result;
        }

        public static unsafe string Concat(string str1, string str2, string str3)
        {
            if (IsNullOrEmpty(str1))
                return Concat(str2, str3);

            if (IsNullOrEmpty(str2))
                return Concat(str1, str3);

            if (IsNullOrEmpty(str3))
                return Concat(str1, str2);

            int str1Length = str1.Length;
            int str2Length = str2.Length;
            int str3Length = str3.Length;
            string result = CreateString(str1Length + str2Length + str3Length);

            fixed(char* resultPtrPinned = result)
            {
                char* resultPtr = resultPtrPinned;
                fixed(char* str1Ptr = str1)
                Runtime.CompilerServices.RuntimeHelpers.MemoryCopy(resultPtr, str1Ptr, sizeof(char) * str1Length);

                resultPtr += str1Length;

                fixed(char* str2Ptr = str2)
                Runtime.CompilerServices.RuntimeHelpers.MemoryCopy(resultPtr, str2Ptr, sizeof(char) * str2Length);

                resultPtr += str2Length;

                fixed(char* str3Ptr = str3)
                Runtime.CompilerServices.RuntimeHelpers.MemoryCopy(resultPtr, str3Ptr, sizeof(char) * str3Length);
            }

            return result;
        }

        public static string Concat(string str1, string str2, string str3, string str4) => string.Empty;

        public override bool Equals(Object obj)
        {
            return Equals(obj as String);
        }

        public static bool operator==(String a, String b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(String a, String b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            // From the MS / Mono source. Adapted to be pure C#.
            // https://referencesource.microsoft.com/#mscorlib/system/string.cs
            // The non-Win32 version (so this code doesn't take advantage of pointer overlay).
            int hash1 = 5381;
            int hash2 = hash1;

            int index = 0;
            int len = Length;
            while (index < len)
            {
                int c = this[index];
                hash1 = ((hash1 << 5) + hash1) ^ c;
                if (index + 1 == len)
                    break;
                c = this[index + 1];
                hash2 = ((hash2 << 5) + hash2) ^ c;
                index += 2;
            }
            return hash1 + (hash2 * 1566083941);
        }

        public unsafe bool Equals(String other)
        {
            if (Object.ReferenceEquals(this, other))
                return true;

            fixed(char* myPtr = this)
            {
                fixed(char* otherPtr = other)
                {
                    if (otherPtr == null)
                        return false;

                    var myLength = Length;
                    if (myLength != other.Length)
                        return false;

                    return Runtime.CompilerServices.RuntimeHelpers.MemoryCompare(myPtr, otherPtr, myLength * sizeof(char)) == 0;
                }
            }
        }

        [System.Runtime.CompilerServices.IndexerName("Chars")]
        public unsafe char this[int index]
        {
            get
            {
                if (index >= _length)
                    throw new Exception();

                fixed(char* characters = &_firstChar)
                return characters[index];
            }
        }

        unsafe internal static string ToString(char value)
        {
            var result = CreateString(1);

            fixed(char* c = result)
            c[0] = value;

            return result;
        }

        unsafe internal static string ToString(int value)
        {
            if (value >= 0)
                return ToString((uint)value);

            if (value == int.MinValue)
                return "-2147483648";

            int length = 1;
            value = -value;

            var tempValue = value;
            while (tempValue > 0)
            {
                tempValue /= 10;
                length++;
            }

            string result = CreateString(length);
            fixed(char* c = result)
            {
                c[0] = '-';
                for (int i = length - 1; i >= 1; i--)
                {
                    c[i] = (char)(value % 10 + '0');
                    value /= 10;
                }
            }

            return result;
        }

        unsafe internal static string ToString(uint value)
        {
            if (value == 0)
                return "0";

            int length = 0;
            var tempValue = value;
            while (tempValue > 0)
            {
                tempValue /= 10;
                length++;
            }

            string result = CreateString(length);
            fixed(char* c = result)
            {
                for (int i = length - 1; i >= 0; i--)
                {
                    c[i] = (char)(value % 10 + '0');
                    value /= 10;
                }
            }

            return result;
        }

        unsafe internal static string ToString(long value)
        {
            if (value >= 0)
                return ToString((ulong)value);

            if (value == -9223372036854775808)
                return "-9223372036854775808";

            int length = 1;
            value = -value;

            var tempValue = value;
            while (tempValue > 0)
            {
                tempValue /= 10;
                length++;
            }

            string result = CreateString(length);
            fixed(char* c = result)
            {
                c[0] = '-';
                for (int i = length - 1; i >= 1; i--)
                {
                    c[i] = (char)(value % 10 + '0');
                    value /= 10;
                }
            }

            return result;
        }

        unsafe internal static string ToString(ulong value)
        {
            if (value == 0)
                return "0";

            int length = 0;
            var tempValue = value;
            while (tempValue > 0)
            {
                tempValue /= 10;
                length++;
            }

            string result = CreateString(length);
            fixed(char* c = result)
            {
                for (int i = length - 1; i >= 0; i--)
                {
                    c[i] = (char)(value % 10 + '0');
                    value /= 10;
                }
            }

            return result;
        }

        public static string Format(string formatString, params object[] objects)
        {
            return formatString;
        }

        public int IndexOf(char value)
        {
            return IndexOf(value, 0);
        }

        public int IndexOf(char value, int startIndex)
        {
            if (startIndex < 0 || startIndex >= Length)
            {
                throw new IndexOutOfRangeException("startIndex");
            }

            for (var i = startIndex; i < Length; ++i)
            {
                if (this[i] == value)
                    return i;
            }

            return -1;
        }

        public string Trim()
        {
            return Trim(' ');
        }

        public unsafe string Trim(char value)
        {
            if (Length == 0)
            {
                return Empty;
            }

            var startIndex = 0;
            for (var i = 0; i < Length; ++i)
            {
                if (this[i] != value)
                {
                    startIndex = i;
                    break;
                }
            }

            var endIndex = Length - 1;
            for (var i = Length - 1; i >= 0 && i > startIndex; --i)
            {
                if (this[i] != value)
                {
                    endIndex = i;
                    break;
                }
            }

            if (startIndex == 0 && endIndex == Length - 1)
            {
                return this;
            }

            fixed(char* characters = &_firstChar)
            {
                return CreateString(characters, startIndex, endIndex - startIndex + 1);
            }
        }
    }

    public sealed class CLSCompliantAttribute : Attribute
    {
        public CLSCompliantAttribute(bool isCompliant)
        {
            this.IsCompliant = isCompliant;
        }

        public bool IsCompliant { get; }
    }

    public class InvalidOperationException : SystemException
    {
        public InvalidOperationException()
        {
        }

        public InvalidOperationException(string msg) : base(msg)
        {
        }
    }

    public class NullReferenceException : SystemException
    {
        public NullReferenceException()
        {
        }

        public NullReferenceException(string msg) : base(msg)
        {
        }
    }

    public class IndexOutOfRangeException : SystemException
    {
        public IndexOutOfRangeException()
        {
        }

        public IndexOutOfRangeException(string msg) : base(msg)
        {
        }
    }

    public class OverflowException : ArithmeticException
    {
        public OverflowException()
            : base("Arithmetic operation resulted in an overflow.")
        {
            this.SetErrorCode(-2146233066);
        }

        public OverflowException(string message)
            : base(message)
        {
            this.SetErrorCode(-2146233066);
        }

        public OverflowException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.SetErrorCode(-2146233066);
        }
    }

    public class ArithmeticException : SystemException
    {
        public ArithmeticException()
            : base("Overflow or underflow in the arithmetic operation.")
        {
            this.SetErrorCode(-2147024362);
        }

        public ArithmeticException(string message)
            : base(message)
        {
            this.SetErrorCode(-2147024362);
        }

        public ArithmeticException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.SetErrorCode(-2147024362);
        }
    }

    public class SystemException : Exception
    {
        public SystemException()
            : base("System error.")
        {
            this.SetErrorCode(-2146233087);
        }

        public SystemException(string message)
            : base(message)
        {
            this.SetErrorCode(-2146233087);
        }

        public SystemException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.SetErrorCode(-2146233087);
        }
    }

    public abstract class ValueType
    {
        public override bool Equals(object obj)
        {
            Environment.FailFast("System.Object::Equals must be implemented on the derived class. The Tiny profile does not support default equality checking for value types.");
            return false;
        }

        public override int GetHashCode()
        {
            Environment.FailFast("System.Object::GetHashCode must be implemented on the derived class. The Tiny profile does not support default hash codes for value types.");
            return 0;
        }
    }

    public struct Void
    {
    }

    public abstract class Attribute
    {
    }

    public class SerializableAttribute : Attribute
    {
    }

    public interface IEquatable<T>
    {
        bool Equals(T other);
    }

    public interface IFormattable
    {
        string ToString(string format, IFormatProvider formatProvider);
    }

    public interface IFormatProvider
    {
        object GetFormat(Type formatType);
    }

    public static class Console
    {
        public static void WriteLine(string input)
        {
            Console_Write(input, 1);
        }

        public static void Write(string input)
        {
            Console_Write(input, 0);
        }

        [DllImport("__Internal")]
        extern private static void Console_Write([MarshalAs(UnmanagedType.LPStr)] string input, int newline);
    }

    public abstract class Enum : ValueType
    {
        public override bool Equals(object other)
        {
            // This method should never be called. IL2CPP will remap the virtual call
            // to a proper method defined in the runtime code.
            Environment.FailFast("System.Enum::Equals should never be called. IL2CPP should remap this to a call in the runtime code.");
            return false;
        }

        public override int GetHashCode()
        {
            Environment.FailFast("Enum::GetHashCode should not be called. The GetHashCode for the underlying type should be called instead.");
            return 0;
        }
    }

    public struct Boolean
    {
        public override string ToString()
        {
            if (this)
                return "True";

            return "False";
        }
    }

    public struct Int16 : IEquatable<Int16>, IComparable<Int16>
    {
        public const short MaxValue = (short)0x7FFF;
        public const short MinValue = unchecked((short)0x8000);
#if IL2CPP_MONO_DEBUGGER
        private short m_value;
#endif

        public bool Equals(Int16 other)
        {
            return this == other;
        }

        public int CompareTo(Int16 value)
        {
            if (this < value) return -1;
            if (this > value) return 1;
            return 0;
        }

        public override int GetHashCode() => (Int32)(this);
    }

    public struct UInt16 : IEquatable<UInt16>, IComparable<UInt16>
    {
        public const ushort MaxValue = (ushort)0xFFFF;
        public const ushort MinValue = 0;
#if IL2CPP_MONO_DEBUGGER
        private ushort m_value;
#endif

        public bool Equals(UInt16 other)
        {
            return this == other;
        }

        public int CompareTo(UInt16 value)
        {
            if (this < value) return -1;
            if (this > value) return 1;
            return 0;
        }

        public override int GetHashCode() => (Int32)(this);
    }

    public struct Int32 : IEquatable<Int32>, IComparable<Int32>
    {
        public const int MaxValue = 0x7fffffff;
        public const int MinValue = unchecked((int)0x80000000);

        public override string ToString() => string.ToString(this);
        public string ToString(string formatString, IFormatProvider formatProvider) => string.ToString(this);
#if IL2CPP_MONO_DEBUGGER
        private int m_value;
#endif

        public bool Equals(Int32 other)
        {
            return this == other;
        }

        public int CompareTo(Int32 value)
        {
            if (this < value) return -1;
            if (this > value) return 1;
            return 0;
        }

        public override int GetHashCode() => (Int32)(this);
    }

    public struct UInt32 : IEquatable<UInt32>, IComparable<UInt32>
    {
        public const uint MaxValue = (uint)0xffffffff;
        public const uint MinValue = 0U;

        public override string ToString() => string.ToString(this);
        public string ToString(string formatString, IFormatProvider formatProvider) => string.ToString(this);
#if IL2CPP_MONO_DEBUGGER
        private uint m_value;
#endif

        public bool Equals(UInt32 other)
        {
            return this == other;
        }

        public int CompareTo(UInt32 value)
        {
            if (this < value) return -1;
            if (this > value) return 1;
            return 0;
        }

        public override int GetHashCode() => (Int32)(this);
    }

    public struct Int64 : IEquatable<Int64>, IComparable<Int64>
    {
        public const long MaxValue = 0x7fffffffffffffffL;
        public const long MinValue = unchecked((long)0x8000000000000000L);

        public override string ToString() => string.ToString(this);
        public string ToString(string formatString, IFormatProvider formatProvider) => string.ToString(this);
#if IL2CPP_MONO_DEBUGGER
        private long m_value;
#endif

        public bool Equals(Int64 other)
        {
            return this == other;
        }

        public int CompareTo(Int64 value)
        {
            if (this < value) return -1;
            if (this > value) return 1;
            return 0;
        }

        public override int GetHashCode() => (Int32)(this);
    }

    public struct UInt64 : IEquatable<UInt64>, IComparable<UInt64>
    {
        public const ulong MaxValue = (ulong)0xffffffffffffffffL;
        public const ulong MinValue = 0x0;

        public override string ToString() => string.ToString(this);
        public string ToString(string formatString, IFormatProvider formatProvider) => string.ToString(this);
#if IL2CPP_MONO_DEBUGGER
        private ulong m_value;
#endif

        public bool Equals(UInt64 other)
        {
            return this == other;
        }

        public int CompareTo(UInt64 value)
        {
            if (this < value) return -1;
            if (this > value) return 1;
            return 0;
        }

        public override int GetHashCode() => (Int32)(this);
    }

    public struct Single
    {
        public const float MinValue = (float)-3.40282346638528859e+38;
        public const float Epsilon = (float)1.4e-45;
        public const float MaxValue = (float)3.40282346638528859e+38;
        public const float PositiveInfinity = (float)1.0 / (float)0.0;
        public const float NegativeInfinity = (float)-1.0 / (float)0.0;
        public const float NaN = (float)0.0 / (float)0.0;

        public string ToString(string format, IFormatProvider formatProvider) => "System.Single";

        public static unsafe bool IsInfinity(float f)
        {
            return (*(int*)&f & int.MaxValue) == 2139095040;
        }

        [SecuritySafeCritical]
        public static unsafe bool IsPositiveInfinity(float f)
        {
            return *(int*)&f == 2139095040;
        }

        [SecuritySafeCritical]
        public static unsafe bool IsNegativeInfinity(float f)
        {
            return *(int*)&f == -8388608;
        }

        [SecuritySafeCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static unsafe bool IsNaN(float f)
        {
            return (*(int*)&f & int.MaxValue) > 2139095040;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool IsFinite(float f)
        {
            return (*(int*)&f & int.MaxValue) < 2139095040;
        }

#if IL2CPP_MONO_DEBUGGER
        private float m_value;
#endif
    }

    public struct Double
    {
        public const double MinValue = -1.7976931348623157E+308;
        public const double MaxValue = 1.7976931348623157E+308;
        public const double Epsilon = 4.9406564584124654E-324;
        public const double NegativeInfinity = (double)-1.0 / (double)(0.0);
        public const double PositiveInfinity = (double)1.0 / (double)(0.0);
        public const double NaN = (double)0.0 / (double)0.0;

        public string ToString(string format, IFormatProvider formatProvider) => "System.Single";

        public static bool IsPositiveInfinity(double d)
        {
            return d == double.PositiveInfinity;
        }

        public static bool IsNegativeInfinity(double d)
        {
            return d == double.NegativeInfinity;
        }

        [SecuritySafeCritical]
        internal static unsafe bool IsNegative(double d)
        {
            return (*(long*)&d & long.MinValue) == long.MinValue;
        }

        [SecuritySafeCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static unsafe bool IsNaN(double d)
        {
            return (ulong)(*(long*)&d & long.MaxValue) > 9218868437227405312UL;
        }

#if IL2CPP_MONO_DEBUGGER
        private double m_value;
#endif
    }

    public unsafe struct IntPtr
    {
        public bool Equals(IntPtr other)
        {
            return m_value == other.m_value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is IntPtr other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked((int)(long)m_value);
        }

        private void* m_value;
        public static readonly IntPtr Zero;

        public static int Size => sizeof(void*);

        public IntPtr(int value)
        {
            m_value = (void*)value;
        }

        public IntPtr(long value)
        {
            m_value = (void*)value;
        }

        public IntPtr(void* value)
        {
            m_value = value;
        }

        public int ToInt32()
        {
            return (int)m_value;
        }

        public static explicit operator IntPtr(int value)
        {
            return new IntPtr(value);
        }

        public static explicit operator IntPtr(long value)
        {
            return new IntPtr(value);
        }

        public static explicit operator IntPtr(void* value)
        {
            return new IntPtr(value);
        }

        public unsafe static explicit operator void*(IntPtr value)
        {
            return value.m_value;
        }

        public unsafe static explicit operator int(IntPtr value)
        {
            return (int)value.m_value;
        }

        public unsafe static explicit operator long(IntPtr value)
        {
            return (long)value.m_value;
        }

        public static IntPtr operator+(IntPtr pointer, int offset)
        {
            return new IntPtr((byte*)pointer.m_value + offset);
        }

        public void* ToPointer()
        {
            return m_value;
        }

        public static bool operator==(IntPtr a, IntPtr b)
        {
            return a.m_value == b.m_value;
        }

        public static bool operator!=(IntPtr a, IntPtr b)
        {
            return !(a == b);
        }
    }

    public unsafe struct UIntPtr
    {
        public static readonly UIntPtr Zero = new UIntPtr(0u);
        private void* _pointer;

        public UIntPtr(ulong value)
        {
            if ((value > UInt32.MaxValue) && (UIntPtr.Size < 8))
            {
                throw new OverflowException();
            }

            _pointer = (void*)value;
        }

        public UIntPtr(uint value)
        {
            _pointer = (void*)value;
        }

        public unsafe UIntPtr(void* value)
        {
            _pointer = value;
        }

        public static explicit operator uint(UIntPtr value)
        {
            return (uint)value._pointer;
        }

        public static explicit operator ulong(UIntPtr value)
        {
            return (ulong)value._pointer;
        }

        public static unsafe explicit operator UIntPtr(void* value)
        {
            return new UIntPtr(value);
        }

        public static int Size
        {
            get { return sizeof(void*); }
        }
    }

    public struct SByte
    {
        public const sbyte MaxValue = 0x7F;
        public const sbyte MinValue = unchecked((sbyte)0x80);
#if IL2CPP_MONO_DEBUGGER
        private sbyte m_value;
#endif
        public override int GetHashCode() => (Int32)(this);
    }

    public struct Byte
    {
        public const byte MaxValue = 0xFF;
        public const byte MinValue = 0;
#if IL2CPP_MONO_DEBUGGER
        private byte m_value;
#endif
        public override int GetHashCode() => (Int32)(this);
    }

    [Flags]
    public enum AttributeTargets
    {
        Assembly = 1,
        Module = 2,
        Class = 4,
        Struct = 8,
        Enum = 16,
        Constructor = 32,
        Method = 64,
        Property = 128,
        Field = 256,
        Event = 512,
        Interface = 1024,
        Parameter = 2048,
        Delegate = 4096,
        ReturnValue = 8192,
        GenericParameter = 16384,
        All = 32767
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageAttribute : Attribute
    {
        public AttributeUsageAttribute(AttributeTargets validOn)
        {
            ValidOn = validOn;
        }

        public AttributeTargets ValidOn { get; }
        public bool AllowMultiple { get; set; }
        public bool Inherited { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class ParamArrayAttribute : Attribute
    {
    }

    public sealed class Type : MemberInfo
    {
        private Type()
        {
        }

        public string AssemblyQualifiedName => "NotImplemented";

        public Type BaseType
        {
            [Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.InternalCall)]
            get;
        }

        public bool IsClass
        {
            get
            {
                if (IsValueType)
                    return false;

                // This method returns true if Type is a class or delegate; that is, not a value type or interface.
                var baseType = BaseType;
                while (baseType != null)
                {
                    if (baseType == typeof(object))
                        return true;

                    baseType = baseType.BaseType;
                }

                // Doesn't derive from object - must be an interface
                return false;
            }
        }

        public bool IsPrimitive
        {
            get
            {
                return this == typeof(bool) || this == typeof(char) ||
                    this == typeof(sbyte) || this == typeof(byte) ||
                    this == typeof(short) || this == typeof(ushort) ||
                    this == typeof(int) || this == typeof(uint) ||
                    this == typeof(long) || this == typeof(ulong) ||
                    this == typeof(IntPtr) || this == typeof(UIntPtr) ||
                    this == typeof(float) || this == typeof(double);
            }
        }

        public bool IsValueType
        {
            get
            {
                var baseType = BaseType;
                return baseType == typeof(ValueType) || baseType == typeof(Enum);
            }
        }

        public static Type GetTypeFromHandle(RuntimeTypeHandle handle)
        {
            return GetTypeFromHandleInternal(handle.Value);
        }

        public bool Equals(Type o)
        {
            return this == o;
        }

        [Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        private static extern Type GetTypeFromHandleInternal(IntPtr handle);

        [Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        public extern bool IsAssignableFrom(Type c);
    }

    public struct RuntimeTypeHandle
    {
        private IntPtr m_Value;

        public IntPtr Value { get { return m_Value; } }
    }


    public class MulticastDelegate : Delegate
    {
        // This is a clever optimization folks at Microsoft came up with:
        // multiple delegates can share the same array if the first "delegateCount" elements match
        // This allows chaining delegates to not reallocate delegate array on every combination/removal
        private MulticastDelegate[] delegates;
        private int delegateCount;

        private protected MulticastDelegate()
        {
        }

        internal MulticastDelegate Combine(MulticastDelegate other)
        {
            var otherDelegates = other.delegates;
            int otherCount = otherDelegates != null ? other.delegateCount : 1;

            var myDelegates = delegates;
            if (myDelegates == null)
            {
                int resultCount = otherCount + 1;
                var resultDelegates = new MulticastDelegate[resultCount];
                resultDelegates[0] = this;
                if (otherDelegates == null)
                {
                    resultDelegates[1] = other;
                }
                else
                {
                    for (int i = 0; i < otherCount; i++)
                        resultDelegates[i + i] = otherDelegates[i];
                }

                return CreateCombinedDelegate(GetType(), resultDelegates, resultCount);
            }
            else
            {
                int myCount = delegateCount;
                int myDelegatesArrayLength = myDelegates.Length;
                int resultCount = myCount + otherCount;

                if (resultCount <= myDelegatesArrayLength)
                {
                    if (otherDelegates == null)
                    {
                        if (myDelegates[myCount] != null)
                        {
                            myDelegates[myCount] = other;
                            return CreateCombinedDelegate(GetType(), myDelegates, resultCount);
                        }
                    }
                    else
                    {
                        bool success = true;
                        for (int i = 0; i < otherCount; i++)
                        {
                            if (myDelegates[myCount] == null)
                            {
                                success = false;
                                break;
                            }

                            myDelegates[myCount + i] = otherDelegates[i];
                        }

                        if (success)
                            return CreateCombinedDelegate(GetType(), myDelegates, resultCount);
                    }
                }

                int allocCount = myDelegatesArrayLength;
                while (allocCount < resultCount)
                    allocCount *= 2;

                var resultDelegates = new MulticastDelegate[allocCount];

                for (int i = 0; i < myCount; i++)
                    resultDelegates[i] = myDelegates[i];

                if (otherDelegates == null)
                {
                    resultDelegates[myCount] = other;
                }
                else
                {
                    for (int i = 0; i < otherCount; i++)
                        resultDelegates[myCount + i] = otherDelegates[i];
                }

                return CreateCombinedDelegate(GetType(), resultDelegates, resultCount);
            }
        }

        internal MulticastDelegate Remove(MulticastDelegate other)
        {
            var myDelegates = delegates;
            var otherDelegates = other.delegates;

            if (otherDelegates == null)
            {
                if (myDelegates == null)
                {
                    if (this.SimpleEquals(other))
                        return null;

                    return this;
                }

                int myCount = delegateCount;
                for (int i = myCount; --i >= 0;)
                {
                    if (other.SimpleEquals(myDelegates[i]))
                    {
                        if (myCount == 2)
                        {
                            // Special case - only one value left, either at the beginning or the end
                            return myDelegates[1 - i];
                        }
                        else
                        {
                            var newDelegates = DeleteFromDelegateList(myDelegates, myCount, i, 1);
                            return CreateCombinedDelegate(GetType(), newDelegates, myCount - 1);
                        }
                    }
                }

                return this;
            }
            else if (myDelegates != null)
            {
                int myCount = delegateCount;
                int otherCount = other.delegateCount;
                for (int i = myCount - otherCount; i >= 0; i--)
                {
                    if (EqualDelegateLists(myDelegates, otherDelegates, i, otherCount))
                    {
                        if (myCount - otherCount == 0)
                            return null;

                        if (myCount - otherCount == 1)
                        {
                            // Special case - only one value left, either at the beginning or the end
                            return myDelegates[i != 0 ? 0 : myCount - 1];
                        }

                        var newDelegates = DeleteFromDelegateList(myDelegates, myCount, i, otherCount);
                        return CreateCombinedDelegate(GetType(), newDelegates, myCount - otherCount);
                    }
                }
            }

            return this;
        }

        private static MulticastDelegate[] DeleteFromDelegateList(MulticastDelegate[] delegates, int delegateCount, int deleteIndex, int deleteCount)
        {
            int allocCount = delegates.Length;
            while (allocCount / 2 >= delegateCount - deleteCount)
                allocCount /= 2;

            var newDelegates = new MulticastDelegate[allocCount];

            for (int i = 0; i < deleteIndex; i++)
                newDelegates[i] = delegates[i];

            for (int i = deleteIndex + deleteCount; i < delegateCount; i++)
                newDelegates[i - deleteCount] = delegates[i];

            return newDelegates;
        }

        private bool EqualDelegateLists(MulticastDelegate[] a, MulticastDelegate[] b, int start, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!a[start + i].SimpleEquals(b[i]))
                    return false;
            }

            return true;
        }

        private unsafe bool SimpleEquals(MulticastDelegate other)
        {
            // NOTE: this assumes delegate invocation lists are empty!
            return method_ptr.ToPointer() == other.method_ptr.ToPointer() && m_target == other.m_target;
        }

        [Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        private static extern MulticastDelegate CreateCombinedDelegate(Type delegateType, MulticastDelegate[] delegates, int delegateCount);
    }

    public unsafe abstract class Delegate
    {
        [Preserve]
        private protected readonly IntPtr method_ptr;
        [Preserve]
        private protected readonly object m_target;
        [Preserve]
        private readonly void* m_ReversePInvokeWrapperPtr;
        [Preserve]
        private readonly bool m_IsDelegateOpen;

        private protected Delegate() {}

        public static Delegate Combine(Delegate a, Delegate b)
        {
            if (a == null)
                return b;

            if (b == null)
                return a;

            var multicastA = a as MulticastDelegate;
            if (multicastA == null || a.GetType() != b.GetType())
                throw new Exception();

            return multicastA.Combine((MulticastDelegate)b);
        }

        public static Delegate Remove(Delegate source, Delegate value)
        {
            if (source == null)
                return null;

            if (value == null)
                return source;

            var sourceAsMulticastDelegate = source as MulticastDelegate;
            if (sourceAsMulticastDelegate == null || source.GetType() != value.GetType())
                throw new Exception();

            return sourceAsMulticastDelegate.Remove((MulticastDelegate)value);
        }
    }

    public class NotImplementedException : SystemException
    {
        public NotImplementedException(string msg = null) : base(msg)
        {
        }
    }

    public class NotSupportedException : SystemException
    {
        public NotSupportedException()
        {
        }

        public NotSupportedException(string message)
            : base(message)
        {
        }
    }

    public class Exception
    {
        public string Message { get; }
        public string StackTrace { get; } = "";

        public Exception()
        {
        }

        public Exception(string message)
        {
            Message = message;
        }

        public Exception(string message, Exception innerException)
        {
            Message = message;
        }

        internal void SetErrorCode(int hr)
        {
        }
    }

    public class ArgumentException : SystemException
    {
        public ArgumentException()
        {
        }

        public ArgumentException(string arg)
        {
        }

        public ArgumentException(string arg1, string arg2)
        {
        }
    }

    public class ArgumentOutOfRangeException : ArgumentException
    {
        public ArgumentOutOfRangeException(string arg)
        {
        }

        public ArgumentOutOfRangeException(string arg, string arg2)
        {
        }
    }

    public class ArgumentNullException : ArgumentException
    {
        public ArgumentNullException()
        {
        }

        public ArgumentNullException(string arg)
        {
        }
    }

    public delegate void Action();
    public delegate void Action<in T>(T obj);
    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);
    public delegate void Action<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void Action<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6, in T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    public delegate TResult Func<out TResult>();
    public delegate TResult Func<in T, out TResult>(T arg);
    public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
    public delegate TResult Func<in T1, in T2, in T3, out TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Func<in T1, in T2, in T3, in T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    public static class GC
    {
        [Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        extern private static void InternalCollect(int generation);

        public static void KeepAlive(object obj)
        {
        }

        public static void Collect()
        {
            InternalCollect(0);
        }
    }

    public interface IComparable<in T>
    {
        int CompareTo(T other);
    }

    public static class Environment
    {
        public static void FailFast(string message)
        {
            FailFast_internal(message);
        }

        public static string[] GetCommandLineArgs()
        {
            return new string[0];
        }

        public static string StackTrace
        {
            get { return GetStackTrace_internal(); }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static string GetStackTrace_internal();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static string FailFast_internal(string message);
    }

#if IL2CPP_MONO_DEBUGGER

    internal class RuntimeType
    {
        object type;
        object type_info;
        object genericCache;
        object serializationCtor;
    }

#endif

    [Serializable]
    public struct TimeSpan : IComparable<TimeSpan>, IEquatable<TimeSpan>
    {
        public const long    TicksPerMillisecond =  10000;
        private const double MillisecondsPerTick = 1.0 / TicksPerMillisecond;

        public const long TicksPerSecond = TicksPerMillisecond * 1000;   // 10,000,000
        private const double SecondsPerTick =  1.0 / TicksPerSecond;         // 0.0001

        public const long TicksPerMinute = TicksPerSecond * 60;         // 600,000,000
        private const double MinutesPerTick = 1.0 / TicksPerMinute; // 1.6666666666667e-9

        public const long TicksPerHour = TicksPerMinute * 60;        // 36,000,000,000
        private const double HoursPerTick = 1.0 / TicksPerHour; // 2.77777777777777778e-11

        public const long TicksPerDay = TicksPerHour * 24;          // 864,000,000,000
        private const double DaysPerTick = 1.0 / TicksPerDay; // 1.1574074074074074074e-12

        private const int MillisPerSecond = 1000;
        private const int MillisPerMinute = MillisPerSecond * 60; //     60,000
        private const int MillisPerHour = MillisPerMinute * 60;   //  3,600,000
        private const int MillisPerDay = MillisPerHour * 24;      // 86,400,000

        internal const long MaxSeconds = Int64.MaxValue / TicksPerSecond;
        internal const long MinSeconds = Int64.MinValue / TicksPerSecond;

        internal const long MaxMilliSeconds = Int64.MaxValue / TicksPerMillisecond;
        internal const long MinMilliSeconds = Int64.MinValue / TicksPerMillisecond;

        internal const long TicksPerTenthSecond = TicksPerMillisecond * 100;

        public static readonly TimeSpan Zero = new TimeSpan(0);

        public static readonly TimeSpan MaxValue = new TimeSpan(Int64.MaxValue);
        public static readonly TimeSpan MinValue = new TimeSpan(Int64.MinValue);

        private long _ticks;

        private const string Error_Overflow_TimeSpanTooLong = "TimeSpan overflowed because the duration is too long.";
        private const string Error_Arg_MustBeTimeSpan = "Object must be of type TimeSpan.";
        private const string Error_Overflow_Duration = "The duration cannot be returned for TimeSpan.MinValue because the absolute value of TimeSpan.MinValue exceeds the value of TimeSpan.MaxValue.";
        private const string Error_Arg_CannotBeNaN = "TimeSpan does not accept floating point Not-a-Number values.";
        private const string Error_Overflow_NegateTwosCompNum = "Negating the minimum value of a twos complement number is invalid.";


        public TimeSpan(long ticks)
        {
            this._ticks = ticks;
        }

        public TimeSpan(int hours, int minutes, int seconds)
        {
            _ticks = TimeToTicks(hours, minutes, seconds);
        }

        public TimeSpan(int days, int hours, int minutes, int seconds)
            : this(days, hours, minutes, seconds, 0)
        {
        }

        public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
        {
            Int64 totalMilliSeconds = ((Int64)days * 3600 * 24 + (Int64)hours * 3600 + (Int64)minutes * 60 + seconds) * 1000 + milliseconds;
            if (totalMilliSeconds > MaxMilliSeconds || totalMilliSeconds < MinMilliSeconds)
                throw new ArgumentOutOfRangeException(null, Error_Overflow_TimeSpanTooLong);
            _ticks =  (long)totalMilliSeconds * TicksPerMillisecond;
        }

        public long Ticks
        {
            get { return _ticks; }
        }

        public int Days
        {
            get { return (int)(_ticks / TicksPerDay); }
        }

        public int Hours
        {
            get { return (int)((_ticks / TicksPerHour) % 24); }
        }

        public int Milliseconds
        {
            get { return (int)((_ticks / TicksPerMillisecond) % 1000); }
        }

        public int Minutes
        {
            get { return (int)((_ticks / TicksPerMinute) % 60); }
        }

        public int Seconds
        {
            get { return (int)((_ticks / TicksPerSecond) % 60); }
        }

        public double TotalDays
        {
            get { return ((double)_ticks) * DaysPerTick; }
        }

        public double TotalHours
        {
            get { return (double)_ticks * HoursPerTick; }
        }

        public double TotalMilliseconds
        {
            get
            {
                double temp = (double)_ticks * MillisecondsPerTick;
                if (temp > MaxMilliSeconds)
                    return (double)MaxMilliSeconds;

                if (temp < MinMilliSeconds)
                    return (double)MinMilliSeconds;

                return temp;
            }
        }

        public double TotalMinutes
        {
            get { return (double)_ticks * MinutesPerTick; }
        }

        public double TotalSeconds
        {
            get { return (double)_ticks * SecondsPerTick; }
        }

        public TimeSpan Add(TimeSpan ts)
        {
            long result = _ticks + ts._ticks;
            // Overflow if signs of operands was identical and result's
            // sign was opposite.
            // >> 63 gives the sign bit (either 64 1's or 64 0's).
            if ((_ticks >> 63 == ts._ticks >> 63) && (_ticks >> 63 != result >> 63))
                throw new OverflowException(Error_Overflow_TimeSpanTooLong);
            return new TimeSpan(result);
        }

        // Compares two TimeSpan values, returning an integer that indicates their
        // relationship.
        //
        public static int Compare(TimeSpan t1, TimeSpan t2)
        {
            if (t1._ticks > t2._ticks) return 1;
            if (t1._ticks < t2._ticks) return -1;
            return 0;
        }

        // Returns a value less than zero if this  object
        public int CompareTo(Object value)
        {
            if (value == null) return 1;
            if (!(value is TimeSpan))
                throw new ArgumentException(Error_Arg_MustBeTimeSpan);
            long t = ((TimeSpan)value)._ticks;
            if (_ticks > t) return 1;
            if (_ticks < t) return -1;
            return 0;
        }

        public int CompareTo(TimeSpan value)
        {
            long t = value._ticks;
            if (_ticks > t) return 1;
            if (_ticks < t) return -1;
            return 0;
        }

        public static TimeSpan FromDays(double value)
        {
            return Interval(value, MillisPerDay);
        }

        public TimeSpan Duration()
        {
            if (Ticks == TimeSpan.MinValue.Ticks)
                throw new OverflowException(Error_Overflow_Duration);
            return new TimeSpan(_ticks >= 0 ? _ticks : -_ticks);
        }

        public override bool Equals(Object value)
        {
            if (value is TimeSpan)
            {
                return _ticks == ((TimeSpan)value)._ticks;
            }
            return false;
        }

        public bool Equals(TimeSpan obj)
        {
            return _ticks == obj._ticks;
        }

        public static bool Equals(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks == t2._ticks;
        }

        public override int GetHashCode()
        {
            return (int)_ticks ^ (int)(_ticks >> 32);
        }

        public static TimeSpan FromHours(double value)
        {
            return Interval(value, MillisPerHour);
        }

        private static TimeSpan Interval(double value, int scale)
        {
            if (Double.IsNaN(value))
                throw new ArgumentException(Error_Arg_CannotBeNaN);
            double tmp = value * scale;
            double millis = tmp + (value >= 0 ? 0.5 : -0.5);
            if ((millis > Int64.MaxValue / TicksPerMillisecond) || (millis < Int64.MinValue / TicksPerMillisecond))
                throw new OverflowException(Error_Overflow_TimeSpanTooLong);
            return new TimeSpan((long)millis * TicksPerMillisecond);
        }

        public static TimeSpan FromMilliseconds(double value)
        {
            return Interval(value, 1);
        }

        public static TimeSpan FromMinutes(double value)
        {
            return Interval(value, MillisPerMinute);
        }

        public TimeSpan Negate()
        {
            if (Ticks == TimeSpan.MinValue.Ticks)
                throw new OverflowException(Error_Overflow_NegateTwosCompNum);
            return new TimeSpan(-_ticks);
        }

        public static TimeSpan FromSeconds(double value)
        {
            return Interval(value, MillisPerSecond);
        }

        public TimeSpan Subtract(TimeSpan ts)
        {
            long result = _ticks - ts._ticks;
            // Overflow if signs of operands was different and result's
            // sign was opposite from the first argument's sign.
            // >> 63 gives the sign bit (either 64 1's or 64 0's).
            if ((_ticks >> 63 != ts._ticks >> 63) && (_ticks >> 63 != result >> 63))
                throw new OverflowException(Error_Overflow_TimeSpanTooLong);
            return new TimeSpan(result);
        }

        public static TimeSpan FromTicks(long value)
        {
            return new TimeSpan(value);
        }

        internal static long TimeToTicks(int hour, int minute, int second)
        {
            // totalSeconds is bounded by 2^31 * 2^12 + 2^31 * 2^8 + 2^31,
            // which is less than 2^44, meaning we won't overflow totalSeconds.
            long totalSeconds = (long)hour * 3600 + (long)minute * 60 + (long)second;
            if (totalSeconds > MaxSeconds || totalSeconds < MinSeconds)
                throw new ArgumentOutOfRangeException(null, Error_Overflow_TimeSpanTooLong);
            return totalSeconds * TicksPerSecond;
        }

        public static TimeSpan operator-(TimeSpan t)
        {
            if (t._ticks == TimeSpan.MinValue._ticks)
                throw new OverflowException(Error_Overflow_NegateTwosCompNum);
            return new TimeSpan(-t._ticks);
        }

        public static TimeSpan operator-(TimeSpan t1, TimeSpan t2)
        {
            return t1.Subtract(t2);
        }

        public static TimeSpan operator+(TimeSpan t)
        {
            return t;
        }

        public static TimeSpan operator+(TimeSpan t1, TimeSpan t2)
        {
            return t1.Add(t2);
        }

        public static bool operator==(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks == t2._ticks;
        }

        public static bool operator!=(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks != t2._ticks;
        }

        public static bool operator<(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks < t2._ticks;
        }

        public static bool operator<=(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks <= t2._ticks;
        }

        public static bool operator>(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks > t2._ticks;
        }

        public static bool operator>=(TimeSpan t1, TimeSpan t2)
        {
            return t1._ticks >= t2._ticks;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct Guid : IFormattable, IComparable<Guid>, IEquatable<Guid>
    {
        public static readonly Guid Empty = new Guid();

        private int _a;
        private short _b;
        private short _c;
        private byte _d;
        private byte _e;
        private byte _f;
        private byte _g;
        private byte _h;
        private byte _i;
        private byte _j;
        private byte _k;

        [Flags]
        private enum GuidStyles
        {
            None = 0x00000000,
            AllowParenthesis = 0x00000001, // Allow the guid to be enclosed in parens
            AllowBraces = 0x00000002, // Allow the guid to be enclosed in braces
            AllowDashes = 0x00000004, // Allow the guid to contain dash group separators
            AllowHexPrefix = 0x00000008, // Allow the guid to contain {0xdd,0xdd}
            RequireParenthesis = 0x00000010, // Require the guid to be enclosed in parens
            RequireBraces = 0x00000020, // Require the guid to be enclosed in braces
            RequireDashes = 0x00000040, // Require the guid to contain dash group separators
            RequireHexPrefix = 0x00000080, // Require the guid to contain {0xdd,0xdd}

            HexFormat = RequireBraces | RequireHexPrefix,           /* X */
            NumberFormat = None,                                    /* N */
            DigitFormat = RequireDashes,                            /* D */
            BraceFormat = RequireBraces | RequireDashes,            /* B */
            ParenthesisFormat = RequireParenthesis | RequireDashes, /* P */

            Any = AllowParenthesis | AllowBraces | AllowDashes | AllowHexPrefix,
        }

        public Guid(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("b");
            if (bytes.Length != 16)
                throw new ArgumentException("b.Length != 16");

            _a = ((int)bytes[3] << 24) | ((int)bytes[2] << 16) | ((int)bytes[1] << 8) | bytes[0];
            _b = (short)(((int)bytes[5] << 8) | bytes[4]);
            _c = (short)(((int)bytes[7] << 8) | bytes[6]);
            _d = bytes[8];
            _e = bytes[9];
            _f = bytes[10];
            _g = bytes[11];
            _h = bytes[12];
            _i = bytes[13];
            _j = bytes[14];
            _k = bytes[15];
        }

        public Guid(int a, short b, short c, byte[] d)
        {
            if (d == null)
                throw new ArgumentNullException("d");
            if (d.Length != 8)
                throw new ArgumentException("d.Length != 8");

            _a = a;
            _b = b;
            _c = c;
            _d = d[0];
            _e = d[1];
            _f = d[2];
            _g = d[3];
            _h = d[4];
            _i = d[5];
            _j = d[6];
            _k = d[7];
        }

        public Guid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            _e = e;
            _f = f;
            _g = g;
            _h = h;
            _i = i;
            _j = j;
            _k = k;
        }

        [CLSCompliant(false)]
        public Guid(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            _a = (int)a;
            _b = (short)b;
            _c = (short)c;
            _d = d;
            _e = e;
            _f = f;
            _g = g;
            _h = h;
            _i = i;
            _j = j;
            _k = k;
        }

        public Guid(string guidString)
        {
            if (guidString == null)
                throw new ArgumentNullException("g");

            this = Empty;

            if (TryParseGuid(guidString, GuidStyles.Any, out var guid))
                this = guid;
            else
                throw new Exception("guid parse failure");
        }

        public static Guid Parse(string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (TryParseGuid(input, GuidStyles.Any, out var guid))
                return guid;
            else
                throw new Exception("guid parse failure");
        }

        public static bool TryParse(string input, out Guid result)
        {
            return TryParseGuid(input, GuidStyles.Any, out result);
        }

        public static Guid ParseExact(string input, string format)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (format == null)
                throw new ArgumentNullException("format");
            if (format.Length != 1)
                throw new ArgumentException("format.Length != 1");

            GuidStyles style;
            char formatCh = format[0];
            if (formatCh == 'D' || formatCh == 'd')
                style = GuidStyles.DigitFormat;
            else if (formatCh == 'N' || formatCh == 'n')
                style = GuidStyles.NumberFormat;
            else if (formatCh == 'B' || formatCh == 'b')
                style = GuidStyles.BraceFormat;
            else if (formatCh == 'P' || formatCh == 'p')
                style = GuidStyles.ParenthesisFormat;
            else if (formatCh == 'X' || formatCh == 'x')
                style = GuidStyles.HexFormat;
            else
                throw new ArgumentException("Invalid guid format specification");

            if (TryParseGuid(input, style, out var guid))
                return guid;
            else
                throw new Exception("guid parse failure");
        }

        public static bool TryParseExact(string input, string format, out Guid result)
        {
            result = Empty;

            if (format == null || format.Length != 1)
                return false;

            GuidStyles style;
            char formatCh = format[0];
            if (formatCh == 'D' || formatCh == 'd')
                style = GuidStyles.DigitFormat;
            else if (formatCh == 'N' || formatCh == 'n')
                style = GuidStyles.NumberFormat;
            else if (formatCh == 'B' || formatCh == 'b')
                style = GuidStyles.BraceFormat;
            else if (formatCh == 'P' || formatCh == 'p')
                style = GuidStyles.ParenthesisFormat;
            else if (formatCh == 'X' || formatCh == 'x')
                style = GuidStyles.HexFormat;
            else
                return false;

            return TryParseGuid(input, style, out result);
        }

        private static bool TryParseGuid(string value, GuidStyles flags, out Guid result)
        {
            result = Empty;

            if (value == null)
                return false;

            var guidString = value.Trim();
            if (guidString.Length == 0)
                return false;

            // Check for dashes
            var dashesExistInString = guidString.IndexOf('-') >= 0;
            if (dashesExistInString)
            {
                if ((flags & (GuidStyles.AllowDashes | GuidStyles.RequireDashes)) == 0)
                {
                    return false;
                }
            }
            else
            {
                if ((flags & GuidStyles.RequireDashes) != 0)
                {
                    return false;
                }
            }

            // Check for braces
            var bracesExistInString = guidString.IndexOf('{') >= 0;
            if (bracesExistInString)
            {
                if ((flags & (GuidStyles.AllowBraces | GuidStyles.RequireBraces)) == 0)
                {
                    return false;
                }
            }
            else
            {
                if ((flags & GuidStyles.RequireBraces) != 0)
                {
                    return false;
                }
            }

            // Check for parenthesis
            var parenthesisExistInString = guidString.IndexOf('(') >= 0;
            if (parenthesisExistInString)
            {
                if ((flags & (GuidStyles.AllowParenthesis | GuidStyles.RequireParenthesis)) == 0)
                {
                    return false;
                }
            }
            else
            {
                if ((flags & GuidStyles.RequireParenthesis) != 0)
                {
                    return false;
                }
            }

            try
            {
                if (dashesExistInString)
                {
                    // Check if it's of the form [{|(]dddddddd-dddd-dddd-dddd-dddddddddddd[}|)]
                    return TryParseGuidWithDashes(guidString, ref result);
                }
                else if (bracesExistInString)
                {
                    // Check if it's of the form {0xdddddddd,0xdddd,0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}}
                    return TryParseGuidWithHexPrefix(guidString, ref result);
                }
                else
                {
                    // Check if it's of the form dddddddddddddddddddddddddddddddd
                    return TryParseGuidWithNoStyle(guidString, ref result);
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static bool TryParseGuidWithNoStyle(string guidString, ref Guid result)
        {
            if (guidString.Length != 32)
                return false;

            if (!IsHexChars(guidString, 0, 32))
                return false;

            result._a =
                CharToHex(guidString[0]) << 28 |
                CharToHex(guidString[1]) << 24 |
                CharToHex(guidString[2]) << 20 |
                CharToHex(guidString[3]) << 16 |
                CharToHex(guidString[4]) << 12 |
                CharToHex(guidString[5]) << 8 |
                CharToHex(guidString[6]) << 4 |
                CharToHex(guidString[7]);

            result._b = (short)(
                CharToHex(guidString[8]) << 12 |
                CharToHex(guidString[9]) << 8 |
                CharToHex(guidString[10]) << 4 |
                CharToHex(guidString[11]));

            result._c = (short)(
                CharToHex(guidString[12]) << 12 |
                CharToHex(guidString[13]) << 8 |
                CharToHex(guidString[14]) << 4 |
                CharToHex(guidString[15]));

            result._d = (byte)(
                CharToHex(guidString[16]) << 4 |
                CharToHex(guidString[17]));

            result._e = (byte)(
                CharToHex(guidString[18]) << 4 |
                CharToHex(guidString[19]));

            result._f = (byte)(
                CharToHex(guidString[20]) << 4 |
                CharToHex(guidString[21]));

            result._g = (byte)(
                CharToHex(guidString[22]) << 4 |
                CharToHex(guidString[23]));

            result._h = (byte)(
                CharToHex(guidString[24]) << 4 |
                CharToHex(guidString[25]));

            result._i = (byte)(
                CharToHex(guidString[26]) << 4 |
                CharToHex(guidString[27]));

            result._j = (byte)(
                CharToHex(guidString[28]) << 4 |
                CharToHex(guidString[29]));

            result._k = (byte)(
                CharToHex(guidString[30]) << 4 |
                CharToHex(guidString[31]));

            return true;
        }

        private static bool TryParseGuidWithDashes(string guidString, ref Guid result)
        {
            int startPos = 0;
            if (guidString[0] == '{')
            {
                if (guidString.Length != 38 || guidString[37] != '}')
                    return false;
                startPos = 1;
            }
            else if (guidString[0] == '(')
            {
                if (guidString.Length != 38 || guidString[37] != ')')
                    return false;
                startPos = 1;
            }
            else if (guidString.Length != 36)
            {
                return false;
            }

            if (guidString[8 + startPos] != '-' || guidString[13 + startPos] != '-' ||
                guidString[18 + startPos] != '-' || guidString[23 + startPos] != '-')
                return false;

            if (!IsHexChars(guidString, 0 + startPos, 8) || !IsHexChars(guidString, 9 + startPos, 4) ||
                !IsHexChars(guidString, 14 + startPos, 4) || !IsHexChars(guidString, 19 + startPos, 4) ||
                !IsHexChars(guidString, 24 + startPos, 12))
                return false;

            result._a =
                CharToHex(guidString[0 + startPos]) << 28 |
                CharToHex(guidString[1 + startPos]) << 24 |
                CharToHex(guidString[2 + startPos]) << 20 |
                CharToHex(guidString[3 + startPos]) << 16 |
                CharToHex(guidString[4 + startPos]) << 12 |
                CharToHex(guidString[5 + startPos]) << 8 |
                CharToHex(guidString[6 + startPos]) << 4 |
                CharToHex(guidString[7 + startPos]);

            result._b = (short)(
                CharToHex(guidString[9 + startPos]) << 12 |
                CharToHex(guidString[10 + startPos]) << 8 |
                CharToHex(guidString[11 + startPos]) << 4 |
                CharToHex(guidString[12 + startPos]));

            result._c = (short)(
                CharToHex(guidString[14 + startPos]) << 12 |
                CharToHex(guidString[15 + startPos]) << 8 |
                CharToHex(guidString[16 + startPos]) << 4 |
                CharToHex(guidString[17 + startPos]));

            result._d = (byte)(
                CharToHex(guidString[19 + startPos]) << 4 |
                CharToHex(guidString[20 + startPos]));

            result._e = (byte)(
                CharToHex(guidString[21 + startPos]) << 4 |
                CharToHex(guidString[22 + startPos]));

            result._f = (byte)(
                CharToHex(guidString[24 + startPos]) << 4 |
                CharToHex(guidString[25 + startPos]));

            result._g = (byte)(
                CharToHex(guidString[26 + startPos]) << 4 |
                CharToHex(guidString[27 + startPos]));

            result._h = (byte)(
                CharToHex(guidString[28 + startPos]) << 4 |
                CharToHex(guidString[29 + startPos]));

            result._i = (byte)(
                CharToHex(guidString[30 + startPos]) << 4 |
                CharToHex(guidString[31 + startPos]));

            result._j = (byte)(
                CharToHex(guidString[32 + startPos]) << 4 |
                CharToHex(guidString[33 + startPos]));

            result._k = (byte)(
                CharToHex(guidString[34 + startPos]) << 4 |
                CharToHex(guidString[35 + startPos]));

            return true;
        }

        private static bool TryParseGuidWithHexPrefix(string guidString, ref Guid result)
        {
            if (guidString.Length != 68)
                return false;

            if (guidString[0] != '{' || guidString[26] != '{')
                return false;

            if (guidString[66] != '}' || guidString[67] != '}')
                return false;

            if (guidString[11] != ',' || guidString[18] != ',' || guidString[25] != ',' || guidString[31] != ',' ||
                guidString[36] != ',' || guidString[41] != ',' || guidString[46] != ',' || guidString[51] != ',' ||
                guidString[56] != ',' || guidString[61] != ',')
                return false;

            if (!IsHexPrefix(guidString, 1) || !IsHexPrefix(guidString, 12) || !IsHexPrefix(guidString, 19) ||
                !IsHexPrefix(guidString, 27) || !IsHexPrefix(guidString, 32) || !IsHexPrefix(guidString, 37) ||
                !IsHexPrefix(guidString, 42) || !IsHexPrefix(guidString, 47) || !IsHexPrefix(guidString, 52) ||
                !IsHexPrefix(guidString, 57) || !IsHexPrefix(guidString, 62))
                return false;

            if (!IsHexChars(guidString, 3, 8) || !IsHexChars(guidString, 14, 4) || !IsHexChars(guidString, 21, 4) ||
                !IsHexChars(guidString, 29, 2) || !IsHexChars(guidString, 34, 2) || !IsHexChars(guidString, 39, 2) ||
                !IsHexChars(guidString, 44, 2) || !IsHexChars(guidString, 49, 2) || !IsHexChars(guidString, 54, 2) ||
                !IsHexChars(guidString, 59, 2) || !IsHexChars(guidString, 64, 2))
                return false;

            result._a =
                CharToHex(guidString[3]) << 28 |
                CharToHex(guidString[4]) << 24 |
                CharToHex(guidString[5]) << 20 |
                CharToHex(guidString[6]) << 16 |
                CharToHex(guidString[7]) << 12 |
                CharToHex(guidString[8]) << 8 |
                CharToHex(guidString[9]) << 4 |
                CharToHex(guidString[10]);

            result._b = (short)(
                CharToHex(guidString[14]) << 12 |
                CharToHex(guidString[15]) << 8 |
                CharToHex(guidString[16]) << 4 |
                CharToHex(guidString[17]));

            result._c = (short)(
                CharToHex(guidString[21]) << 12 |
                CharToHex(guidString[22]) << 8 |
                CharToHex(guidString[23]) << 4 |
                CharToHex(guidString[24]));

            result._d = (byte)(
                CharToHex(guidString[29]) << 4 |
                CharToHex(guidString[30]));

            result._e = (byte)(
                CharToHex(guidString[34]) << 4 |
                CharToHex(guidString[35]));

            result._f = (byte)(
                CharToHex(guidString[39]) << 4 |
                CharToHex(guidString[40]));

            result._g = (byte)(
                CharToHex(guidString[44]) << 4 |
                CharToHex(guidString[45]));

            result._h = (byte)(
                CharToHex(guidString[49]) << 4 |
                CharToHex(guidString[50]));

            result._i = (byte)(
                CharToHex(guidString[54]) << 4 |
                CharToHex(guidString[55]));

            result._j = (byte)(
                CharToHex(guidString[59]) << 4 |
                CharToHex(guidString[60]));

            result._k = (byte)(
                CharToHex(guidString[64]) << 4 |
                CharToHex(guidString[65]));

            return true;
        }

        private static bool IsHexChar(char value)
        {
            if (value >= '0' && value <= '9')
                return true;
            else if (value >= 'A' && value <= 'F')
                return true;
            else if (value >= 'a' && value <= 'f')
                return true;

            return false;
        }

        private static bool IsHexChars(string guidString, int index, int length)
        {
            for (var i = index; i < index + length; ++i)
            {
                if (i >= guidString.Length)
                    return false;
                if (!IsHexChar(guidString[i]))
                    return false;
            }
            return true;
        }

        private static bool IsHexPrefix(string guidString, int index)
        {
            if (index + 1 >= guidString.Length)
                return false;
            if (guidString[index] != '0')
                return false;
            if (guidString[index + 1] != 'x' && guidString[index + 1] != 'X')
                return false;

            return true;
        }

        private static int CharToHex(char value)
        {
            if (value >= '0' && value <= '9')
                return value - 0x30;
            else if (value >= 'A' && value <= 'F')
                return value - 0x41 + 10;
            else if (value >= 'a' && value <= 'f')
                return value - 0x61 + 10;

            throw new ArgumentOutOfRangeException("value");
        }

        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[16];

            bytes[0] = (byte)(_a);
            bytes[1] = (byte)(_a >> 8);
            bytes[2] = (byte)(_a >> 16);
            bytes[3] = (byte)(_a >> 24);
            bytes[4] = (byte)(_b);
            bytes[5] = (byte)(_b >> 8);
            bytes[6] = (byte)(_c);
            bytes[7] = (byte)(_c >> 8);
            bytes[8] = _d;
            bytes[9] = _e;
            bytes[10] = _f;
            bytes[11] = _g;
            bytes[12] = _h;
            bytes[13] = _i;
            bytes[14] = _j;
            bytes[15] = _k;

            return bytes;
        }

        public override string ToString()
        {
            return ToString("D", null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null || format.Length == 0)
                format = "D";
            if (format.Length != 1)
                throw new ArgumentException("format.Length != 1");

            string guidString;
            int offset = 0;
            bool dash = true;
            bool hex = false;

            char formatCh = format[0];
            if (formatCh == 'D' || formatCh == 'd')
            {
                guidString = string.CreateString(36);
            }
            else if (formatCh == 'N' || formatCh == 'n')
            {
                guidString = string.CreateString(32);
                dash = false;
            }
            else if (formatCh == 'B' || formatCh == 'b')
            {
                guidString = string.CreateString(38);
                unsafe
                {
                    fixed(char* guidChars = guidString)
                    {
                        guidChars[offset++] = '{';
                        guidChars[37] = '}';
                    }
                }
            }
            else if (formatCh == 'P' || formatCh == 'p')
            {
                guidString = string.CreateString(38);
                unsafe
                {
                    fixed(char* guidChars = guidString)
                    {
                        guidChars[offset++] = '(';
                        guidChars[37] = ')';
                    }
                }
            }
            else if (formatCh == 'X' || formatCh == 'x')
            {
                guidString = string.CreateString(68);
                unsafe
                {
                    fixed(char* guidChars = guidString)
                    {
                        guidChars[offset++] = '{';
                        guidChars[67] = '}';
                    }
                }
                dash = false;
                hex = true;
            }
            else
            {
                throw new ArgumentException("Invalid guid format specification");
            }

            unsafe
            {
                fixed(char* guidChars = guidString)
                {
                    if (hex)
                    {
                        // {0xdddddddd,0xdddd,0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}}
                        guidChars[offset++] = '0';
                        guidChars[offset++] = 'x';
                        offset = HexsToChars(guidChars, offset, _a >> 24, _a >> 16);
                        offset = HexsToChars(guidChars, offset, _a >> 8, _a);
                        guidChars[offset++] = ',';
                        guidChars[offset++] = '0';
                        guidChars[offset++] = 'x';
                        offset = HexsToChars(guidChars, offset, _b >> 8, _b);
                        guidChars[offset++] = ',';
                        guidChars[offset++] = '0';
                        guidChars[offset++] = 'x';
                        offset = HexsToChars(guidChars, offset, _c >> 8, _c);
                        guidChars[offset++] = ',';
                        guidChars[offset++] = '{';
                        offset = HexsToChars(guidChars, offset, _d, _e, true);
                        guidChars[offset++] = ',';
                        offset = HexsToChars(guidChars, offset, _f, _g, true);
                        guidChars[offset++] = ',';
                        offset = HexsToChars(guidChars, offset, _h, _i, true);
                        guidChars[offset++] = ',';
                        offset = HexsToChars(guidChars, offset, _j, _k, true);
                        guidChars[offset++] = '}';
                    }
                    else
                    {
                        // [{|(]dddddddd[-]dddd[-]dddd[-]dddd[-]dddddddddddd[}|)]
                        offset = HexsToChars(guidChars, offset, _a >> 24, _a >> 16);
                        offset = HexsToChars(guidChars, offset, _a >> 8, _a);
                        if (dash) guidChars[offset++] = '-';
                        offset = HexsToChars(guidChars, offset, _b >> 8, _b);
                        if (dash) guidChars[offset++] = '-';
                        offset = HexsToChars(guidChars, offset, _c >> 8, _c);
                        if (dash) guidChars[offset++] = '-';
                        offset = HexsToChars(guidChars, offset, _d, _e);
                        if (dash) guidChars[offset++] = '-';
                        offset = HexsToChars(guidChars, offset, _f, _g);
                        offset = HexsToChars(guidChars, offset, _h, _i);
                        offset = HexsToChars(guidChars, offset, _j, _k);
                    }
                }
            }
            return guidString;
        }

        private static char HexToChar(int value)
        {
            value = value & 0xF;
            return (char)((value > 9) ? value - 10 + 0x61 : value + 0x30);
        }

        [System.Security.SecurityCritical]
        private static unsafe int HexsToChars(char* guidChars, int offset, int a, int b)
        {
            return HexsToChars(guidChars, offset, a, b, false);
        }

        [System.Security.SecurityCritical]
        private static unsafe int HexsToChars(char* guidChars, int offset, int a, int b, bool hex)
        {
            if (hex)
            {
                guidChars[offset++] = '0';
                guidChars[offset++] = 'x';
            }
            guidChars[offset++] = HexToChar(a >> 4);
            guidChars[offset++] = HexToChar(a);
            if (hex)
            {
                guidChars[offset++] = ',';
                guidChars[offset++] = '0';
                guidChars[offset++] = 'x';
            }
            guidChars[offset++] = HexToChar(b >> 4);
            guidChars[offset++] = HexToChar(b);
            return offset;
        }

        public override int GetHashCode()
        {
            return _a ^ (((int)_b << 16) | (int)(ushort)_c) ^ (((int)_f << 24) | _k);
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is Guid guid))
                return false;

            if (guid._a != _a)
                return false;
            if (guid._b != _b)
                return false;
            if (guid._c != _c)
                return false;
            if (guid._d != _d)
                return false;
            if (guid._e != _e)
                return false;
            if (guid._f != _f)
                return false;
            if (guid._g != _g)
                return false;
            if (guid._h != _h)
                return false;
            if (guid._i != _i)
                return false;
            if (guid._j != _j)
                return false;
            if (guid._k != _k)
                return false;

            return true;
        }

        public bool Equals(Guid other)
        {
            if (other._a != _a)
                return false;
            if (other._b != _b)
                return false;
            if (other._c != _c)
                return false;
            if (other._d != _d)
                return false;
            if (other._e != _e)
                return false;
            if (other._f != _f)
                return false;
            if (other._g != _g)
                return false;
            if (other._h != _h)
                return false;
            if (other._i != _i)
                return false;
            if (other._j != _j)
                return false;
            if (other._k != _k)
                return false;

            return true;
        }

        public int CompareTo(Guid value)
        {
            if (value._a != _a)
                return _a < value._a ? -1 : 1;
            if (value._b != _b)
                return _b < value._b ? -1 : 1;
            if (value._c != _c)
                return _c < value._c ? -1 : 1;
            if (value._d != _d)
                return _d < value._d ? -1 : 1;
            if (value._e != _e)
                return _e < value._e ? -1 : 1;
            if (value._f != _f)
                return _f < value._f ? -1 : 1;
            if (value._g != _g)
                return _g < value._g ? -1 : 1;
            if (value._h != _h)
                return _h < value._h ? -1 : 1;
            if (value._i != _i)
                return _i < value._i ? -1 : 1;
            if (value._j != _j)
                return _j < value._j ? -1 : 1;
            if (value._k != _k)
                return _k < value._k ? -1 : 1;

            return 0;
        }

        public static bool operator==(Guid a, Guid b)
        {
            if (a._a != b._a)
                return false;
            if (a._b != b._b)
                return false;
            if (a._c != b._c)
                return false;
            if (a._d != b._d)
                return false;
            if (a._e != b._e)
                return false;
            if (a._f != b._f)
                return false;
            if (a._g != b._g)
                return false;
            if (a._h != b._h)
                return false;
            if (a._i != b._i)
                return false;
            if (a._j != b._j)
                return false;
            if (a._k != b._k)
                return false;

            return true;
        }

        public static bool operator!=(Guid a, Guid b)
        {
            return !(a == b);
        }

        [System.Security.SecuritySafeCritical]
        public static Guid NewGuid()
        {
            throw new NotImplementedException();
        }
    }

    public static class Nullable
    {
        public static bool Equals<T>(T? n1, T? n2) where T : struct
        {
            if (n1.has_value != n2.has_value)
                return false;

            if (!n1.has_value)
                return true;

            return System.Collections.Generic.EqualityComparer<T>.Default.Equals(n1.value, n2.value);
        }
    }

    public struct Nullable<T> where T : struct
    {
        #region Sync with runtime code
        internal T value;
        internal bool has_value;
        #endregion

        public Nullable(T value)
        {
            this.has_value = true;
            this.value = value;
        }

        public bool HasValue
        {
            get { return has_value; }
        }

        public T Value
        {
            get
            {
                if (!has_value)
                    throw new InvalidOperationException("Nullable object must have a value.");

                return value;
            }
        }

        public override bool Equals(object other)
        {
            if (other == null)
                return has_value == false;
            if (!(other is Nullable<T>))
                return false;

            return Equals((Nullable<T>)other);
        }

        bool Equals(Nullable<T> other)
        {
            if (other.has_value != has_value)
                return false;

            if (has_value == false)
                return true;

            return other.value.Equals(value);
        }

        public override int GetHashCode()
        {
            if (!has_value)
                return 0;

            return value.GetHashCode();
        }

        public T GetValueOrDefault()
        {
            return value;
        }

        public T GetValueOrDefault(T defaultValue)
        {
            return has_value ? value : defaultValue;
        }

        public override string ToString()
        {
            if (has_value)
                return value.ToString();
            else
                return String.Empty;
        }

        public static implicit operator Nullable<T>(T value)
        {
            return new Nullable<T>(value);
        }

        public static explicit operator T(Nullable<T> value)
        {
            return value.Value;
        }
    }
}

namespace System.Diagnostics
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ConditionalAttribute : Attribute
    {
        private string _conditionString;

        public string ConditionString => _conditionString;

        public ConditionalAttribute(string conditionString)
        {
            _conditionString = conditionString;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
        AttributeTargets.Constructor |
        AttributeTargets.Method, Inherited = false)]
    public sealed class DebuggerStepThroughAttribute : Attribute
    {
        public DebuggerStepThroughAttribute()
        {
        }
    }

    public class DebuggerDisplayAttribute : Attribute
    {
        public DebuggerDisplayAttribute(string display)
        {
        }
    }

#if IL2CPP_MONO_DEBUGGER

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
    public sealed class DebuggerHiddenAttribute : Attribute
    {
    }

#endif

    public sealed class DebuggerTypeProxyAttribute : Attribute
    {
        public DebuggerTypeProxyAttribute(Type t)
        {
        }
    }

    public sealed class DebuggableAttribute : Attribute
    {
        public enum DebuggingModes
        {
            None = 0x0,
            Default = 0x1,
            DisableOptimizations = 0x100,
            IgnoreSymbolStoreSequencePoints = 0x2,
            EnableEditAndContinue = 0x4
        }

        private DebuggingModes m_DebuggingModes;

        public DebuggableAttribute(bool isJITTrackingEnabled, bool isJITOptimizerDisabled)
        {
            m_DebuggingModes = 0;

            if (isJITTrackingEnabled)
            {
                m_DebuggingModes |= DebuggingModes.Default;
            }

            if (isJITOptimizerDisabled)
            {
                m_DebuggingModes |= DebuggingModes.DisableOptimizations;
            }
        }

        public DebuggableAttribute(DebuggingModes modes)
        {
            m_DebuggingModes = modes;
        }

        public bool IsJITTrackingEnabled
        {
            get { return ((m_DebuggingModes & DebuggingModes.Default) != 0); }
        }

        public bool IsJITOptimizerDisabled
        {
            get { return ((m_DebuggingModes & DebuggingModes.DisableOptimizations) != 0); }
        }

        public DebuggingModes DebuggingFlags
        {
            get { return m_DebuggingModes; }
        }
    }

    public sealed class Debugger
    {
        Debugger() {}

        public static bool IsAttached
        {
            get
            {
                return IsAttached_internal();
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static bool IsAttached_internal();

        public static void Break()
        {
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsLogging();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Log(int level, string category, string message);
    }

    public class Stopwatch
    {
        [DllImport("__Internal")]
        private static extern long Stopwatch_GetTimestamp();

        public static readonly long Frequency = 10000000;

        public static readonly bool IsHighResolution = true;

        public static Stopwatch StartNew()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            return s;
        }

        long elapsed;
        long started;
        bool is_running;

        public TimeSpan Elapsed
        {
            get
            {
                if (IsHighResolution)
                {
                    // convert our ticks to TimeSpace ticks, 100 nano second units
                    // using two divisions helps avoid overflow
                    return TimeSpan.FromTicks((long)(ElapsedTicks / (Frequency / TimeSpan.TicksPerSecond)));
                }
                else
                {
                    return TimeSpan.FromTicks(ElapsedTicks);
                }
            }
        }

        public long ElapsedMilliseconds
        {
            get
            {
                checked {
                    if (IsHighResolution)
                    {
                        return (long)(ElapsedTicks / (Frequency / 1000));
                    }
                    else
                    {
                        return (long)Elapsed.TotalMilliseconds;
                    }
                }
            }
        }

        public long ElapsedTicks
        {
            get { return is_running ? Stopwatch_GetTimestamp() - started + elapsed : elapsed; }
        }

        public bool IsRunning
        {
            get { return is_running; }
        }

        public void Reset()
        {
            elapsed = 0;
            is_running = false;
        }

        public void Start()
        {
            if (is_running)
                return;
            started = Stopwatch_GetTimestamp();
            is_running = true;
        }

        public void Stop()
        {
            if (!is_running)
                return;
            elapsed += Stopwatch_GetTimestamp() - started;
            if (elapsed < 0)
                elapsed = 0;
            is_running = false;
        }

        public void Restart()
        {
            started = Stopwatch_GetTimestamp();
            elapsed = 0;
            is_running = true;
        }
    }
}

namespace System.Runtime.ConstrainedExecution
{
    [Serializable]
    public enum Consistency
    {
        MayCorruptProcess,
        MayCorruptAppDomain,
        MayCorruptInstance,
        WillNotCorruptState,
    }

    [Serializable]
    public enum Cer
    {
        None,
        MayFail,
        Success,
    }

    public sealed class ReliabilityContractAttribute : Attribute
    {
        private Consistency _consistency;
        private Cer _cer;

        public ReliabilityContractAttribute(Consistency consistencyGuarantee, Cer cer)
        {
            this._consistency = consistencyGuarantee;
            this._cer = cer;
        }

        public Consistency ConsistencyGuarantee
        {
            get
            {
                return this._consistency;
            }
        }

        public Cer Cer
        {
            get
            {
                return this._cer;
            }
        }
    }
}

namespace System.Runtime.CompilerServices
{
    public static class RuntimeHelpers
    {
        public extern static int OffsetToStringData
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static unsafe extern void MemoryCopy(void* destination, void* source, int size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static unsafe extern int MemoryCompare(void* left, void* right, int size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern int MemCmpRef<T>(ref T a, ref T b);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern int MemHashRef<T>(ref T a);
    }

    public static class IsVolatile
    {
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
    public sealed class MethodImplAttribute : Attribute
    {
        public MethodCodeType MethodCodeType;

        public MethodImplAttribute(MethodImplOptions methodImplOptions)
        {
            Value = methodImplOptions;
        }

        public MethodImplOptions Value { get; }
    }

    public enum MethodImplOptions
    {
        Unmanaged = 4,
        NoInlining = 8,
        ForwardRef = 16,
        Synchronized = 32,
        NoOptimization = 64,
        PreserveSig = 128,
        AggressiveInlining = 256,
        InternalCall = 4096
    }

    public enum MethodCodeType
    {
        IL = 0,
        Native = 1,
        OPTIL = 2,
        Runtime = 3
    }

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ExtensionAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public sealed class CompilerGeneratedAttribute : Attribute
    {
        public CompilerGeneratedAttribute() {}
    }

    public enum CompilationRelaxations : int
    {
        NoStringInterning = 0x0008,
    };

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Method)]
    public class CompilationRelaxationsAttribute : Attribute
    {
        private int m_Relaxations;

        public CompilationRelaxationsAttribute(int relaxations)
        {
            m_Relaxations = relaxations;
        }

        public CompilationRelaxationsAttribute(CompilationRelaxations relaxations)
        {
            m_Relaxations = (int)relaxations;
        }

        public int CompilationRelaxations
        {
            get
            {
                return m_Relaxations;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class RuntimeCompatibilityAttribute : Attribute
    {
        private bool m_WrapNonExceptionThrows;

        public RuntimeCompatibilityAttribute()
        {
        }

        public bool WrapNonExceptionThrows
        {
            get
            {
                return m_WrapNonExceptionThrows;
            }
            set
            {
                m_WrapNonExceptionThrows = value;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public sealed class IndexerNameAttribute : Attribute
    {
        public IndexerNameAttribute(String indexerName)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class InternalsVisibleToAttribute : Attribute
    {
        private string _assemblyName;
        private bool _allInternalsVisible = true;

        public InternalsVisibleToAttribute(string assemblyName)
        {
            this._assemblyName = assemblyName;
        }

        public string AssemblyName
        {
            get
            {
                return _assemblyName;
            }
        }

        public bool AllInternalsVisible
        {
            get { return _allInternalsVisible; }
            set { _allInternalsVisible = value; }
        }
    }
}

namespace System.Runtime.InteropServices
{
    public sealed class DllImportAttribute : Attribute
    {
        internal String _val;

        public DllImportAttribute(String dllName)
        {
            _val = dllName;
        }

        public String Value { get { return _val; } }

        public String EntryPoint;
        public CharSet CharSet;
        public bool SetLastError;
        public bool ExactSpelling;
        public bool PreserveSig;
        public CallingConvention CallingConvention;
        public bool BestFitMapping;
        public bool ThrowOnUnmappableChar;
    }

    public sealed class FieldOffsetAttribute : Attribute
    {
        internal int _value;

        public FieldOffsetAttribute(int offset)
        {
            _value = offset;
        }

        public int Value { get { return _value; } }
    }

    public sealed class UnmanagedFunctionPointerAttribute : Attribute
    {
        CallingConvention m_callingConvention;

        public UnmanagedFunctionPointerAttribute(CallingConvention callingConvention) { m_callingConvention = callingConvention; }

        public CallingConvention CallingConvention { get { return m_callingConvention; } }

        public CharSet CharSet;
        public bool BestFitMapping;
        public bool ThrowOnUnmappableChar;

        // This field is ignored and marshaling behaves as if it was true (for historical reasons).
        public bool SetLastError;
    }

    public sealed class InAttribute : Attribute
    {
        public InAttribute()
        {
        }
    }

    public sealed class OutAttribute : Attribute
    {
        public OutAttribute()
        {
        }
    }

    public unsafe sealed class MarshalAsAttribute : Attribute
    {
        internal UnmanagedType _val;

        public MarshalAsAttribute(UnmanagedType unmanagedType)
        {
            _val = unmanagedType;
        }

        public MarshalAsAttribute(short unmanagedType)
        {
            _val = (UnmanagedType)unmanagedType;
        }

        public UnmanagedType Value { get { return _val; } }

        // Fields used with SubType = SafeArray.
        public VarEnum SafeArraySubType;
        public Type SafeArrayUserDefinedSubType;

        // Field used with iid_is attribute (interface pointers).
        public int IidParameterIndex;

        // Fields used with SubType = ByValArray and LPArray.
        // Array size =  parameter(PI) * PM + C
        public UnmanagedType ArraySubType;
        public short SizeParamIndex;           // param index PI
        public int SizeConst;                // constant C

        // Fields used with SubType = CustomMarshaler
        public String MarshalType;              // Name of marshaler class
        public Type MarshalTypeRef;           // Type of marshaler class
        public String MarshalCookie;            // cookie to pass to marshaler
    }

    public enum UnmanagedType
    {
        Bool = 2,
        I1 = 3,
        U1 = 4,
        I2 = 5,
        U2 = 6,
        I4 = 7,
        U4 = 8,
        I8 = 9,
        U8 = 10,
        R4 = 11,
        R8 = 12,
        Currency = 15,
        BStr = 19,
        LPStr = 20,
        LPWStr = 21,
        LPTStr = 22,
        ByValTStr = 23,
        IUnknown = 25,
        IDispatch = 26,
        Struct = 27,
        Interface = 28,
        SafeArray = 29,
        ByValArray = 30,
        SysInt = 31,
        SysUInt = 32,
        VBByRefStr = 34,
        AnsiBStr = 35,
        TBStr = 36,
        VariantBool = 37,
        FunctionPtr = 38,
        AsAny = 40,
        LPArray = 42,
        LPStruct = 43,
        CustomMarshaler = 44,
        Error = 45
    }

    public enum VarEnum
    {
        VT_EMPTY = 0,
        VT_NULL = 1,
        VT_I2 = 2,
        VT_I4 = 3,
        VT_R4 = 4,
        VT_R8 = 5,
        VT_CY = 6,
        VT_DATE = 7,
        VT_BSTR = 8,
        VT_DISPATCH = 9,
        VT_ERROR = 10,
        VT_BOOL = 11,
        VT_VARIANT = 12,
        VT_UNKNOWN = 13,
        VT_DECIMAL = 14,
        VT_I1 = 16,
        VT_UI1 = 17,
        VT_UI2 = 18,
        VT_UI4 = 19,
        VT_I8 = 20,
        VT_UI8 = 21,
        VT_INT = 22,
        VT_UINT = 23,
        VT_VOID = 24,
        VT_HRESULT = 25,
        VT_PTR = 26,
        VT_SAFEARRAY = 27,
        VT_CARRAY = 28,
        VT_USERDEFINED = 29,
        VT_LPSTR = 30,
        VT_LPWSTR = 31,
        VT_RECORD = 36,
        VT_FILETIME = 64,
        VT_BLOB = 65,
        VT_STREAM = 66,
        VT_STORAGE = 67,
        VT_STREAMED_OBJECT = 68,
        VT_STORED_OBJECT = 69,
        VT_BLOB_OBJECT = 70,
        VT_CF = 71,
        VT_CLSID = 72,
        VT_VECTOR = 4096,
        VT_ARRAY = 8192,
        VT_BYREF = 16384
    }

    public enum CallingConvention
    {
        Winapi = 1,
        Cdecl = 2,
        StdCall = 3,
        ThisCall = 4,
        FastCall = 5
    }

    public enum LayoutKind
    {
        Sequential = 0,
        Explicit = 2,
        Auto = 3
    }

    public enum CharSet
    {
        None = 1,
        Ansi = 2,
        Unicode = 3,
        Auto = 4
    }

    public unsafe sealed class StructLayoutAttribute : Attribute
    {
        private const int DEFAULT_PACKING_SIZE = 8;
        internal LayoutKind _val;

        internal StructLayoutAttribute(LayoutKind layoutKind, int pack, int size, CharSet charSet)
        {
            _val = layoutKind;
            Pack = pack;
            Size = size;
            CharSet = charSet;
        }

        public StructLayoutAttribute(LayoutKind layoutKind)
        {
            _val = layoutKind;
        }

        public StructLayoutAttribute(short layoutKind)
        {
            _val = (LayoutKind)layoutKind;
        }

        public LayoutKind Value { get { return _val; } }
        public int Pack;
        public int Size;
        public CharSet CharSet;
    }

    public static class Marshal
    {
        [CompilerServices.MethodImpl(CompilerServices.MethodImplOptions.InternalCall)]
        public static extern string PtrToStringAnsi(IntPtr ptr);

        [CompilerServices.MethodImpl(CompilerServices.MethodImplOptions.InternalCall)]
        public static extern IntPtr StringToCoTaskMemAnsi(string str);

        [CompilerServices.MethodImpl(CompilerServices.MethodImplOptions.InternalCall)]
        public static extern void FreeCoTaskMem(IntPtr ptr);

        [CompilerServices.MethodImpl(CompilerServices.MethodImplOptions.InternalCall)]
        public static extern IntPtr GetFunctionPointerForDelegate(Delegate d);

        [CompilerServices.MethodImpl(CompilerServices.MethodImplOptions.InternalCall)]
        public static extern int SizeOf<T>();
    }

    public enum GCHandleType
    {
        Weak,
        WeakTrackResurrection,
        Normal,
        Pinned
    }

    public struct GCHandle
    {
        // fields
        private int handle;

        private GCHandle(IntPtr h)
        {
            handle = (int)h;
        }

        // Constructors
        private GCHandle(object obj)
            : this(obj, GCHandleType.Normal)
        {}

        internal GCHandle(object value, GCHandleType type)
        {
            // MS does not crash/throw on (most) invalid GCHandleType values (except -1)
            if ((type < GCHandleType.Weak) || (type > GCHandleType.Normal))
                type = GCHandleType.Normal;
            handle = GetTargetHandle(value, 0, type);
        }

        // Properties

        public bool IsAllocated
        {
            get
            {
                return (handle != 0);
            }
        }

        public object Target
        {
            get
            {
                if (!IsAllocated)
                    throw new InvalidOperationException("Handle is not allocated");
                return GetTarget(handle);
            }
            set
            {
                handle = GetTargetHandle(value, handle, (GCHandleType)(-1));
            }
        }

        public static System.Runtime.InteropServices.GCHandle Alloc(object value)
        {
            return new GCHandle(value);
        }

        public static System.Runtime.InteropServices.GCHandle Alloc(object value, GCHandleType type)
        {
            return new GCHandle(value, type);
        }

        public void Free()
        {
            // Copy the handle instance member to a local variable. This is required to prevent
            // race conditions releasing the handle.
            int local_handle = handle;

            // Free the handle if it hasn't already been freed.
            if (local_handle != 0)
            {
                //This is an atomic operation in regular mscorlib...if we add threads we'll need to do that
                handle = 0;
                FreeHandle(local_handle);
            }
            else
            {
                throw new InvalidOperationException("Handle is not initialized.");
            }
        }

        public static explicit operator IntPtr(GCHandle value)
        {
            return (IntPtr)value.handle;
        }

        public static explicit operator GCHandle(IntPtr value)
        {
            unsafe {
                if (value.ToPointer() == (void*)0)
                    throw new InvalidOperationException("GCHandle value cannot be zero");
            }
            return new GCHandle(value);
        }

        [Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        private extern static object GetTarget(int handle);

        [Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        private extern static int GetTargetHandle(object obj, int handle, GCHandleType type);

        [Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.InternalCall)]
        private extern static void FreeHandle(int handle);

        public static GCHandle FromIntPtr(IntPtr value)
        {
            return (GCHandle)value;
        }

        public static IntPtr ToIntPtr(GCHandle value)
        {
            return (IntPtr)value;
        }

        public IntPtr AddrOfPinnedObject()
        {
            throw new NotImplementedException();
        }
    }
}

namespace System.Security
{
    [AttributeUsage(AttributeTargets.Module, AllowMultiple = true, Inherited = false)]
    sealed public class UnverifiableCodeAttribute : Attribute
    {
    }

    public sealed class SecuritySafeCriticalAttribute : Attribute
    {
    }

    public sealed class SecurityCriticalAttribute : Attribute
    {
    }
}

namespace System.Security.Permissions
{
    public enum SecurityAction
    {
        Demand = 2,
        Assert = 3,
        Deny = 4,
        PermitOnly = 5,
        LinkDemand = 6,
        InheritanceDemand = 7,
        RequestMinimum = 8,
        RequestOptional = 9,
        RequestRefuse = 10,
    }

    public abstract class SecurityAttribute : Attribute
    {
        private SecurityAction _action;

        public SecurityAttribute(SecurityAction action)
        {
            _action = action;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public abstract class CodeAccessSecurityAttribute : SecurityAttribute
    {
        public CodeAccessSecurityAttribute(SecurityAction action)
            : base(action)
        {
        }
    }
    public enum SecurityPermissionFlag
    {
        NoFlags = 0x00,
        Assertion = 0x01,
        UnmanagedCode = 0x02,
        SkipVerification = 0x04,
        Execution = 0x08,
        ControlThread = 0x10,
        ControlEvidence = 0x20,
        ControlPolicy = 0x40,
        SerializationFormatter = 0x80,
        ControlDomainPolicy = 0x100,
        ControlPrincipal = 0x200,
        ControlAppDomain = 0x400,
        RemotingConfiguration = 0x800,
        Infrastructure = 0x1000,
        BindingRedirects = 0x2000,
        AllFlags = 0x3fff,
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    sealed public class SecurityPermissionAttribute : CodeAccessSecurityAttribute
    {
        private SecurityPermissionFlag _flags;

        public SecurityPermissionAttribute(SecurityAction action)
            : base(action)
        {
        }

        public SecurityPermissionFlag Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Assertion
        {
            get { return (_flags & SecurityPermissionFlag.Assertion) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.Assertion : _flags & ~SecurityPermissionFlag.Assertion; }
        }

        public bool UnmanagedCode
        {
            get { return (_flags & SecurityPermissionFlag.UnmanagedCode) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.UnmanagedCode : _flags & ~SecurityPermissionFlag.UnmanagedCode; }
        }

        public bool SkipVerification
        {
            get { return (_flags & SecurityPermissionFlag.SkipVerification) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.SkipVerification : _flags & ~SecurityPermissionFlag.SkipVerification; }
        }

        public bool Execution
        {
            get { return (_flags & SecurityPermissionFlag.Execution) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.Execution : _flags & ~SecurityPermissionFlag.Execution; }
        }

        public bool ControlThread
        {
            get { return (_flags & SecurityPermissionFlag.ControlThread) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.ControlThread : _flags & ~SecurityPermissionFlag.ControlThread; }
        }

        public bool ControlEvidence
        {
            get { return (_flags & SecurityPermissionFlag.ControlEvidence) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.ControlEvidence : _flags & ~SecurityPermissionFlag.ControlEvidence; }
        }

        public bool ControlPolicy
        {
            get { return (_flags & SecurityPermissionFlag.ControlPolicy) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.ControlPolicy : _flags & ~SecurityPermissionFlag.ControlPolicy; }
        }

        public bool SerializationFormatter
        {
            get { return (_flags & SecurityPermissionFlag.SerializationFormatter) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.SerializationFormatter : _flags & ~SecurityPermissionFlag.SerializationFormatter; }
        }

        public bool ControlDomainPolicy
        {
            get { return (_flags & SecurityPermissionFlag.ControlDomainPolicy) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.ControlDomainPolicy : _flags & ~SecurityPermissionFlag.ControlDomainPolicy; }
        }

        public bool ControlPrincipal
        {
            get { return (_flags & SecurityPermissionFlag.ControlPrincipal) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.ControlPrincipal : _flags & ~SecurityPermissionFlag.ControlPrincipal; }
        }

        public bool ControlAppDomain
        {
            get { return (_flags & SecurityPermissionFlag.ControlAppDomain) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.ControlAppDomain : _flags & ~SecurityPermissionFlag.ControlAppDomain; }
        }

        public bool RemotingConfiguration
        {
            get { return (_flags & SecurityPermissionFlag.RemotingConfiguration) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.RemotingConfiguration : _flags & ~SecurityPermissionFlag.RemotingConfiguration; }
        }

        public bool Infrastructure
        {
            get { return (_flags & SecurityPermissionFlag.Infrastructure) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.Infrastructure : _flags & ~SecurityPermissionFlag.Infrastructure; }
        }

        public bool BindingRedirects
        {
            get { return (_flags & SecurityPermissionFlag.BindingRedirects) != 0; }
            set { _flags = value ? _flags | SecurityPermissionFlag.BindingRedirects : _flags & ~SecurityPermissionFlag.BindingRedirects; }
        }
    }
}

namespace System.Reflection
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class DefaultMemberAttribute : Attribute
    {
        private String m_memberName;

        public DefaultMemberAttribute(String memberName)
        {
            m_memberName = memberName;
        }

        public String MemberName
        {
            get { return m_memberName; }
        }
    }

#if IL2CPP_MONO_DEBUGGER

    internal class MonoAssembly
    {
        object assembly;
        object resolve_event_holder;
        object evidence;
        object minimum;
        object optional;
        object refuse;
        object granted;
        object denied;
        bool from_byte_array;
        string name;
    }

#endif
}

namespace System.ComponentModel
{
    public enum EditorBrowsableState
    {
        Always,
        Never,
        Advanced,
    }

    public sealed class EditorBrowsableAttribute : Attribute
    {
        public EditorBrowsableAttribute(EditorBrowsableState state)
        {
            State = state;
        }

        public EditorBrowsableAttribute() : this(EditorBrowsableState.Always)
        {
        }

        public EditorBrowsableState State { get; }
    }
}


namespace System
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
        AttributeTargets.Enum | AttributeTargets.Constructor |
        AttributeTargets.Method | AttributeTargets.Property |
        AttributeTargets.Field | AttributeTargets.Event |
        AttributeTargets.Interface | AttributeTargets.Delegate,
        Inherited = false)]
    [Serializable]
    public sealed class ObsoleteAttribute : Attribute
    {
        private string _message;
        private bool _error;

        //   Constructors
        public ObsoleteAttribute()
            : base()
        {
        }

        public ObsoleteAttribute(string message)
        {
            _message = message;
        }

        public ObsoleteAttribute(string message, bool error)
        {
            _message = message;
            _error = error;
        }

        // Properties
        public string Message
        {
            get { return _message; }
        }

        public bool IsError
        {
            get { return _error; }
        }
    }

    [CLSCompliant(false)]
    public struct TypedReference
    {
#pragma warning disable 169
        RuntimeTypeHandle type;
        private IntPtr Value;
        private IntPtr Type;
#pragma warning restore

/*
        public unsafe static Object ToObject(TypedReference value)
        {
            return InternalToObject(&value);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal unsafe extern static Object InternalToObject(void * value);
*/
    }
}

namespace System.Threading
{
    public struct SpinLock
    {
        public void Enter(ref bool lockTaken)
        {
            lockTaken = true;
        }

        public void Exit(bool useMemoryBarrier)
        {
        }
    }

    public static class Monitor
    {
        public static void Enter(Object obj)
        {
        }

        public static void Enter(Object obj, ref bool lockTaken)
        {
            lockTaken = true;
        }

        public static void Exit(Object obj)
        {
        }
    }

#if IL2CPP_MONO_DEBUGGER
    [StructLayout(LayoutKind.Sequential)]
    sealed class InternalThread
    {
        int lock_thread_id;
        IntPtr handle;
        IntPtr native_handle;
        IntPtr unused3;
        private IntPtr name;
        private int name_len;
        private uint state;
        private object abort_exc;
        private int abort_state_handle;
        internal Int64 thread_id;
        private IntPtr debugger_thread;
        private UIntPtr static_data;
        private IntPtr runtime_thread_info;
        private object current_appcontext;
        private object root_domain_thread;
        internal byte[] _serialized_principal;
        internal int _serialized_principal_version;
        private IntPtr appdomain_refs;
        private int interruption_requested;
        private IntPtr synch_cs;
        internal bool threadpool_thread;
        private bool thread_interrupt_requested;
        internal int stack_size;
        internal byte apartment_state;
        internal volatile int critical_region_level;
        internal int managed_id;
        private int small_id;
        private IntPtr manage_callback;
        private IntPtr unused4;
        private IntPtr flags;
        private IntPtr thread_pinning_ref;
        private IntPtr abort_protected_block_count;
        private int priority;
        private IntPtr owned_mutex;
        private IntPtr suspended_event;
        private int self_suspended;
        private IntPtr unused1;
        private IntPtr unused2;
        private IntPtr last;
    }

    [StructLayout(LayoutKind.Sequential)]
    public sealed partial class Thread
    {
        private InternalThread internal_thread;
        object m_ThreadStartArg;
        object pending_exception;
        /*IPrincipal*/ object principal;
        int principal_version;
        private MulticastDelegate m_Delegate;
        private /*ExecutionContext*/ object m_ExecutionContext;
        private bool m_ExecutionContextBelongsToOuterScope;
    }
#endif
}


namespace System.Reflection
{
    public abstract class MemberInfo
    {
    }

    public static class CustomAttributeExtensions
    {
        public static Attribute GetCustomAttribute(this MemberInfo element, Type attributeType) => null;
    }
}

namespace System.Linq
{
    internal struct Dummy
    {
    }
}

namespace System
{
    public class Activator
    {
        // This is specifically not implemented anywhere; it's needed for Roslyn,
        // and it's marked as Unmanaged so that il2cpp doesn't complain about icalls
        // with generic parameters.
        [MethodImplAttribute(MethodImplOptions.Unmanaged)]
        public extern static T CreateInstance<T>();

        Activator() {}
    }
}
