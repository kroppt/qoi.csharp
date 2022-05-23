using System;
using System.IO;

namespace Qoi.Csharp
{
    public class Decoder
    {
        private readonly BinaryReader _binReader;

        private Decoder(BinaryReader binReader)
        {
            _binReader = binReader;
        }

        public static Image Decode(byte[] input)
        {
            using (var memStream = new MemoryStream(input))
            {
                using (var binReader = new BinaryReader(memStream))
                {
                    var decoder = new Decoder(binReader);
                    return decoder.Decode();
                }
            }
        }

        private void ParseMagic()
        {
            var correctMagic = new byte[] {
                (byte)'q', (byte)'o', (byte)'i', (byte)'f',
            };
            var actualMagic = new byte[] {
                _binReader.ReadByte(),
                _binReader.ReadByte(),
                _binReader.ReadByte(),
                _binReader.ReadByte(),
            };
            if (actualMagic.Length < correctMagic.Length)
            {
                throw new BadMagicBytesException();
            }
            for (int i = 0; i < actualMagic.Length; i++)
            {
                if (actualMagic[i] != correctMagic[i])
                {
                    throw new BadMagicBytesException();
                }
            }
        }

        private uint ReadUInt32BigEndian()
        {
            var value = 0u;
            value |= (uint)(_binReader.ReadByte() << 24);
            value |= (uint)(_binReader.ReadByte() << 16);
            value |= (uint)(_binReader.ReadByte() << 08);
            value |= (uint)(_binReader.ReadByte() << 00);
            return value;
        }

        private Image Decode()
        {
            ParseMagic();
            var width = ReadUInt32BigEndian();
            var height = ReadUInt32BigEndian();
            return new Image(new byte[] { }, width, height, Channels.Rgba, ColorSpace.SRgb);
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
