using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BenchmarkDotNet;

namespace OpenCollections.Benchmark
{
    /// <summary>
    /// Benchmarks the performance impact of implementing a <see cref="IEnumerable{string}"/> version of StreamReader
    /// </summary>
    public class EnumerableStreamReaderBenchmark
    {
        public string Path { get; }

        public int MaxNumbers { get; }

        Random Generator { get; set; } = new Random();

        public EnumerableStreamReaderBenchmark(string path = @"C:\$Programming\C# Projects\Heater1\bin\Debug\Primes.txt", int maxNumbers = 50000)
        {
            Path = path;
            MaxNumbers = maxNumbers;
            //CreateTestFile();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void StreamReader()
        {
            List<string> results = new List<string>();
            using (var reader = File.OpenText(Path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    results.Add(line);
                }
            }
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void EnumberableReader()
        {
            List<string> results = new List<string>();
            foreach (var item in Factory.CreateReader(Path))
            {
                results.Add(item);
            }
        }

        private void CreateTestFile()
        {
            var TestData = GetRandomNumberList();

            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
            using (var writer = File.CreateText(Path))
            {
                foreach (var item in TestData)
                {
                    writer.WriteLine(item);
                }
            }
        }

        private List<string> GetRandomNumberList(int maxNumbers = 1000)
        {
            List<string> numbers = new List<string>();
            for (int i = 0; i < maxNumbers; i++)
            {
                numbers.Add($"{Generator.Next(-99999, 99999)}");
            }
            return numbers;
        }
    }
}
