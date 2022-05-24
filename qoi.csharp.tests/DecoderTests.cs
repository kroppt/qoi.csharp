using Xunit;

namespace Qoi.Csharp.Tests
{
    public class DecoderTests
    {
        [Fact]
        public void ShouldSucceed()
        {
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, 0, 0, 0, 0, 0, 3, 0,
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            Decoder.Decode(input);
        }

        [Fact]
        public void ShouldFailParsingBadMagicBytes()
        {
            var input = new byte[] {
                (byte)'a', (byte)'b', (byte)'c', (byte)'d', 0, 0, 0, 0, 0, 0, 0, 0, 3, 0,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)expectedWidth, 0, 0, 0, (byte)expectedHeight, 3, 0,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, 9, 0,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, 3, 2,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, 3, 0,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, 3, 0,
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
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)width, 0, 0, 0, (byte)height, 3, 0,
                0, 0, 0, 0, 0, 1, 1, 1,
            };

            Assert.Throws<Decoder.InvalidHeaderException>(() =>
            {
                Decoder.Decode(input);
            });
        }
    }
}
