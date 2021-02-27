using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OpenCollections;

namespace OpenCollections.Bench
{
    class Program
    {
        static void Main()
        {
            //_ = BenchmarkRunner.Run<AsyncPipelineBenchmark>();
            //_ = BenchmarkRunner.Run<EnumerableStreamReaderBenchmark>();
            //_ = BenchmarkRunner.Run<PipelineExampleVsLINQBenchmark>();
            _ = BenchmarkRunner.Run<HttpPipelineVsConsumerPipeline>();
            Console.Read();
        }
    }
}
