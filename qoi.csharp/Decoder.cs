using System;
using System.Collections.Generic;
using System.IO;

namespace Qoi.Csharp
{
    public class Decoder
    {
        private readonly BinaryReader _binReader;
        private readonly List<byte> _pixelBytes;
        private readonly Pixel[] _cache;
        private Pixel _prev;

        private const int CACHE_SIZE = 64;

        private Decoder(BinaryReader binReader)
        {
            _binReader = binReader;
            _pixelBytes = new List<byte>();
            _cache = new Pixel[64];
            _prev = new Pixel { R = 0, G = 0, B = 0, A = 255, };
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

        private void ParseChunks(uint width, uint height)
        {
            while (_pixelBytes.Count < width * height * 4)
            {
                ParseChunk();
            }
        }

        private int CalculateIndex(Pixel pixel)
        {
            return (pixel.R * 3 + pixel.G * 5 + pixel.B * 7 + pixel.A * 11) % CACHE_SIZE;
        }

        private void ParseChunk()
        {
            var tag = _binReader.ReadByte();
            if ((tag & Tag.MASK) == Tag.INDEX)
            {
                var pixel = _cache[tag];
                _pixelBytes.Add(pixel.R);
                _pixelBytes.Add(pixel.G);
                _pixelBytes.Add(pixel.B);
                _pixelBytes.Add(pixel.A);
                _prev = pixel;
                return;
            }

            if ((tag & Tag.MASK) == Tag.DIFF)
            {
                var dr = (byte)((tag & 0b00_11_00_00) >> 4);
                var dg = (byte)((tag & 0b00_00_11_00) >> 2);
                var db = (byte)((tag & 0b00_00_00_11) >> 0);
                byte bias = 2;
                var pixel = new Pixel
                {
                    R = (byte)(_prev.R + dr - bias),
                    G = (byte)(_prev.G + dg - bias),
                    B = (byte)(_prev.B + db - bias),
                    A = _prev.A,
                };
                _pixelBytes.Add(pixel.R);
                _pixelBytes.Add(pixel.G);
                _pixelBytes.Add(pixel.B);
                _pixelBytes.Add(pixel.A);
                _cache[CalculateIndex(pixel)] = pixel;
                _prev = pixel;
                return;
            }

            if ((tag & Tag.MASK) == Tag.LUMA)
            {
                var dg = (byte)(tag & 0b00_111111) - 32;
                var dxdg = _binReader.ReadByte();
                var drdg = (dxdg & 0b1111_0000) >> 4;
                var dr = (drdg - 8) + dg;
                var dbdg = (dxdg & 0b0000_1111) >> 0;
                var db = (dbdg - 8) + dg;
                var pixel = new Pixel
                {
                    R = (byte)(_prev.R + dr),
                    G = (byte)(_prev.G + dg),
                    B = (byte)(_prev.B + db),
                    A = _prev.A,
                };
                _pixelBytes.Add(pixel.R);
                _pixelBytes.Add(pixel.G);
                _pixelBytes.Add(pixel.B);
                _pixelBytes.Add(pixel.A);
                _cache[CalculateIndex(pixel)] = pixel;
                _prev = pixel;
                return;
            }

            if (tag != Tag.RGB && tag != Tag.RGBA && (tag & Tag.MASK) == Tag.RUN)
            {
                var run = (byte)(tag & ~Tag.MASK);
                var runLength = run + 1;
                var pixel = _prev;
                for (var i = 0; i < runLength; i++)
                {
                    _pixelBytes.Add(pixel.R);
                    _pixelBytes.Add(pixel.G);
                    _pixelBytes.Add(pixel.B);
                    _pixelBytes.Add(pixel.A);
                }
                _cache[CalculateIndex(pixel)] = pixel;
                _prev = pixel;
                return;
            }

            var r = _binReader.ReadByte();
            var g = _binReader.ReadByte();
            var b = _binReader.ReadByte();
            byte a = 255;
            if (tag == Tag.RGB)
            {
            }
            else
            {
                a = _binReader.ReadByte();
            }
            _pixelBytes.Add(r);
            _pixelBytes.Add(g);
            _pixelBytes.Add(b);
            _pixelBytes.Add(a);
            var newPixel = new Pixel { R = r, G = g, B = b, A = a };
            _cache[CalculateIndex(newPixel)] = newPixel;
            _prev = newPixel;
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
            ParseChunks(width, height);
            ParseEndMarker();
            var bytes = _pixelBytes.ToArray();
            return new Image(bytes, width, height, Channels.Rgba, ColorSpace.SRgb);
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
