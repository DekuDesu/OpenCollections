using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using OpenCollections;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace OpenCollections.Benchmark
{
    [RPlotExporter]
    public class PipelineExampleVsLINQBenchmark
    {
        string Path { get; }
        private readonly Random Generator = new Random();

        private List<TestClass> TestData { get; }

        private List<int> Expected { get; }


        public PipelineExampleVsLINQBenchmark()
        {
            TestData = CreateTestData(10000);
            Path = $"{nameof(PipelineExampleVsLINQBenchmark)}TestData.txt";
            WriteTestData();
            Expected = TestData.Select((x) => x.Id).ToList();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void TestPipeline()
        {
            var reader = Factory.CreateReader(Path);

            var producer = Factory.CreateProducer(reader);

            var consumer = Factory.CreateConsumer<string, int>(producer);

            consumer.Operation = (x) => JsonConvert.DeserializeObject<TestClass>(x).Id;

            producer.Produce();

            AssertList(Expected, consumer.ResultCollection.ToArray());
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void ControlPipeline()
        {
            List<int> Ids = new List<int>();
            using (var reader = File.OpenText(Path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Ids.Add(JsonConvert.DeserializeObject<TestClass>(line).Id);
                }
            }
            AssertList(Expected, Ids.ToArray());
        }

        private void WriteTestData()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
            using (var writer = File.CreateText(Path))
            {
                foreach (var item in TestData)
                {
                    writer.WriteLine(JsonConvert.SerializeObject(item));
                }
            }
        }

        private List<TestClass> CreateTestData(int maxObjects)
        {
            var testObjects = new List<TestClass>();
            for (int i = 0; i < maxObjects; i++)
            {
                testObjects.Add(CreateTestObject());
            }
            return testObjects;
        }

        private TestClass CreateTestObject()
        {
            return new TestClass()
            {
                Id = Generator.Next(),
                Name = Generator.Next().ToString(),
                Value = (float)Generator.NextDouble(),
                SubValue = Generator.NextDouble(),
                DataList = new List<string>()
                {
                    Generator.Next().ToString(),
                    Generator.Next().ToString(),
                    Generator.Next().ToString(),
                },
                Matrix = new int[][]{
                    new int[]{
                        1,2,3
                    },
                    new int[]{
                        4,5,6
                    },
                    new int[]{
                        7,8,9
                    },
                }
            };
        }

        private void AssertList(IEnumerable<int> expected, IEnumerable<int> actual)
        {
            foreach (var item in expected)
            {
                if (actual.Contains(item) == false)
                {
                    throw new Exception("Test Failed");
                }
            }
        }
    }

    public class TestClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Value { get; set; }
        public double SubValue { get; set; }
        public List<string> DataList { get; set; } = new List<string>();

        public int[][] Matrix { get; set; }
    }
}
