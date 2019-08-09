using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Serialization
{
    internal struct Handle
    {
        public int Index;
        public int Version;
    }

    internal struct HandleData
    {
        public int DataIndex;
        public int DataVersion;
    }

    internal struct BinaryToken
    {
        public TokenType Type;
        public int HandleIndex;
        public int Position;
        public int Parent;
        public int Length;

        public override string ToString()
        {
            return $"Type=[{Type}] HandleIndex=[{HandleIndex}] Position=[{Position}] Parent=[{Parent}] Length=[{Length}]";
        }
    }

    internal unsafe struct PackedBinaryStreamData
    {
        public BinaryToken* Tokens;
        public HandleData* Handles;
        public int TokensCapacity;
        public int TokenNextIndex;
        public int TokenParentIndex;

        public byte* Buffer;
        public int BufferCapacity;
        public int BufferPosition;
    }

    public unsafe struct PackedBinaryStream : IDisposable
    {
        /// <summary>
        /// All input characters were consumes and all tokens were generated.
        /// </summary>
        private const int k_ResultSuccess = 0;

        /// <summary>
        /// The maximum depth limit has been exceeded.
        /// </summary>
        private const int k_ResultStackOverflow = -4;

        private const int k_DefaultBufferCapacity = 4096;
        private const int k_DefaultTokenCapacity = 1024;

        [BurstCompile(CompileSynchronously = true)]
        private struct InitializeJob : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction] public BinaryToken* BinaryTokens;
            [NativeDisableUnsafePtrRestriction] public HandleData* Handles;

            public int StartIndex;

            public void Execute(int index)
            {
                index += StartIndex;

                BinaryTokens[index] = new BinaryToken
                {
                    HandleIndex = index
                };

                Handles[index] = new HandleData
                {
                    DataIndex = index,
                    DataVersion = 1
                };
            }
        }

        private struct DiscardCompletedJobOutput
        {
            public int Result;
            public int TokenNextIndex;
            public int TokenParentIndex;
            public int BufferPosition;
        }

        /// <summary>
        /// Compress/shrink the buffers removing any completed tokens.
        ///
        /// The last token and all ancestors are preserved while siblings are discarded.
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        private struct DiscardCompletedJob : IJob
        {
            private const int k_StackSize = 128;

            [NativeDisableUnsafePtrRestriction] public DiscardCompletedJobOutput* Output;

            [NativeDisableUnsafePtrRestriction] public BinaryToken* Tokens;
            public int TokenNextIndex;
            public int TokenParentIndex;

            [NativeDisableUnsafePtrRestriction] public HandleData* Handles;
            [NativeDisableUnsafePtrRestriction] public byte* Buffer;
            public int BufferPosition;

            public void Execute()
            {
                var stack = stackalloc int[k_StackSize];
                var sp = -1;

                var index = TokenNextIndex - 1;

                while (index != -1 && Tokens[index].Length == -1)
                {
                    index = Tokens[index].Parent;
                }

                while (index != -1)
                {
                    var token = Tokens[index];
                    var partIndex = index + 1;
                    var partCount = 1;

                    for (;partIndex < TokenNextIndex; partIndex++)
                    {
                        if (Tokens[partIndex].Length == -1)
                        {
                            partCount++;
                            continue;
                        }

                        break;
                    }

                    if (sp + partCount >= k_StackSize)
                    {
                        Output->Result = k_ResultStackOverflow;
                        return;
                    }

                    for (var i = partCount - 1; i >= 0; i--)
                    {
                        stack[++sp] = index + i;
                    }

                    index = token.Parent;
                }

                var binaryTokenNextIndex = sp + 1;
                var binaryBufferPosition = 0;

                for (var i = 0; sp >= 0; i++, sp--)
                {
                    index = stack[sp];

                    if (TokenParentIndex == index)
                    {
                        TokenParentIndex = i;
                    }

                    Swap(i, index);

                    var token = Tokens[i];

                    var length = 0;

                    if (index + 1 >= TokenNextIndex)
                    {
                        length = BufferPosition - token.Position;
                    }
                    else
                    {
                        length = Tokens[index + 1].Position - token.Position;
                    }

                    Copy(Buffer + binaryBufferPosition, Buffer + token.Position, length);

                    token.Position = binaryBufferPosition;

                    var parentIndex = i - 1;
                    if (token.Length != -1)
                    {
                        while (parentIndex != -1 && Tokens[parentIndex].Length == -1)
                        {
                            parentIndex--;
                        }
                    }
                    token.Parent = parentIndex;

                    Tokens[i] = token;
                    binaryBufferPosition += length;
                }

                // Patch the lengths
                for (int i = 0, length = binaryTokenNextIndex; i < binaryTokenNextIndex; i++)
                {
                    var token = Tokens[i];

                    if (token.Length != -1)
                    {
                        token.Length = length;
                    }

                    length--;
                    Tokens[i] = token;
                }

                // Invalidate all views that are outside of the collapsed range
                for (var i = binaryTokenNextIndex; i < TokenNextIndex; i++)
                {
                    Handles[Tokens[i].HandleIndex].DataVersion++;
                }

                Output->Result = k_ResultSuccess;
                Output->BufferPosition = binaryBufferPosition;
                Output->TokenNextIndex = binaryTokenNextIndex;
                Output->TokenParentIndex = TokenParentIndex;
            }

            private void Swap(int x, int y)
            {
                var tmp = Tokens[x];
                Tokens[x] = Tokens[y];
                Tokens[y] = tmp;

                // Update handle pointers
                Handles[Tokens[x].HandleIndex].DataIndex = x;
                Handles[Tokens[y].HandleIndex].DataIndex = y;
            }

            private static void Copy(byte* destination, byte* source, int length)
            {
                for (var i = 0; i < length; i++)
                {
                    destination[i] = source[i];
                }
            }
        }

        /// <summary>
        /// Increments the version counter on all handles to invalidate all distributed views.
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        private struct ClearJob : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction] public HandleData* Handles;

            public void Execute(int index)
            {
                Handles[index].DataVersion++;
            }
        }

        private readonly Allocator m_Label;

        [NativeDisableUnsafePtrRestriction] private PackedBinaryStreamData* m_Data;

        public PackedBinaryStream(Allocator label) : this(k_DefaultTokenCapacity, k_DefaultBufferCapacity, label)
        {

        }

        public PackedBinaryStream(int initialTokensCapacity, int initialBufferCapacity, Allocator label)
        {
            m_Label = label;
            m_Data = (PackedBinaryStreamData*) UnsafeUtility.Malloc(sizeof(PackedBinaryStreamData), UnsafeUtility.AlignOf<PackedBinaryStreamData>(), m_Label);
            UnsafeUtility.MemClear(m_Data, sizeof(PackedBinaryStreamData));

            // Allocate token and handle buffers.
            m_Data->Tokens = (BinaryToken*) UnsafeUtility.Malloc(sizeof(BinaryToken) * initialTokensCapacity, UnsafeUtility.AlignOf<BinaryToken>(), m_Label);
            m_Data->Handles = (HandleData*) UnsafeUtility.Malloc(sizeof(HandleData) * initialTokensCapacity, UnsafeUtility.AlignOf<HandleData>(), m_Label);
            m_Data->TokensCapacity = initialTokensCapacity;

            // Allocate string/primitive storage.
            m_Data->Buffer = (byte*) UnsafeUtility.Malloc(sizeof(byte) * initialBufferCapacity, UnsafeUtility.AlignOf<byte>(), m_Label);
            m_Data->BufferCapacity = initialBufferCapacity;

            // Initialize handles and tokens with the correct indices.
            new InitializeJob
            {
                Handles = m_Data->Handles,
                BinaryTokens = m_Data->Tokens,
                StartIndex = 0
            }.Run(initialTokensCapacity);

            Clear();
        }

        internal PackedBinaryStreamData* GetUnsafeData() => m_Data;

        internal int TokenNextIndex => m_Data->TokenNextIndex;

        internal bool IsValid(Handle handle)
        {
            if ((uint) handle.Index >= (uint) m_Data->TokensCapacity)
            {
                throw new IndexOutOfRangeException();
            }

            return m_Data->Handles[handle.Index].DataVersion == handle.Version;
        }

        internal BinaryToken GetToken(int tokenIndex)
        {
            if ((uint) tokenIndex >= (uint) m_Data->TokenNextIndex)
            {
                throw new IndexOutOfRangeException();
            }

            return m_Data->Tokens[tokenIndex];
        }

        internal BinaryToken GetToken(Handle handle)
        {
            if ((uint) handle.Index >= (uint) m_Data->TokensCapacity)
            {
                throw new IndexOutOfRangeException();
            }

            var data = m_Data->Handles[handle.Index];

            if (data.DataVersion != handle.Version)
            {
                throw new InvalidOperationException("View is invalid. The underlying data has been released.");
            }

            return GetToken(data.DataIndex);
        }

        internal int GetTokenIndex(Handle handle)
        {
            if ((uint) handle.Index >= (uint) m_Data->TokensCapacity)
            {
                throw new IndexOutOfRangeException();
            }

            var data = m_Data->Handles[handle.Index];

            if (data.DataVersion != handle.Version)
            {
                throw new InvalidOperationException("View is invalid. The underlying data has been released.");
            }

            return data.DataIndex;
        }

        internal Handle GetHandle(int tokenIndex)
        {
            return GetHandle(GetToken(tokenIndex));
        }

        internal Handle GetHandle(BinaryToken token)
        {
            return new Handle {Index = token.HandleIndex, Version = m_Data->Handles[token.HandleIndex].DataVersion };
        }

        internal Handle GetChild(Handle handle)
        {
            var start = GetTokenIndex(handle);

            for (var index = start + 1; index < m_Data->TokenNextIndex; index++)
            {
                var token = GetToken(index);

                if (token.Length != -1 && token.Parent == start)
                {
                    return GetHandle(token);
                }
            }

            throw new InvalidOperationException("Token out of range. Data has not been read yet.");
        }

        internal T* GetBufferPtr<T>(Handle handle) where T : unmanaged
        {
            var position = GetToken(handle).Position;

            if (position + sizeof(T) > m_Data->BufferPosition)
            {
                throw new IndexOutOfRangeException();
            }

            return (T*) (m_Data->Buffer + position);
        }

        internal void EnsureTokenCapacity(int newLength)
        {
            if (newLength <= m_Data->TokensCapacity)
            {
                return;
            }

            var fromLength = m_Data->TokensCapacity;

            m_Data->Tokens = Resize(m_Data->Tokens, fromLength, newLength, m_Label);
            m_Data->Handles = Resize(m_Data->Handles, fromLength, newLength, m_Label);

            new InitializeJob
            {
                Handles = m_Data->Handles,
                BinaryTokens = m_Data->Tokens,
                StartIndex = fromLength
            }.Run(newLength - fromLength);


            m_Data->TokensCapacity = newLength;
        }

        internal void EnsureBufferCapacity(int length)
        {
            if (length <= m_Data->BufferCapacity)
            {
                return;
            }

            m_Data->Buffer = Resize(m_Data->Buffer, m_Data->BufferPosition, length, m_Label);
            m_Data->BufferCapacity = length;
        }

        /// <summary>
        /// Clears all data from the buffer.
        /// </summary>
        public void Clear()
        {
            new ClearJob { Handles = m_Data->Handles }.Run(m_Data->TokenNextIndex);
            m_Data->TokenNextIndex = 0;
            m_Data->TokenParentIndex = -1;
            m_Data->BufferPosition = 0;
        }

        /// <summary>
        /// Discards completed data from the buffers.
        /// </summary>
        internal void DiscardCompleted()
        {
            var output = new DiscardCompletedJobOutput();

            new DiscardCompletedJob
                {
                    Output = &output,
                    Tokens = m_Data->Tokens,
                    TokenNextIndex = m_Data->TokenNextIndex,
                    TokenParentIndex = m_Data->TokenParentIndex,
                    Handles = m_Data->Handles,
                    Buffer = m_Data->Buffer,
                    BufferPosition = m_Data->BufferPosition
                }
                .Run();

            if (output.Result == k_ResultStackOverflow)
            {
                throw new StackOverflowException();
            }

            m_Data->TokenNextIndex = output.TokenNextIndex;
            m_Data->TokenParentIndex = output.TokenParentIndex;
            m_Data->BufferPosition = output.BufferPosition;
        }

        public void Dispose()
        {
            UnsafeUtility.Free(m_Data->Tokens, m_Label);
            UnsafeUtility.Free(m_Data->Handles, m_Label);
            UnsafeUtility.Free(m_Data->Buffer, m_Label);
            UnsafeUtility.MemClear(m_Data, sizeof(PackedBinaryStreamData));
            UnsafeUtility.Free(m_Data, m_Label);
            m_Data = null;
        }

        private static T* Resize<T>(T* buffer, int fromLength, int toLength, Allocator label) where T : unmanaged
        {
            var tmp = (T*) UnsafeUtility.Malloc(toLength * sizeof(T), UnsafeUtility.AlignOf<T>(), label);
            UnsafeUtility.MemCpy(tmp, buffer, fromLength * sizeof(T));
            UnsafeUtility.Free(buffer, label);
            return tmp;
        }
    }
}
