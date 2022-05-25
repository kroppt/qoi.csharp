using Xunit;

namespace Qoi.Csharp.Tests
{
    public class DecoderTests
    {
        [Fact]
        public void ShouldSucceed()
        {
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, 0, 0, 0, 0, 0, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            Decoder.Decode(input);
        }

        [Fact]
        public void ShouldFailParsingBadMagicBytes()
        {
            var input = new byte[] {
                (byte)'a', (byte)'b', (byte)'c', (byte)'d', 0, 0, 0, 0, 0, 0, 0, 0, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            Assert.Throws<Decoder.InvalidHeaderException>(() =>
            {
                Decoder.Decode(input);
            });
        }

        [Fact]
        public void ShouldCorrectlyParseHeaderWidthAndHeight()
        {
            var expectedWidth = 0u;
            var expectedHeight = 0u;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)expectedWidth, 0, 0, 0, (byte)expectedHeight, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var actual = Decoder.Decode(input);

            Assert.Equal(expectedWidth, actual.Width);
            Assert.Equal(expectedHeight, actual.Height);
        }

        [Fact]
        public void ShouldFailParsingBadChannels()
        {
            var width = 0;
            var height = 0;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, 9, (byte)ColorSpace.SRgb,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            Assert.Throws<Decoder.InvalidHeaderException>(() =>
            {
                Decoder.Decode(input);
            });
        }

        [Fact]
        public void ShouldFailParsingBadColorSpace()
        {
            var width = 0;
            var height = 0;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, (byte)Channels.Rgb, 2,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            Assert.Throws<Decoder.InvalidHeaderException>(() =>
            {
                Decoder.Decode(input);
            });
        }

        [Fact]
        public void ShouldFailParsingMissingEndMarker()
        {
            var width = 0;
            var height = 0;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
            };

            Assert.Throws<Decoder.InvalidHeaderException>(() =>
            {
                Decoder.Decode(input);
            });
        }

        [Fact]
        public void ShouldFailParsingPartialEndMarker()
        {
            var width = 0;
            var height = 0;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                0, 0, 0, 0, 0,
            };

            Assert.Throws<Decoder.InvalidHeaderException>(() =>
            {
                Decoder.Decode(input);
            });
        }

        [Fact]
        public void ShouldFailParsingBadEndMarker()
        {
            var width = 0;
            var height = 0;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                0, 0, 0, 0, 0, 1, 1, 1,
            };

            Assert.Throws<Decoder.InvalidHeaderException>(() =>
            {
                Decoder.Decode(input);
            });
        }

        [Fact]
        public void ShouldParseRGBChunk()
        {
            byte size = 1;
            var expected = new byte[]{
                128, 0, 0, 255,
            };
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, size, 0, 0, 0, size, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                0b11111110,
                128,
                0,
                0,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.Bytes;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldParseRGBAChunk()
        {
            byte size = 1;
            var expected = new byte[]{
                128, 0, 0, 128,
            };
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, size, 0, 0, 0, size, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                0b11111111,
                128,
                0,
                0,
                128,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.Bytes;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldParseIndexChunk()
        {
            byte width = 3;
            byte height = 1;
            var expected = new byte[] {
                128, 0, 0, 255,
                0, 127, 0, 255,
                128, 0, 0, 255,
            };
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, width, 0, 0, 0, height, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                0b11111110, // RGB
                128, // red
                0, // green
                0, // blue
                0b11111110, // RGB
                0, // red
                127, // green
                0, // blue
                0b00_000000 | 53, // index
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.Bytes;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldParseDiffChunk()
        {
            byte width = 2;
            byte height = 1;
            var expected = new byte[] {
                128, 0, 0, 255,
                129, 0, 0, 255,
            };
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, width, 0, 0, 0, height, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                0b11111110, // RGB
                128, // red
                0, // green
                0, // blue
                0b01_000000 | 0b00_11_10_10, // diff
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.Bytes;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldParseDiffChunkWithWraparound()
        {
            byte width = 2;
            byte height = 1;
            var expected = new byte[] {
                128, 255, 0, 255,
                128, 0, 255, 255,
            };
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, width, 0, 0, 0, height, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                0b11111110, // RGB
                128, // red
                255, // green
                0, // blue
                0b01_000000 | 0b00_10_11_01, // diff
                0, 0, 0, 0, 0, 0, 0, 1,
            };
        }
    }
}
