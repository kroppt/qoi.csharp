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

        [Fact]
        public void ShouldHaveIndexChunk()
        {
            byte expected = 53;
            var input = new byte[] {
                128, 0, 0, 255, // RGB chunk
                0, 127, 0, 255, // RGB chunk
                128, 0, 0, 255, // index chunk
                0, 127, 0, 255, // index chunk
            };

            var bytes = Encoder.Encode(input, 2, 2, Channels.Rgba, ColorSpace.SRgb);

            var actual = bytes[22];
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldHaveDiffChunk()
        {
            byte expected = 0b_01_11_10_10;
            var input = new byte[] {
                128, 0, 0, 255, // RGB chunk
                129, 0, 0, 255, // diff chunk
            };

            var bytes = Encoder.Encode(input, 2, 1, Channels.Rgba, ColorSpace.SRgb);

            var actual = bytes[18];
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldHaveDiffChunkWithWraparound()
        {
            byte expected = 0b_01_10_11_01;
            var input = new byte[] {
                128, 255, 0, 255, // RGB chunk
                128, 0, 255, 255, // diff chunk
            };

            var bytes = Encoder.Encode(input, 2, 1, Channels.Rgba, ColorSpace.SRgb);

            var actual = bytes[18];
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldHaveLumaChunk()
        {
            var expected = new byte[] { 0b_10_111111, 0b_0000_1111 };
            var input = new byte[] {
                128, 0, 0, 255,
                151, 31, 38, 255,
            };

            var bytes = Encoder.Encode(input, 2, 1, Channels.Rgba, ColorSpace.SRgb);

            var actual = new ArraySegment<byte>(bytes, 18, 2);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldHaveLumaChunkWraparound()
        {
            var expected = new byte[] { 0b_10_100010, 0b_0110_0101 };
            var input = new byte[] {
                128, 255, 0, 255,
                128, 1, 255, 255,
            };

            var bytes = Encoder.Encode(input, 2, 1, Channels.Rgba, ColorSpace.SRgb);

            var actual = new ArraySegment<byte>(bytes, 18, 2);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldHaveRunChunk()
        {
            byte expected = 0b_11_000010;
            var input = new byte[] {
                128, 0, 0, 255,
                128, 0, 0, 255,
                128, 0, 0, 255,
                128, 0, 0, 255,
                128, 129, 0, 255,
            };

            var bytes = Encoder.Encode(input, 5, 1, Channels.Rgba, ColorSpace.SRgb);

            var actual = bytes[18];
            Assert.Equal(expected, actual);
        }

        private static void WriteBigEndian(BinaryWriter binWriter, int value)
        {
            binWriter.Write((byte)((value >> 24) & 0xFF));
            binWriter.Write((byte)((value >> 16) & 0xFF));
            binWriter.Write((byte)((value >> 08) & 0xFF));
            binWriter.Write((byte)((value >> 00) & 0xFF));
        }
    }
}
