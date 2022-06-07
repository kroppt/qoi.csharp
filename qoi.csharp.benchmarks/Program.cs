using BenchmarkDotNet.Running;

namespace qoi.csharp.benchmarks
{
    public class Program
    {
        public static void Main(string[] args) =>
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
