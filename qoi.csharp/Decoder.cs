using System;

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
            var decoder = new Decoder(input);
            decoder.ParseHeader();
            return new Image(new byte[] { }, Channels.Rgba, ColorSpace.SRgb);
        }

        private void ParseHeader()
        {
            var magic = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f',
            };
            if (_input.Length < magic.Length)
            {
                throw new BadMagicBytesException();
            }
            for (int i = 0; i < magic.Length; i++)
            {
                if (magic[i] != _input[i])
                {
                    throw new BadMagicBytesException();
                }
            }
        }

        public class BadMagicBytesException : Exception
        {
            public BadMagicBytesException() : base()
            {
            }

            public BadMagicBytesException(string message) : base(message)
            {
            }

            public BadMagicBytesException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
