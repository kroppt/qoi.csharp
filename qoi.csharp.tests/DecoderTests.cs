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

            Assert.Throws<Decoder.BadMagicBytesException>(() =>
            {
                Decoder.Decode(input);
            });
        }

        [Fact]
        public void ShouldCorrectlyParseHeaderWidthAndHeight()
        {
            var expectedWidth = 1u;
            var expectedHeight = 1u;
            var input = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, (byte)expectedWidth, 0, 0, 0, (byte)expectedHeight, 3, 0,
                0b11111110, // RGB tag
                128, // red
                0, // green
                0, // blue
                0, 0, 0, 0, 0, 0, 0, 1,
            };

            var actual = Decoder.Decode(input);

            Assert.Equal(expectedWidth, actual.Width);
            Assert.Equal(expectedHeight, actual.Height);
        }
    }
}
