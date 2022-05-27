using Xunit;

namespace Qoi.Csharp.Tests
{
    public class DecoderTests
    {
        [Fact]
        public void ShouldSucceed()
        {
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, 0, 0, 0, 0, 0, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            Decoder.Decode(input);
        }

        [Fact]
        public void ShouldFailParsingBadMagicBytes()
        {
            var input = new byte[] {
                (byte)'a', (byte)'b', (byte)'c', (byte)'d', 0, 0, 0, 0, 0, 0, 0, 0, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)expectedWidth, 0, 0, 0, (byte)expectedHeight, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, (byte)Channels.Rgba, 2,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                0, 0, 0, 0, 0, 1, 1, 1,
            };

            Assert.Throws<Decoder.InvalidHeaderException>(() =>
            {
                Decoder.Decode(input);
            });
        }

        [Fact]
        public void ShouldParseChannelsRGBA()
        {
            var expected = Channels.Rgba;
            var width = 0;
            var height = 0;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.Channels;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldParseChannelsRGB()
        {
            var expected = Channels.Rgb;
            var width = 0;
            var height = 0;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.Channels;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldParseColorSpaceSRgb()
        {
            var expected = ColorSpace.SRgb;
            var width = 0;
            var height = 0;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.ColorSpace;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldParseColorSpaceLinear()
        {
            var expected = ColorSpace.Linear;
            var width = 0;
            var height = 0;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, (byte)Channels.Rgb, (byte)ColorSpace.Linear,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.ColorSpace;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldParseRGBChunk()
        {
            byte size = 1;
            var expected = new byte[]{
                128, 0, 0, 255,
            };
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, size, 0, 0, 0, size, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                Tag.RGB,
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
        public void ShouldHaveBytesLengthBasedOnRGBChannels()
        {
            byte width = 1;
            byte height = 1;
            var expectedBytesLength = width * height * 3;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, width, 0, 0, 0, height, (byte)Channels.Rgb, (byte)ColorSpace.SRgb,
                Tag.RGB,
                128,
                0,
                0,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var actual = Decoder.Decode(input);

            Assert.Equal(expectedBytesLength, actual.Bytes.Length);
        }

        [Fact]
        public void ShouldHaveBytesLengthBasedOnRGBAChannels()
        {
            byte width = 1;
            byte height = 1;
            var expectedBytesLength = width * height * 4;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, width, 0, 0, 0, height, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                Tag.RGB,
                128,
                0,
                0,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var actual = Decoder.Decode(input);

            Assert.Equal(expectedBytesLength, actual.Bytes.Length);
        }

        [Fact]
        public void ShouldParseRGBAChunk()
        {
            byte size = 1;
            var expected = new byte[]{
                128, 0, 0, 128,
            };
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, size, 0, 0, 0, size, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                Tag.RGBA,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, width, 0, 0, 0, height, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                Tag.RGB,
                128, // red
                0, // green
                0, // blue
                Tag.RGB,
                0, // red
                127, // green
                0, // blue
                Tag.INDEX | 53,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, width, 0, 0, 0, height, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                Tag.RGB,
                128, // red
                0, // green
                0, // blue
                Tag.DIFF | 0b00_11_10_10,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, width, 0, 0, 0, height, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                Tag.RGB,
                128, // red
                255, // green
                0, // blue
                Tag.DIFF | 0b00_10_11_01,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.Bytes;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldParseLumaChunk()
        {
            byte width = 2;
            byte height = 1;
            var expected = new byte[] {
                128, 0, 0, 255,
                151, 31, 38, 255,
            };
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, width, 0, 0, 0, height, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                Tag.RGB,
                128, // red
                0, // green
                0, // blue
                Tag.LUMA | 0b00_111111,
                0b0000_1111,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.Bytes;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldParseLumaChunkWithWraparound()
        {
            byte width = 2;
            byte height = 1;
            var expected = new byte[] {
                128, 255, 0, 255,
                128, 1, 255, 255,
            };
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, width, 0, 0, 0, height, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                Tag.RGB,
                128, // red
                255, // green
                0, // blue
                Tag.LUMA | 0b00_100010,
                0b0110_0101,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.Bytes;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldParseRunChunk()
        {
            byte width = 5;
            byte height = 1;
            var expected = new byte[] {
                128, 0, 0, 255,
                128, 0, 0, 255,
                128, 0, 0, 255,
                128, 0, 0, 255,
                128, 129, 0, 255,
            };
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, width, 0, 0, 0, height, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                Tag.RGB,
                128, // red
                0, // green
                0, // blue
                Tag.RUN | 0b00_000010,
                Tag.RGB,
                128, // red
                129, // green
                0, // blue
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.Bytes;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldParseIndexChunkAfterRun()
        {
            byte width = 2;
            byte height = 2;
            var expected = new byte[] {
                0, 0, 0, 255,
                0, 0, 0, 255,
                127, 0, 0, 255,
                0, 0, 0, 255,
            };
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, width, 0, 0, 0, height, (byte)Channels.Rgba, (byte)ColorSpace.SRgb,
                Tag.RUN | 0b_000001, // run 2
                Tag.RGB, 127, 0, 0,  // RGB
                Tag.INDEX | 0b_110101, // index 53
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var image = Decoder.Decode(input);

            var actual = image.Bytes;
            Assert.Equal(expected, actual);
        }
    }
}
