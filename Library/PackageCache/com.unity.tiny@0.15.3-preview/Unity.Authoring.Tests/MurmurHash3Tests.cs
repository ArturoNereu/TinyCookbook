using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Authoring.Hashing;

namespace Unity.Authoring.Tests
{
    internal class MurmurHash3Tests
    {
        [Test]
        public void MurmurHash3_x86_32()
        {
            using (var stream = new MemoryStream())
            {
                var key = new byte[256];
                for (var i = 0; i < 256; i++)
                {
                    key[i] = (byte)i;
                    var hash = MurmurHash3.ComputeHash32(key.Take(i).ToArray(), (uint)(256 - i));
                    stream.Write(BitConverter.GetBytes(hash), 0, 4);
                }

                var result = MurmurHash3.ComputeHash32(stream.GetBuffer());
                Assert.IsTrue(result == 0xb0f57ee3);
            }
        }

        [Test]
        public void MurmurHash3_x86_32_Stream()
        {
            var data = "The quick brown fox jumps over the lazy dog";
            var bytes = Encoding.ASCII.GetBytes(data);
            using (var stream = new MemoryStream(bytes))
            {
                var result1 = MurmurHash3.ComputeHash32(bytes, 0x9747b28c);
                var result2 = MurmurHash3.ComputeHash32(stream, 0x9747b28c);
                Assert.IsTrue(result2 == result1);
            }
        }

        [Test]
        public void MurmurHash3_x64_128()
        {
            using (var stream = new MemoryStream())
            {
                var key = new byte[256];
                for (var i = 0; i < 256; i++)
                {
                    key[i] = (byte)i;
                    var hash = MurmurHash3.ComputeHash128(key.Take(i).ToArray(), (uint)(256 - i));
                    stream.Write(hash, 0, hash.Length);
                }

                var result = MurmurHash3.ComputeHash128(stream.GetBuffer());
                var first4Bytes = result[0] | ((uint)result[1] << 8) | ((uint)result[2] << 16) | ((uint)result[3] << 24);
                Assert.IsTrue(first4Bytes == 0x6384ba69);
            }
        }

        [Test]
        public void MurmurHash3_x64_128_Stream()
        {
            var data = "The quick brown fox jumps over the lazy dog";
            var bytes = Encoding.ASCII.GetBytes(data);
            using (var stream = new MemoryStream(bytes))
            {
                var result1 = MurmurHash3.ComputeHash128(bytes, 0x9747b28c);
                var result2 = MurmurHash3.ComputeHash128(stream, 0x9747b28c);
                Assert.IsTrue(result2.SequenceEqual(result1));
            }
        }
    }
}
