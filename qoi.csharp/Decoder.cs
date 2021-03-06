using System;
using System.Collections.Generic;
using System.IO;

namespace Qoi.Csharp
{
    /// <summary>
    /// Decoder for QOI images.
    /// </summary>
    public class Decoder
    {
        private readonly BinaryReader _binReader;
        private List<byte> _pixelBytes;
        private readonly Pixel[] _cache;
        private Channels? _channels;
        private ColorSpace? _colorSpace;
        private Pixel _prev;

        private const int CACHE_SIZE = 64;

        private Decoder(BinaryReader binReader)
        {
            _binReader = binReader;
            _pixelBytes = null;
            _cache = new Pixel[64];
            _channels = null;
            _colorSpace = null;
            _prev = new Pixel { R = 0, G = 0, B = 0, A = 255, };
        }

        /// <summary>
        /// Decode QOI bytes into a raw image.
        /// </summary>
        /// <param name="input">The raw bytes of a QOI image.</param>
        /// <returns>An image containing color and meta data.</returns>
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
            _channels = (Channels)_binReader.ReadByte();
            if (!Enum.IsDefined(typeof(Channels), _channels))
            {
                throw new InvalidHeaderException($"Value {_channels} for Channels is not valid.");
            }
        }

        private void ParseColorSpace()
        {
            _colorSpace = (ColorSpace)_binReader.ReadByte();
            if (!Enum.IsDefined(typeof(ColorSpace), _colorSpace))
            {
                throw new InvalidHeaderException($"Value {_colorSpace} for ColorSpace is not valid.");
            }
        }

        private void ParseChunks(uint width, uint height)
        {
            var pixelSize = 3;
            if (_channels == Channels.Rgba)
            {
                pixelSize = 4;
            }
            _pixelBytes = new List<byte>((int)(width * height * pixelSize));
            while (_pixelBytes.Count < _pixelBytes.Capacity)
            {
                ParseChunk();
            }
        }

        private int CalculateIndex(Pixel pixel)
        {
            return (pixel.R * 3 + pixel.G * 5 + pixel.B * 7 + pixel.A * 11) % CACHE_SIZE;
        }

        private void WritePixel(Pixel pixel)
        {
            if (_channels == Channels.Rgb)
            {
                _pixelBytes.Add(pixel.R);
                _pixelBytes.Add(pixel.G);
                _pixelBytes.Add(pixel.B);
            }
            else
            {
                _pixelBytes.Add(pixel.R);
                _pixelBytes.Add(pixel.G);
                _pixelBytes.Add(pixel.B);
                _pixelBytes.Add(pixel.A);
            }
        }

        private void ParseChunk()
        {
            var tag = _binReader.ReadByte();
            if ((tag & Tag.MASK) == Tag.INDEX)
            {
                var pixel = _cache[tag];
                WritePixel(pixel);
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
                WritePixel(pixel);
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
                WritePixel(pixel);
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
                    WritePixel(pixel);
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
            var newPixel = new Pixel { R = r, G = g, B = b, A = a };
            WritePixel(newPixel);
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
            return new Image(bytes, width, height, _channels.Value, _colorSpace.Value);
        }

        /// <summary>
        /// Represents errors that occurred parsing a QOI header.
        /// </summary>
        public class InvalidHeaderException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InvalidHeaderException"/> class.
            /// </summary>
            public InvalidHeaderException() : base()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InvalidHeaderException"/> class with a specified error message.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            public InvalidHeaderException(string message) : base(message)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InvalidHeaderException"/> class with a specified error message
            /// and a reference to the inner exception that is the cause of this exception.
            /// </summary>
            /// <param name="message">
            /// The error message that explains the reason for the exception.
            /// </param>
            /// <param name="innerException">
            /// The exception that is the cause of the current exception,
            /// or a null reference (Nothing in Visual Basic) if no inner exception is specified.
            /// </param>
            public InvalidHeaderException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
