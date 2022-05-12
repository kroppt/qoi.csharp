using System;
using System.IO;

namespace Qoi.Csharp
{
    public class Encoder
    {
        private readonly BinaryWriter _binWriter;
        private readonly byte[] _input;
        private readonly int _width;
        private readonly int _height;
        private readonly Channels _channels;
        private readonly ColorSpace _colorSpace;

        private Encoder(BinaryWriter binWriter, byte[] input, int width, int height, Channels channels, ColorSpace colorSpace)
        {
            _binWriter = binWriter;
            _input = input;
            _width = width;
            _height = height;
            _channels = channels;
            _colorSpace = colorSpace;
        }

        public static byte[] Encode(byte[] input, int width, int height, Channels channels, ColorSpace colorSpace)
        {
            using (var memStream = new MemoryStream())
            {
                using (var binWriter = new BinaryWriter(memStream))
                {
                    var encoder = new Encoder(binWriter, input, width, height, channels, colorSpace);
                    encoder.Encode();
                }
                return memStream.ToArray();
            }
        }

        private void Encode()
        {
            WriteHeader();
            WriteChunks();
            WriteEndMarker();
        }

        private void WriteBigEndian(int value)
        {
            _binWriter.Write((byte)((value >> 030) & 0xFF));
            _binWriter.Write((byte)((value >> 020) & 0xFF));
            _binWriter.Write((byte)((value >> 010) & 0xFF));
            _binWriter.Write((byte)((value >> 000) & 0xFF));
        }

        private void WriteHeader()
        {
            _binWriter.Write((byte)'q');
            _binWriter.Write((byte)'o');
            _binWriter.Write((byte)'i');
            _binWriter.Write((byte)'f');
            WriteBigEndian(_width);
            WriteBigEndian(_height);
            _binWriter.Write((byte)_channels);
            _binWriter.Write((byte)_colorSpace);
        }

        private void WriteEndMarker()
        {
            _binWriter.Write((byte)0);
            _binWriter.Write((byte)0);
            _binWriter.Write((byte)0);
            _binWriter.Write((byte)0);
            _binWriter.Write((byte)0);
            _binWriter.Write((byte)0);
            _binWriter.Write((byte)0);
            _binWriter.Write((byte)1);
        }

        private struct Pixel
        {
            public Pixel(byte r, byte g, byte b, byte a) { R = r; G = g; B = b; A = a; }

            public byte R;
            public byte G;
            public byte B;
            public byte A;
        }

        private void WriteChunks()
        {
            int pixelSize;
            switch (_channels)
            {
                case Channels.Rgb:
                    pixelSize = 3;
                    break;
                case Channels.Rgba:
                    pixelSize = 4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var prev = new Pixel(0, 0, 0, 255);
            for (int i = 0; i < _input.Length / pixelSize; i += pixelSize)
            {
                var alpha = _channels == Channels.Rgba ? _input[i + 3] : (byte)255;
                var next = new Pixel(_input[i], _input[i + 1], _input[i + 2], alpha);
                if (prev.A == next.A)
                {
                    WriteRgbChunk(next);
                }
                else
                {
                    WriteRgbaChunk(next);
                }
                prev = next;
            }
        }

        private void WriteRgbChunk(Pixel pixel)
        {
            _binWriter.Write((byte)0b11111110);
            _binWriter.Write(pixel.R);
            _binWriter.Write(pixel.G);
            _binWriter.Write(pixel.B);
        }

        private void WriteRgbaChunk(Pixel pixel)
        {
            _binWriter.Write((byte)0b11111111);
            _binWriter.Write(pixel.R);
            _binWriter.Write(pixel.G);
            _binWriter.Write(pixel.B);
            _binWriter.Write(pixel.A);
        }
    }
}
