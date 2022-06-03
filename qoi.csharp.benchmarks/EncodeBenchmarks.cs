using System.IO;

using BenchmarkDotNet.Attributes;

using Qoi.Csharp;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace qoi.csharp.benchmarks
{
    public class EncodeBenchmarks
    {
        private byte[] _nonAlphaBytes;
        private int _nonAlphaWidth;
        private int _nonAlphaHeight;
        private byte[] _alphaBytes;
        private int _alphaWidth;
        private int _alphaHeight;

        public EncodeBenchmarks()
        {
            using (Image<Rgb24> png = SixLabors.ImageSharp.Image.Load<Rgb24>("testdata/10x10.png"))
            {
                _nonAlphaWidth = png.Width;
                _nonAlphaHeight = png.Height;
                _nonAlphaBytes = new byte[png.Width * png.Height * 3];
                png.CopyPixelDataTo(_nonAlphaBytes);
            }
            using (Image<Rgba32> png = SixLabors.ImageSharp.Image.Load<Rgba32>("testdata/sample.png"))
            {
                _alphaWidth = png.Width;
                _alphaHeight = png.Height;
                _alphaBytes = new byte[png.Width * png.Height * 4];
                png.CopyPixelDataTo(_nonAlphaBytes);
            }
        }

        [Benchmark]
        public byte[] NonAlphaImage()
        {
            return Encoder.Encode(_nonAlphaBytes, _nonAlphaWidth, _nonAlphaHeight, Channels.Rgb, ColorSpace.SRgb);
        }

        [Benchmark]
        public byte[] AlphaImage()
        {
            return Encoder.Encode(_alphaBytes, _alphaWidth, _alphaHeight, Channels.Rgb, ColorSpace.SRgb);
        }
    }
}
