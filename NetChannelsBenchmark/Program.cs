using System;
using BenchmarkDotNet.Running;

namespace NetChannelsBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BigOjectChannelBenchmark>();

            Console.WriteLine(summary);

            Console.Read();
        }
    }
}
