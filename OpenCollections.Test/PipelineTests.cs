using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using OpenCollections;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace OpenCollections.Tests
{
    public class PipelineTests
    {
        Random Generator = new Random();

        string PathBase = $@"Example{nameof(PipelineTests)}";

        string Path => $@"{PathBase}.txt";

        [Fact]
        public void BasicStringPipeline()
        {
            var expected = CreateTestFile(Path);

            var Reader = Factory.CreateReader(Path);

            var producer = Factory.CreateProducer(Reader.ReadLine());

            var consumer = Factory.CreateConsumer<string, int>(producer);

            consumer.Operation = (x) => int.Parse(x);

            producer.Produce();

            consumer.Consume();

            IEnumerable<int> actual = consumer.ResultCollection;

            var casted = actual.Select((x) => x.ToString());

            (bool result, string error) = Helpers.VerifyCollection(expected, casted);

            Assert.True(result, error);
        }

        [Fact]
        public void BasicStringAsyncPipeline()
        {
            var expected = CreateTestFile(Path);

            var Reader = Factory.CreateReader(Path);

            var producer = Factory.CreateProducer(Reader.ReadLine());

            var consumer = Factory.CreateConsumer<string, int>(producer);

            consumer.Operation = (x) => int.Parse(x);

            var consumer2 = Factory.CreateConsumer<int, string>(consumer);

            consumer2.Operation = (x) => x.ToString() + "-Op2";

            Task.WaitAll(producer.ProduceAsync(), consumer.ConsumeAsync(), consumer2.ConsumeAsync());

            IEnumerable<string> actual = consumer2.ResultCollection;

            var casted = expected.Select((x) => x.ToString() + "-Op2");

            (bool result, string error) = Helpers.VerifyCollection(casted, actual);

            Assert.True(result, error);
        }

        [Fact]
        public void BasicMutliConsumerPipeline()
        {
            string path = $"{nameof(BasicMutliConsumerPipeline)}.txt";

            var testData = CreateTestFile(path);

            var expected = testData.ToArray().Select((x) => int.Parse(x)).Select((x) => x + 1).Select((x) => x + 1);

            var reader = Factory.CreateReader(path);

            var producer = Factory.CreateProducer(reader);

            var intparser = Factory.CreateConsumer<string, int>(producer);

            intparser.Operation = (x) => int.Parse(x);

            var multiplier = Factory.CreateConsumer<int, int>(intparser);

            multiplier.Operation = (x) => x + 1;

            var multiplier2 = Factory.CreateConsumer<int, int>(multiplier);

            multiplier2.Operation = (x) => x + 1;

            Task.WaitAll(producer.ProduceAsync(), intparser.ConsumeAsync(), multiplier.ConsumeAsync(), multiplier2.ConsumeAsync());

            var actual = Helpers.VerifyCollection(expected, multiplier2.ResultCollection);

            Assert.True(actual.result, actual.message);
        }

        [Fact]
        public void BasicFileReadingPipeline()
        {
            string path = $"{nameof(BasicFileReadingPipeline)}.txt";

            var testData = CreateTestFile(path);
            var expected = testData.ToArray().Select((x) => int.Parse(x));

            var reader = Factory.CreateReader(path);

            var producer = Factory.CreateProducer(reader);

            var consumer = Factory.CreateConsumer<string, int>(producer);

            consumer.Operation = (x) => int.Parse(x);

            Task.WaitAll(producer.ProduceAsync(), consumer.ConsumeAsync());

            var actual = Helpers.VerifyCollection(expected, consumer.ResultCollection);

            Assert.True(actual.result, actual.message);
        }

        private (string[] paths, List<TestClass> expected) CreateTestFlatFiles()
        {
            string[] paths =
                {
                $"{PathBase}1.txt",
                $"{PathBase}2.txt",
                $"{PathBase}3.txt",
                $"{PathBase}4.txt",
                $"{PathBase}5.txt",
                $"{PathBase}6.txt",
                $"{PathBase}7.txt",
                $"{PathBase}8.txt",
                $"{PathBase}9.txt",
                $"{PathBase}10.txt",
            };

            var expected = new List<IEnumerable<TestClass>>();

            // create the flat files
            foreach (var item in paths)
            {
                expected.Add(CreateTestFlatFiles(item));
            }

            var joined = new List<TestClass>();

            foreach (var item in expected)
            {
                foreach (var item1 in item)
                {
                    joined.Add(item1);
                }
            }
            return (paths, joined);
        }

        private IEnumerable<string> CreateTestFile(string path, IEnumerable<string> TestData = null)
        {
            TestData = TestData ?? GetRandomNumberList();

            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
            using (var writer = File.CreateText(path))
            {
                foreach (var item in TestData)
                {
                    writer.WriteLine(item);
                }
            }
            return TestData;
        }

        private IEnumerable<TestClass> CreateTestFlatFiles(string path, IEnumerable<TestClass> TestData = null)
        {
            var randomIDs = GetRandomNumberList();

            var testClasses = new List<TestClass>();

            int current = 0;

            foreach (var item in randomIDs)
            {
                testClasses.Add(new TestClass(item)
                {
                    Value = current++
                });
            }

            TestData = TestData ?? testClasses;

            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
            using (var writer = File.CreateText(path))
            {
                foreach (var item in TestData)
                {
                    writer.WriteLine(JsonConvert.SerializeObject(item));
                }
            }
            return TestData;
        }

        private List<string> GetRandomNumberList(int maxNumbers = 500)
        {
            List<string> numbers = new List<string>();
            for (int i = 0; i < maxNumbers; i++)
            {
                numbers.Add($"{Generator.Next(int.MinValue >> 1, int.MaxValue >> 1)}");
            }
            return numbers;
        }

        private class TestClass : IEquatable<TestClass>
        {
            public string Data { get; set; }
            public float Value { get; set; }
            public string[] SubData { get; set; }

            public bool Created { get; }

            public TestClass(string data)
            {
                this.Data = data ?? "emptyData";
                this.Value = 475.3345f;
                this.SubData = new string[] { "data1", "data2" };
                this.Created = true;
            }

            public bool Equals(TestClass other)
            {
                return Data == other.Data && Value == other.Value;
            }

            public override string ToString()
            {
                return $"Data: {Data} Value: {Value} Created: {Created}";
            }
        }
    }
}
