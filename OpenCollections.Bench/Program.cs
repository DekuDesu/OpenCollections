using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OpenCollections;

namespace OpenCollections.Benchmark
{
    class Program
    {
        static void Main()
        {
            //_ = BenchmarkRunner.Run<AsyncPipelineBenchmark>();
            //_ = BenchmarkRunner.Run<EnumerableStreamReaderBenchmark>();
            _ = BenchmarkRunner.Run<PipelineExampleVsLINQBenchmark>();
            Console.Read();
        }
    }
}
