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
    }
}
