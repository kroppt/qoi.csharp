using BenchmarkDotNet.Running;

namespace qoi.csharp.benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<DecodeBenchmarks>();
        }
    }
}
