namespace Qoi.Csharp
{
    public class Decoder
    {
        private readonly byte[] _input;

        private Decoder(byte[] input)
        {
            _input = input;
        }

        public static Image Decode(byte[] input)
        {
            return new Image(new byte[] { }, Channels.Rgba, ColorSpace.SRgb);
        }
    }
}
