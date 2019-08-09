using System;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Serialization.Json;
using UnityEngine;

namespace Unity.Serialization
{
    public struct SerializedObjectReaderConfiguration
    {
        /// <summary>
        /// If true, the input stream is read asynchronously. The default is true.
        /// </summary>
        public bool UseReadAsync;

        /// <summary>
        /// The buffer size, in bytes, of the blocks/chunks read from the input stream. The default size is 4096.
        /// </summary>
        public int BlockBufferSize;

        /// <summary>
        /// The internal token buffer size, in tokens. This should be big enough to contain all tokens generated from a block. The default size is 1024.
        /// </summary>
        public int TokenBufferSize;

        /// <summary>
        /// The packed binary output buffer size, in bytes. This should be big enough to contain all string and primitive data for the needed scope. The default size is 4096.
        /// </summary>
        public int OutputBufferSize;

        /// <summary>
        /// The size of the Node buffer for internal reads. For optimal performance, this should be equal to the maximum batch size. The default size is 128.
        /// </summary>
        public int NodeBufferSize;

        /// <summary>
        /// JSON validation type to use. The default is `Standard`.
        /// </summary>
        public JsonValidationType ValidationType;

        public static readonly SerializedObjectReaderConfiguration Default = new SerializedObjectReaderConfiguration
        {
            UseReadAsync = true,
            BlockBufferSize = 4096,
            TokenBufferSize = 1024,
            OutputBufferSize = 4096,
            NodeBufferSize = 128,
            ValidationType = JsonValidationType.Standard
        };
    }

    public class SerializedObjectReader : IDisposable
    {
        private readonly bool m_LeaveOutputOpen;

        private readonly StreamReader m_StreamReader;
        private readonly IBlockReader m_BlockReader;
        private readonly JsonTokenizer m_Tokenizer;
        private readonly NodeParser m_Parser;
        private readonly PackedBinaryStream m_BinaryStream;
        private readonly PackedBinaryWriter m_BinaryWriter;

        private Block m_Block;

        private static Stream OpenFileStreamWithConfiguration(string path, SerializedObjectReaderConfiguration configuration)
        {
            if (configuration.BlockBufferSize < 16)
            {
                throw new ArgumentException("SerializedObjectReaderConfiguration.BlockBufferSize < 16");
            }

            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, configuration.BlockBufferSize, configuration.UseReadAsync ? FileOptions.Asynchronous : FileOptions.None);
        }

        private static PackedBinaryStream OpenBinaryStreamWithConfiguration(SerializedObjectReaderConfiguration configuration, Allocator label)
        {
            if (configuration.TokenBufferSize < 16)
            {
                throw new ArgumentException("TokenBufferSize < 16");
            }

            if (configuration.OutputBufferSize < 16)
            {
                throw new ArgumentException("OutputBufferSize < 16");
            }

            return new PackedBinaryStream(configuration.TokenBufferSize, configuration.OutputBufferSize, label);
        }

        public SerializedObjectReader(string path, Allocator label = SerializationConfiguration.DefaultAllocatorLabel)
            : this(path, SerializedObjectReaderConfiguration.Default, label)
        {
        }

        public SerializedObjectReader(string path, SerializedObjectReaderConfiguration configuration, Allocator label = SerializationConfiguration.DefaultAllocatorLabel)
            : this(path, OpenBinaryStreamWithConfiguration(configuration, label), configuration, label, false)
        {
        }

        public SerializedObjectReader(string path, PackedBinaryStream output, Allocator label = SerializationConfiguration.DefaultAllocatorLabel, bool leaveOutputOpen = true)
            : this(path, output, SerializedObjectReaderConfiguration.Default, label, leaveOutputOpen)
        {
        }

        public SerializedObjectReader(string path, PackedBinaryStream output, SerializedObjectReaderConfiguration configuration, Allocator label = SerializationConfiguration.DefaultAllocatorLabel, bool leaveOutputOpen = true)
            : this(OpenFileStreamWithConfiguration(path, configuration), output, configuration, label, false, leaveOutputOpen)
        {
        }

        public SerializedObjectReader(Stream input, Allocator label = SerializationConfiguration.DefaultAllocatorLabel, bool leaveInputOpen = true)
            : this(input, SerializedObjectReaderConfiguration.Default, label, leaveInputOpen)
        {
        }

        public SerializedObjectReader(Stream input, PackedBinaryStream output, Allocator label =  SerializationConfiguration.DefaultAllocatorLabel, bool leaveInputOpen = true, bool leaveOutputOpen = true)
            : this(input, output, SerializedObjectReaderConfiguration.Default, label, leaveInputOpen, leaveOutputOpen)
        {
        }

        public SerializedObjectReader(Stream input, SerializedObjectReaderConfiguration configuration, Allocator label = SerializationConfiguration.DefaultAllocatorLabel, bool leaveInputOpen = true)
            : this(input, OpenBinaryStreamWithConfiguration(configuration, label), configuration, label, leaveInputOpen, false)
        {
        }

        public SerializedObjectReader(Stream input, PackedBinaryStream output, SerializedObjectReaderConfiguration configuration, Allocator label = SerializationConfiguration.DefaultAllocatorLabel, bool leaveInputOpen = true, bool leaveOutputOpen = true)
        {
            if (configuration.BlockBufferSize < 16)
            {
                throw new ArgumentException("BlockBufferSize < 16");
            }

            if (configuration.TokenBufferSize < 16)
            {
                throw new ArgumentException("TokenBufferSize < 16");
            }

            m_LeaveOutputOpen = leaveOutputOpen;
            var blockBufferCharSize = configuration.BlockBufferSize / sizeof(char);
            m_StreamReader = new StreamReader(input, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: configuration.BlockBufferSize, leaveInputOpen);
            m_BlockReader = configuration.UseReadAsync ? new AsyncBlockReader(m_StreamReader, blockBufferCharSize) : (IBlockReader) new SyncBlockReader(m_StreamReader, blockBufferCharSize);
            m_Tokenizer = new JsonTokenizer(configuration.TokenBufferSize, configuration.ValidationType, label);
            m_Parser = new NodeParser(m_Tokenizer, configuration.NodeBufferSize, label);
            m_BinaryStream = output;
            m_BinaryWriter = new PackedBinaryWriter(m_BinaryStream, m_Tokenizer);
        }

        /// <summary>
        /// Advances the reader to the given node type, ignoring depth/scope.
        /// </summary>
        /// <param name="node">The node type to stop at.</param>
        /// <returns>The node type the parser stopped at.</returns>
        public NodeType Step(NodeType node = NodeType.Any)
        {
            ReadInternal(node, NodeParser.k_IgnoreParent);
            return m_Parser.NodeType;
        }

        /// <summary>
        /// Advances the reader to the given node type, ignoring depth/scope.
        /// </summary>
        /// <param name="view">The view at the returned node type.</param>
        /// <param name="node">The node type to stop at.</param>
        /// <returns>The node type the parser stopped at.</returns>
        public NodeType Step(out SerializedValueView view, NodeType node = NodeType.Any)
        {
            view = ReadInternal(node, NodeParser.k_IgnoreParent);
            return m_Parser.NodeType;
        }

        /// <summary>
        /// Reads the next node in the stream, respecting depth/scope.
        /// </summary>
        /// <param name="node">The node type to stop at.</param>
        /// <returns>The node type the parser stopped at.</returns>
        public NodeType Read(NodeType node = NodeType.Any)
        {
            ReadInternal(node, m_Parser.TokenParentIndex);
            return m_Parser.NodeType;
        }

        /// <summary>
        /// Reads the next node in the stream, respecting depth/scope.
        /// </summary>
        /// <param name="view">The view at the returned node type.</param>
        /// <param name="node">The node type to stop at.</param>
        /// <returns>The node type the parser stopped at.</returns>
        public NodeType Read(out SerializedValueView view, NodeType node = NodeType.Any)
        {
            view = ReadInternal(node, m_Parser.TokenParentIndex);
            return m_Parser.NodeType;
        }

        public void Read(SerializedMemberViewCollection collection)
        {
            Read(out var view);
            collection.Add(view.AsMemberView());
        }

        public SerializedObjectView ReadObject()
        {
            FillBuffer();
            if (!CheckNextTokenType(TokenType.Object))
            {
                throw new InvalidDataException();
            }

            Read(out var view);
            return view.AsObjectView();
        }

        public bool ReadArrayElement(out SerializedValueView view)
        {
            if (!CheckArrayElement())
            {
                view = default;
                return false;
            }

            view = ReadInternal(NodeType.Any, m_Parser.TokenParentIndex);

            if (m_Parser.NodeType != NodeType.Any)
            {
                return true;
            }

            view = default;
            return false;
        }

        public unsafe int ReadArrayElementBatch(NativeArray<SerializedValueView> views, int count)
        {
            if (count > views.Length)
            {
                throw new IndexOutOfRangeException();
            }

            return ReadArrayElementBatch((SerializedValueView*) views.GetUnsafePtr(), count);
        }

        public unsafe int ReadArrayElementBatch(SerializedValueView* views, int count)
        {
            if (!CheckArrayElement())
            {
                return 0;
            }

            count = ReadInternalBatch(views, count, NodeType.Any, m_Parser.TokenParentIndex);
            return count;
        }

        private bool CheckArrayElement()
        {
            if (m_Parser.Node == -1)
            {
                return false;
            }

            return CheckNextTokenParent(m_Parser.NodeType == NodeType.BeginArray ? m_Parser.Node : m_Tokenizer.Tokens[m_Parser.Node].Parent);
        }

        private bool CheckNextTokenType(TokenType type)
        {
            return m_Parser.TokenNextIndex < m_Tokenizer.TokenNextIndex && m_Tokenizer.Tokens[m_Parser.TokenNextIndex].Type == type;
        }

        private bool CheckNextTokenParent(int parent)
        {
            if (m_Parser.TokenNextIndex >= m_Tokenizer.TokenNextIndex)
            {
                return false;
            }

            return m_Tokenizer.Tokens[m_Parser.TokenNextIndex].Parent == parent;
        }

        private unsafe int GetViewIndex(int node, int inputTokenStart, int binaryTokenStart)
        {
            if (node == -1)
            {
                return -1;
            }

            var data = m_BinaryStream.GetUnsafeData();

            if (node >= inputTokenStart)
            {
                // This is a newly written token.
                // Since we know tokens are written in order; we can simply compute the offset.
                var offset = m_Parser.TokenNextIndex - node;
                return data->TokenNextIndex - offset;
            }

            // This is a previously written token.
            // Since we know we can never discard an incomplete token.
            // We must walk up the tree the same number of times for both streams to find the correct token.
            var binaryIndex = binaryTokenStart;

            var binaryTokens = m_BinaryStream.GetUnsafeData()->Tokens;
            while (inputTokenStart != node)
            {
                inputTokenStart = m_Tokenizer.Tokens[inputTokenStart].Parent;
                binaryIndex = binaryTokens[binaryIndex].Parent;
            }

            return binaryIndex;
        }

        private SerializedValueView ReadInternal(NodeType type, int parent)
        {
            for (;;)
            {
                if (FillBuffer())
                {
                    if (parent >= 0)
                    {
                        parent = m_Tokenizer.DiscardRemap[parent];
                    }
                }

                var parserStart = m_Parser.TokenNextIndex - 1;
                var writerStart = m_BinaryStream.TokenNextIndex - 1;

                m_Parser.Step(type, parent);

                Write(m_Parser.TokenNextIndex - m_BinaryWriter.TokenNextIndex);

                if (m_Parser.NodeType == NodeType.None && m_Block != null)
                {
                    continue;
                }

                var node = m_Parser.Node;
                return node == -1 ? default : m_BinaryWriter.GetView(GetViewIndex(node, parserStart, writerStart));
            }
        }

        private unsafe int ReadInternalBatch(SerializedValueView* views, int count, NodeType type, int parent)
        {
            var index = 0;

            for (;;)
            {
                if (FillBuffer())
                {
                    if (parent >= 0)
                    {
                        parent = m_Tokenizer.DiscardRemap[parent];
                    }
                }

                var parserStart = m_Parser.TokenNextIndex - 1;
                var writerStart = m_BinaryStream.TokenNextIndex - 1;

                var batchCount = m_Parser.StepBatch(count - index, type, parent);

                Write(m_Parser.TokenNextIndex - m_BinaryWriter.TokenNextIndex);

                for (var i = 0; i < batchCount; i++)
                {
                    views[index + i] = m_BinaryWriter.GetView(GetViewIndex(m_Parser.Nodes[i], parserStart, writerStart));
                }

                index += batchCount;

                if (m_Parser.NodeType == NodeType.None && m_Block != null)
                {
                    continue;
                }

                return index;
            }
        }

        private unsafe void Write(int count)
        {
            if (count <= 0)
            {
                return;
            }

            fixed (char* ptr = m_Block.Buffer)
            {
                m_BinaryWriter.Write(new UnsafeBuffer<char> {Buffer = ptr, Length = m_Block.Length}, count);
            }
        }

        private unsafe bool FillBuffer()
        {
            if (m_Parser.TokenNextIndex < m_Tokenizer.TokenNextIndex || m_Parser.NodeType != NodeType.None)
            {
                return false;
            }

            m_Block = m_BlockReader.GetNextBlock();

            if (null == m_Block || m_Block.Length == 0)
            {
                m_Block = null;
                return false;
            }

            m_Tokenizer.DiscardCompleted();
            m_Parser.Seek(m_Tokenizer.TokenNextIndex, m_Tokenizer.TokenParentIndex);
            m_BinaryWriter.Seek(m_Tokenizer.TokenNextIndex, m_BinaryWriter.TokenParentIndex != -1
                                    ? m_Tokenizer.DiscardRemap[m_BinaryWriter.TokenParentIndex]
                                    : -1);

            fixed (char* ptr = m_Block.Buffer)
            {
                m_Tokenizer.Write(new UnsafeBuffer<char> {Buffer = ptr, Length = m_Block.Buffer.Length}, 0, m_Block.Length);
            }

            return true;
        }

        public void DiscardCompleted()
        {
            m_BinaryStream.DiscardCompleted();
        }

        public void Dispose()
        {
            m_BinaryWriter.Dispose();
            if (!m_LeaveOutputOpen)
            {
                m_BinaryStream.Dispose();
            }
            m_Parser.Dispose();
            m_Tokenizer.Dispose();
            m_StreamReader.Dispose();
        }
    }
}
