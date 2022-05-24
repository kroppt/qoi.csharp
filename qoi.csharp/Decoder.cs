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
            var actualMagic = _binReader.ReadBytes(4);
            if (actualMagic.Length < correctMagic.Length)
            {
                throw new InvalidHeaderException("Magic bytes were invalid.");
            }
            for (int i = 0; i < actualMagic.Length; i++)
            {
                if (actualMagic[i] != correctMagic[i])
                {
                    throw new InvalidHeaderException("Magic bytes were invalid.");
                }
            }
        }

        private void ParseChannels()
        {
            var channels = (Channels)_binReader.ReadByte();
            if (!Enum.IsDefined(typeof(Channels), channels))
            {
                throw new InvalidHeaderException($"Value {channels} for Channels is not valid.");
            }
        }

        private void ParseColorSpace()
        {
            var colorSpace = (ColorSpace)_binReader.ReadByte();
            if (!Enum.IsDefined(typeof(ColorSpace), colorSpace))
            {
                throw new InvalidHeaderException($"Value {colorSpace} for ColorSpace is not valid.");
            }
        }

        private void ParseEndMarker()
        {
            var correctEndMarker = new byte[] {
                (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)1,
            };
            var actualEndMarker = _binReader.ReadBytes(8);
            if (actualEndMarker.Length < correctEndMarker.Length)
            {
                throw new InvalidHeaderException("End marker is invalid.");
            }
            for (int i = 0; i < actualEndMarker.Length; i++)
            {
                if (actualEndMarker[i] != correctEndMarker[i])
                {
                    throw new InvalidHeaderException("End marker is invalid.");
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
            ParseChannels();
            ParseColorSpace();
            ParseEndMarker();
            return new Image(new byte[] { }, width, height, Channels.Rgba, ColorSpace.SRgb);
        }

        public class InvalidHeaderException : Exception
        {
            public InvalidHeaderException() : base()
            {
            }

            public InvalidHeaderException(string message) : base(message)
            {
            }

            public InvalidHeaderException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
