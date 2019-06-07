using System.Runtime.InteropServices;
using Unity.Entities.Serialization;
using Unity.Tiny.Codec;

namespace Unity.Authoring
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct SceneHeader
    {
        public const int CurrentVersion = 1;

        [FieldOffset(0)]
        public int Version;
        [FieldOffset(4)]
        public int SharedComponentCount;
        [FieldOffset(8)]
        public Codec Codec;
        [FieldOffset(12)]
        public int DecompressedSize; // should not include this header

        public void SerializeHeader(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);
            writer.Write(SharedComponentCount);
            writer.Write((int)Codec);
            writer.Write(DecompressedSize);
        }
    }
}
