using BenchmarkDotNet.Running;

namespace qoi.csharp.benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run<DecodeBenchmarks>();
            _ = BenchmarkRunner.Run<EncodeBenchmarks>();
        }
    }
}
