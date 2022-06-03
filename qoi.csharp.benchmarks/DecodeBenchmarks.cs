using System.IO;

using BenchmarkDotNet.Attributes;

using Qoi.Csharp;

namespace qoi.csharp.benchmarks
{
    public class DecodeBenchmarks
    {
        private byte[] _nonAlphaBytes;
        private byte[] _alphaBytes;

        public DecodeBenchmarks()
        {
            _nonAlphaBytes = File.ReadAllBytes("testdata/10x10.qoi");
            _alphaBytes = File.ReadAllBytes("testdata/sample.qoi");
        }

        [Benchmark]
        public Image NonAlphaImage()
        {
            return Decoder.Decode(_nonAlphaBytes);
        }

        [Benchmark]
        public Image AlphaImage()
        {
            return Decoder.Decode(_alphaBytes);
        }
    }
}
