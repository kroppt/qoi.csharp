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
        private readonly Pixel[] _cache;

        private const int CACHE_SIZE = 64;

        private Encoder(BinaryWriter binWriter, byte[] input, int width, int height, Channels channels, ColorSpace colorSpace)
        {
            _binWriter = binWriter;
            _input = input;
            _width = width;
            _height = height;
            _channels = channels;
            _colorSpace = colorSpace;
            _cache = new Pixel[CACHE_SIZE];
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
            _binWriter.Write((byte)((value >> 24) & 0xFF));
            _binWriter.Write((byte)((value >> 16) & 0xFF));
            _binWriter.Write((byte)((value >> 08) & 0xFF));
            _binWriter.Write((byte)((value >> 00) & 0xFF));
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
            for (int i = 0; i < _input.Length; i += pixelSize)
            {
                var alpha = _channels == Channels.Rgba ? _input[i + 3] : (byte)255;
                var next = new Pixel(_input[i], _input[i + 1], _input[i + 2], alpha);
                var index = CalculateIndex(next);
                if (_cache[index].Equals(next))
                {
                    WriteIndexChunk(index);
                }
                else if (prev.A == next.A)
                {
                    var diff = new Diff(prev, next);
                    var lumaDiff = new LumaDiff(prev, next);
                    if (diff.IsSmall())
                    {
                        WriteDiffChunk(diff);
                    }
                    else if (lumaDiff.IsSmall())
                    {
                        WriteLumaChunk(lumaDiff);
                    }
                    else
                    {
                        WriteRgbChunk(next);
                    }
                    _cache[index] = next;
                }
                else
                {
                    WriteRgbaChunk(next);
                    _cache[index] = next;
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

        private void WriteIndexChunk(int index)
        {
            _binWriter.Write((byte)index);
        }

        private void WriteDiffChunk(Diff diff)
        {
            byte chunk = 0b_01_00_00_00;
            chunk |= (byte)(diff.R << 4);
            chunk |= (byte)(diff.G << 2);
            chunk |= (byte)(diff.B << 0);
            _binWriter.Write(chunk);
        }

        private void WriteLumaChunk(LumaDiff lumaDiff)
        {
            byte byte1 = 0b_10_000000;
            byte1 |= lumaDiff.G;
            byte byte2 = 0;
            byte2 |= (byte)(lumaDiff.RG << 4);
            byte2 |= (byte)(lumaDiff.BG << 0);
            _binWriter.Write(byte1);
            _binWriter.Write(byte2);
        }

        private int CalculateIndex(Pixel pixel)
        {
            return (pixel.R * 3 + pixel.G * 5 + pixel.B * 7 + pixel.A * 11) % CACHE_SIZE;
        }
    }
}
