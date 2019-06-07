using System;
using System.IO;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Serialization.Json
{
    [Flags]
    internal enum JsonType
    {
        Undefined = 1 << 0,
        BeginObject = 1 << 1, // '{'
        EndObject = 1 << 2, // '}'
        BeginArray = 1 << 3, // '['
        EndArray = 1 << 4, // ']'
        MemberSeparator = 1 << 5, // ':'
        ValueSeparator = 1 << 6, // ','
        String = 1 << 7, // '"'..'".
        Number = 1 << 8, // '0'..'9', 'e', 'E', '-'
        True = 1 << 9, // 'true'
        False = 1 << 10, // 'false'
        Null = 1 << 11, // 'null'
        EOF = 1 << 12,

        // Any value type
        Value = BeginObject | BeginArray | String | Number | True | False | Null
    }

    internal struct JsonValidationResult
    {
        /// <summary>
        /// The type that was expected by the validator.
        /// </summary>
        public JsonType ExpectedType;

        /// <summary>
        /// The type that the validator stopped at.
        /// </summary>
        public JsonType ActualType;

        /// <summary>
        /// The character that the validator stopped at.
        /// </summary>
        public char Char;

        /// <summary>
        /// The line the validator stopped at.
        /// </summary>
        public int LineCount;

        /// <summary>
        /// The char (on the line) the validator stopped at.
        /// </summary>
        public int CharCount;

        public bool IsValid()
        {
            return (ActualType & ExpectedType) == ActualType;
        }

        public override string ToString()
        {
            var actualChar = Char == '\0' ? "\\0" : Char.ToString();
            var isValid = IsValid() ? "valid" : "invalid";
            return $"Input json was {isValid}. {nameof(ExpectedType)}=[{ExpectedType}] {nameof(ActualType)}=[{ActualType}] ActualChar=['{actualChar}'] at Line=[{LineCount}] at Character=[{CharCount}]";
        }
    }

    internal unsafe class JsonStandardValidator : IJsonValidator, IDisposable
    {
        private const int k_ResultSuccess = 0;
        private const int k_ResultEndOfStream = -1;
        private const int k_ResultInvalidJson = -2;
        private const int k_DefaultDepthLimit = 128;

        private struct ValidationJobData
        {
            public int CharBufferPosition;
            public JsonTypeStack Stack;
            public ushort PrevChar;
            public JsonType Expected;
            public JsonType Actual;
            public int LineCount;
            public int LineStart;
            public int CharCount;
            public ushort Char;
            public JsonType PartialTokenType;
            public int PartialTokenState;
        }

        private struct JsonTypeStack : IDisposable
        {
            private readonly Allocator m_Label;
            [NativeDisableUnsafePtrRestriction] private JsonType* m_Stack;
            private int m_Length;
            private int m_Position;

            public JsonTypeStack(int length, Allocator label)
            {
                m_Label = label;
                m_Stack = (JsonType*) UnsafeUtility.Malloc(length * sizeof(JsonType), UnsafeUtility.AlignOf<JsonType>(), label);
                m_Length = length;
                m_Position = -1;
            }

            public void Push(JsonType type)
            {
                if (m_Position + 1 >= m_Length)
                {
                    Resize(m_Length * 2);
                }

                m_Stack[++m_Position] = type;
            }

            public void Pop()
            {
                m_Position--;
            }

            public JsonType Peek()
            {
                return m_Position < 0 ? JsonType.Undefined : m_Stack[m_Position];
            }

            public void Clear()
            {
                m_Position = -1;
            }

            private void Resize(int length)
            {
                var buffer = UnsafeUtility.Malloc(length * sizeof(JsonType), UnsafeUtility.AlignOf<JsonType>(), m_Label);
                UnsafeUtility.MemCpy(buffer, m_Stack, m_Length * sizeof(JsonType));
                UnsafeUtility.Free(m_Stack, m_Label);
                m_Stack = (JsonType*) buffer;
                m_Length = length;
            }

            public void Dispose()
            {
                UnsafeUtility.Free(m_Stack, m_Label);
                m_Stack = null;
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct StandardJsonValidationJob : IJob
        {
            [NativeDisableUnsafePtrRestriction] public ValidationJobData* Data;

            [NativeDisableUnsafePtrRestriction] public ushort* CharBuffer;
            public int CharBufferLength;

            private int m_CharBufferPosition;
            private JsonTypeStack m_Stack;
            private ushort m_PrevChar;
            private JsonType m_Expected;
            private int m_LineCount;
            private int m_LineStart;
            private JsonType m_PartialTokenType;
            private int m_PartialTokenState;

            private void Break(JsonType actual)
            {
                var charCount = m_CharBufferPosition - m_LineStart;

                // Copy back locals to data ptr
                Data->CharBufferPosition = m_CharBufferPosition;
                Data->Stack = m_Stack;
                Data->PrevChar = m_PrevChar;
                Data->Expected = m_Expected;
                Data->Actual = actual;
                Data->LineCount = m_LineCount;
                Data->LineStart = -charCount;
                Data->CharCount = charCount;
                Data->Char = m_CharBufferPosition < CharBufferLength ? CharBuffer[m_CharBufferPosition] : '\0';
                Data->PartialTokenType = m_PartialTokenType;
                Data->PartialTokenState = m_PartialTokenState;
            }

            public void Execute()
            {
                // Copy to locals from data ptr
                m_CharBufferPosition = Data->CharBufferPosition;
                m_Stack = Data->Stack;
                m_PrevChar = Data->PrevChar;
                m_Expected = Data->Expected;
                m_LineCount = Data->LineCount;
                m_LineStart = Data->LineStart;
                m_PartialTokenType = Data->PartialTokenType;
                m_PartialTokenState = Data->PartialTokenState;

                switch (m_PartialTokenType)
                {
                    case JsonType.String:
                    {
                        var result = ReadString();
                        if (result != k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.String;
                            m_PartialTokenState = m_PartialTokenState + m_CharBufferPosition;
                            Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                            return;
                        }

                        m_PartialTokenType = JsonType.Undefined;
                        m_PartialTokenState = 0;
                        m_CharBufferPosition++;
                    }
                        break;

                    case JsonType.Number:
                    {
                        var state = m_PartialTokenState;
                        var result = ReadNumber(ref state);

                        if (result != k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.Number;
                            m_PartialTokenState = state;
                            Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                            return;
                        }

                        m_PartialTokenType = JsonType.Undefined;
                        m_PartialTokenState = 0;
                    }
                        break;

                    case JsonType.True:
                    {
                        var result = ReadTrue(m_PartialTokenState);
                        if (result != k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.True;
                            m_PartialTokenState += m_CharBufferPosition;
                            Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                            return;
                        }

                        m_PartialTokenType = JsonType.Undefined;
                        m_PartialTokenState = 0;
                    }
                        break;

                    case JsonType.False:
                    {
                        var result = ReadFalse(m_PartialTokenState);
                        if (result != k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.False;
                            m_PartialTokenState += m_CharBufferPosition;
                            Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                            return;
                        }


                        m_PartialTokenType = JsonType.Undefined;
                        m_PartialTokenState = 0;
                    }
                        break;

                    case JsonType.Null:
                    {
                        var result = ReadNull(m_PartialTokenState);
                        if (result != k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.Null;
                            m_PartialTokenState += m_CharBufferPosition;
                            Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                            return;
                        }

                        m_PartialTokenType = JsonType.Undefined;
                        m_PartialTokenState = 0;
                    }
                        break;
                }

                while (m_CharBufferPosition < CharBufferLength)
                {
                    var c = CharBuffer[m_CharBufferPosition];

                    switch (c)
                    {
                        case '{':
                        {
                            if (!IsExpected(JsonType.BeginObject))
                            {
                                Break(JsonType.BeginObject);
                                return;
                            }

                            m_Stack.Push(JsonType.BeginObject);
                            m_Expected = JsonType.String | JsonType.EndObject;
                        }
                            break;

                        case '[':
                        {
                            if (!IsExpected(JsonType.BeginArray))
                            {
                                Break(JsonType.BeginArray);
                                return;
                            }

                            m_Stack.Push(JsonType.BeginArray);
                            m_Expected = JsonType.Value | JsonType.EndArray;
                        }
                            break;

                        case '}':
                        {
                            if (!IsExpected(JsonType.EndObject))
                            {
                                Break(JsonType.EndObject);
                                return;
                            }

                            m_Stack.Pop();

                            if (m_Stack.Peek() == JsonType.String)
                            {
                                m_Stack.Pop();
                            }

                            switch (m_Stack.Peek())
                            {
                                case JsonType.BeginObject:
                                    m_Expected = JsonType.ValueSeparator | JsonType.EndObject;
                                    break;
                                case JsonType.BeginArray:
                                    m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                                    break;
                                default:
                                    m_Expected = JsonType.EOF;
                                    break;
                            }
                        }
                            break;

                        case ']':
                        {
                            if (!IsExpected(JsonType.EndArray))
                            {
                                Break(JsonType.EndArray);
                                return;
                            }

                            m_Stack.Pop();

                            if (m_Stack.Peek() == JsonType.String)
                            {
                                m_Stack.Pop();
                            }

                            switch (m_Stack.Peek())
                            {
                                case JsonType.BeginObject:
                                    m_Expected = JsonType.ValueSeparator | JsonType.EndObject;
                                    break;
                                case JsonType.BeginArray:
                                    m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                                    break;
                                default:
                                    m_Expected = JsonType.EOF;
                                    break;
                            }
                        }
                            break;

                        case ' ':
                        case '\t':
                        case '\r':
                            break;

                        case '\n':
                        {
                            m_LineCount++;
                            m_LineStart = m_CharBufferPosition;
                        }
                            break;

                        case ':':
                        {
                            if (!IsExpected(JsonType.MemberSeparator))
                            {
                                Break(JsonType.MemberSeparator);
                                return;
                            }

                            m_Expected = JsonType.Value;
                        }
                            break;

                        case ',':
                        {
                            if (!IsExpected(JsonType.ValueSeparator))
                            {
                                Break(JsonType.ValueSeparator);
                                return;
                            }

                            switch (m_Stack.Peek())
                            {
                                case JsonType.BeginObject:
                                    m_Expected = JsonType.String;
                                    break;
                                case JsonType.BeginArray:
                                    m_Expected = JsonType.Value;
                                    break;
                                default:
                                    m_Expected = JsonType.Undefined;
                                    break;
                            }
                        }
                            break;

                        case '"':
                        {
                            if (!IsExpected(JsonType.String))
                            {
                                Break(JsonType.String);
                                return;
                            }

                            var start = m_CharBufferPosition;

                            m_CharBufferPosition++;

                            var result = ReadString();

                            if (result != k_ResultSuccess)
                            {
                                m_PartialTokenType = JsonType.String;
                                m_PartialTokenState = m_CharBufferPosition - start;
                                Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                                return;
                            }
                        }
                            break;

                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '-':
                        {
                            if (!IsExpected(JsonType.Number))
                            {
                                Break(JsonType.Number);
                                return;
                            }

                            var state = 0;
                            var result = ReadNumber(ref state);

                            if (result != k_ResultSuccess)
                            {
                                m_PartialTokenType = JsonType.Number;
                                m_PartialTokenState = state;
                                Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                                return;
                            }

                            m_CharBufferPosition--;
                        }
                            break;

                        case 't':
                        {
                            if (!IsExpected(JsonType.True))
                            {
                                Break(JsonType.True);
                                return;
                            }

                            var start = m_CharBufferPosition;
                            var result = ReadTrue(0);

                            if (result != k_ResultSuccess)
                            {
                                m_PartialTokenType = JsonType.True;
                                m_PartialTokenState = m_CharBufferPosition - start;
                                Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                                return;
                            }

                            m_CharBufferPosition--;
                        }
                            break;

                        case 'f':
                        {
                            if (!IsExpected(JsonType.False))
                            {
                                Break(JsonType.False);
                                return;
                            }

                            var start = m_CharBufferPosition;
                            var result = ReadFalse(0);

                            if (result != k_ResultSuccess)
                            {
                                m_PartialTokenType = JsonType.False;
                                m_PartialTokenState = m_CharBufferPosition - start;
                                Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                                return;
                            }

                            m_CharBufferPosition--;
                        }
                            break;

                        case 'n':
                        {
                            if (!IsExpected(JsonType.Null))
                            {
                                Break(JsonType.Null);
                                return;
                            }

                            var start = m_CharBufferPosition;
                            var result = ReadNull(0);

                            if (result != k_ResultSuccess)
                            {
                                m_PartialTokenType = JsonType.Null;
                                m_PartialTokenState = m_CharBufferPosition - start;
                                Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                                return;
                            }

                            m_CharBufferPosition--;
                        }
                            break;

                        default:
                        {
                            Break(JsonType.Undefined);
                            return;
                        }
                    }

                    m_CharBufferPosition++;
                }

                Break(JsonType.EOF);
            }

            private int ReadString()
            {
                m_PrevChar = '\0';

                while (m_CharBufferPosition < CharBufferLength)
                {
                    var c = CharBuffer[m_CharBufferPosition];

                    if (c == '"' && m_PrevChar != '\\')
                    {
                        switch (m_Stack.Peek())
                        {
                            case JsonType.BeginObject:
                            {
                                m_Stack.Push(JsonType.String);
                                m_Expected = JsonType.MemberSeparator;
                            }
                                break;

                            case JsonType.BeginArray:
                            {
                                m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                            }
                                break;

                            case JsonType.String:
                            {
                                m_Stack.Pop();

                                switch (m_Stack.Peek())
                                {
                                    case JsonType.BeginObject:
                                        m_Expected = JsonType.ValueSeparator | JsonType.EndObject;
                                        break;
                                    case JsonType.BeginArray:
                                        m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                                        break;
                                    default:
                                        m_Expected = JsonType.Undefined;
                                        break;
                                }
                            }
                                break;
                        }

                        return k_ResultSuccess;
                    }

                    m_PrevChar = c;
                    m_CharBufferPosition++;
                }

                return k_ResultEndOfStream;
            }

            private int ReadNumber(ref int state)
            {
                const int stateStart = 0;
                const int stateIntegerPart = 1;
                const int stateDecimalPart = 2;
                const int stateEPart = 3;

                m_PrevChar = '\0';

                while (m_CharBufferPosition < CharBufferLength)
                {
                    var c = CharBuffer[m_CharBufferPosition];

                    if (c == '\t' ||
                        c == '\r' ||
                        c == '\n' ||
                        c == ' ' ||
                        c == ',' ||
                        c == ']' ||
                        c == '}')
                    {
                        break;
                    }

                    switch (c)
                    {
                        case '-':
                        {
                            if (state == stateEPart)
                            {
                                break;
                            }

                            if (state != stateStart)
                            {
                                return k_ResultInvalidJson;
                            }

                            state = stateIntegerPart;
                        }
                            break;

                        case '.':
                        {
                            if (state != stateIntegerPart)
                            {
                                return k_ResultInvalidJson;
                            }

                            state = stateDecimalPart;
                        }
                            break;

                        case 'e':
                        case 'E':
                        {
                            if (m_PrevChar == '-' || m_PrevChar == '.' || state != stateDecimalPart && state != stateIntegerPart)
                            {
                                return k_ResultInvalidJson;
                            }

                            state = stateEPart;
                        }
                            break;

                        default:
                        {
                            if (c < '0' || c > '9')
                            {
                                return k_ResultInvalidJson;
                            }

                            if (state == stateStart)
                            {
                                state = stateIntegerPart;
                            }
                        }
                            break;
                    }

                    m_PrevChar = c;
                    m_CharBufferPosition++;
                }

                if (m_CharBufferPosition >= CharBufferLength)
                {
                    return k_ResultEndOfStream;
                }

                if (m_PrevChar == 'e' || m_PrevChar == 'E' || m_PrevChar == '-' || m_PrevChar == '.')
                {
                    return k_ResultInvalidJson;
                }

                if (m_Stack.Peek() == JsonType.String)
                {
                    m_Stack.Pop();
                }

                switch (m_Stack.Peek())
                {
                    case JsonType.BeginObject:
                        m_Expected = JsonType.ValueSeparator | JsonType.EndObject;
                        break;
                    case JsonType.BeginArray:
                        m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                        break;
                    default:
                        m_Expected = JsonType.Undefined;
                        break;
                }

                return k_ResultSuccess;
            }

            private int ReadTrue(int start)
            {
                var expected = stackalloc ushort[4] {'t', 'r', 'u', 'e'};
                return ReadPrimitive(expected, start, 4);
            }

            private int ReadFalse(int start)
            {
                var expected = stackalloc ushort[5] {'f', 'a', 'l', 's', 'e'};
                return ReadPrimitive(expected, start, 5);
            }

            private int ReadNull(int start)
            {
                var expected = stackalloc ushort[4] {'n', 'u', 'l', 'l'};
                return ReadPrimitive(expected, start, 4);
            }

            private int ReadPrimitive(ushort* expected, int start, int length)
            {
                for (var i = start; i < length && m_CharBufferPosition < CharBufferLength; i++)
                {
                    var c = CharBuffer[m_CharBufferPosition];

                    if (c != expected[i])
                    {
                        return k_ResultInvalidJson;
                    }

                    m_CharBufferPosition++;
                }

                if (m_CharBufferPosition >= CharBufferLength)
                {
                    return k_ResultEndOfStream;
                }

                if (m_Stack.Peek() == JsonType.String)
                {
                    m_Stack.Pop();
                }

                switch (m_Stack.Peek())
                {
                    case JsonType.BeginObject:
                        m_Expected = JsonType.ValueSeparator | JsonType.EndObject;
                        break;
                    case JsonType.BeginArray:
                        m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                        break;
                    default:
                        m_Expected = JsonType.Undefined;
                        break;
                }

                return k_ResultSuccess;
            }

            private bool IsExpected(JsonType type)
            {
                return (type & m_Expected) == type;
            }
        }

        private readonly Allocator m_Allocator;
        private JsonTypeStack m_Stack;
        private JobHandle m_Handle;
        private ValidationJobData* m_Data;

        public JsonStandardValidator(Allocator label = SerializationConfiguration.DefaultAllocatorLabel)
        {
            m_Allocator = label;
            m_Stack = new JsonTypeStack(k_DefaultDepthLimit, label);
            m_Data = (ValidationJobData*) UnsafeUtility.Malloc(sizeof(ValidationJobData), UnsafeUtility.AlignOf<ValidationJobData>(), label);
            Initialize();
        }

        public void Initialize()
        {
            m_Stack.Clear();

            UnsafeUtility.MemClear(m_Data, sizeof(ValidationJobData));

            m_Data->Stack = m_Stack;
            m_Data->PrevChar = '\0';
            m_Data->Expected = JsonType.BeginObject | JsonType.BeginArray;
            m_Data->CharCount = 1;
            m_Data->LineCount = 1;
            m_Data->LineStart = -1;
        }

        public JsonValidationResult GetResult()
        {
            if (!m_Handle.IsCompleted)
            {
                throw new InvalidDataException("Validation job is in progress.");
            }

            return new JsonValidationResult
            {
                ExpectedType = m_Data->Expected,
                ActualType = m_Data->Actual,
                Char = (char) m_Data->Char,
                LineCount = m_Data->LineCount,
                CharCount = m_Data->CharCount
            };
        }

        public JobHandle ValidateAsync(UnsafeBuffer<char> buffer, int start, int count)
        {
            if (!m_Handle.IsCompleted)
            {
                throw new InvalidDataException("The validator is currently in use by a previous operation.");
            }

            m_Data->CharBufferPosition = start;

            m_Handle = new StandardJsonValidationJob
            {
                Data = m_Data,
                CharBuffer = (ushort*) buffer.Buffer,
                CharBufferLength = start + count,
            }.Schedule();

            return m_Handle;
        }

        public JsonValidationResult Validate(UnsafeBuffer<char> buffer, int start, int count)
        {
            ValidateAsync(buffer, start, count).Complete();
            return GetResult();
        }

        public void Dispose()
        {
            UnsafeUtility.Free(m_Data, m_Allocator);
            m_Data = null;
            m_Stack.Dispose();
        }
    }
}
