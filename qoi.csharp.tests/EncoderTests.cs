using System;
using System.IO;

using Xunit;

namespace Qoi.Csharp.Tests
{
    public class EncoderTests
    {
        [Fact]
        public void ShouldSucceed()
        {
            var input = new byte[] { 0, 0, 0, 255 };

            var bytes = Encoder.Encode(input, 1, 1, Channels.Rgba, ColorSpace.SRgb);

            Assert.NotNull(bytes);
        }

        [Fact]
        public void ShouldHaveCorrectHeader()
        {
            var width = 1;
            var height = 1;
            byte[] expected;
            using (var memStream = new MemoryStream())
            {
                using (var binWriter = new BinaryWriter(memStream))
                {
                    binWriter.Write((byte)'q');
                    binWriter.Write((byte)'o');
                    binWriter.Write((byte)'i');
                    binWriter.Write((byte)'f');
                    WriteBigEndian(binWriter, width);
                    WriteBigEndian(binWriter, height);
                    binWriter.Write((byte)Channels.Rgba);
                    binWriter.Write((byte)ColorSpace.SRgb);
                }
                expected = memStream.ToArray();
            }
            var input = new byte[] { 0, 0, 0, 255 };

            var bytes = Encoder.Encode(input, 1, 1, Channels.Rgba, ColorSpace.SRgb);

            var actual = new ArraySegment<byte>(bytes, 0, 14);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldHaveCorrectEndMarker()
        {
            var expected = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 };
            var input = new byte[] { 100, 0, 0, 255 };

            var bytes = Encoder.Encode(input, 1, 1, Channels.Rgba, ColorSpace.SRgb);

            var actual = new ArraySegment<byte>(bytes, bytes.Length - 8, 8);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldHaveRgbaChunk()
        {
            var expected = new byte[] { 0b11111111, 0, 0, 0, 128 };
            var input = new byte[] {
                0, 0, 0, 128,
                0, 0, 0, 128,
                0, 0, 0, 128,
                0, 0, 0, 128,
            };

            var bytes = Encoder.Encode(input, 2, 2, Channels.Rgba, ColorSpace.SRgb);

            var actual = new ArraySegment<byte>(bytes, 14, 5);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldHaveRgbChunk()
        {
            var expected = new byte[] { 0b11111110, 128, 0, 0 };
            var input = new byte[] {
                128, 0, 0, 255,
                128, 0, 0, 255,
                128, 0, 0, 255,
                128, 0, 0, 255,
            };

            var bytes = Encoder.Encode(input, 2, 2, Channels.Rgba, ColorSpace.SRgb);

            var actual = new ArraySegment<byte>(bytes, 14, 4);
            Assert.Equal(expected, actual);
        }

        private static void WriteBigEndian(BinaryWriter binWriter, int value)
        {
            binWriter.Write((byte)((value >> 030) & 0xFF));
            binWriter.Write((byte)((value >> 020) & 0xFF));
            binWriter.Write((byte)((value >> 010) & 0xFF));
            binWriter.Write((byte)((value >> 000) & 0xFF));
        }
    }
}
