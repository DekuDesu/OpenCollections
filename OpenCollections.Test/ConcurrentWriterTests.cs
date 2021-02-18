using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Concurrent;
using System.Threading;

namespace OpenCollections.Tests
{
    public class ConcurrentWriterTests
    {
        Random Generator = new Random();

        string PathBase = $"Exmaple{nameof(ConcurrentWriterTests)}";

        string Path => $"{PathBase}.txt";

        [Theory]
        [InlineData("")]
        public void ThrowsPathArgumentException(string path)
        {
            var dataQueue = GetRandomNumberList(10);

            var writer = Factory.CreateWriter(path, dataQueue);

            Assert.Throws<System.ArgumentException>(() => writer.WriteLines(false));
        }

        [Fact]
        public void WritesFIFO()
        {
            var dataQueue = GetRandomNumberList();

            string[] expected = dataQueue.ToArray();

            var writer = Factory.CreateWriter(Path, dataQueue);

            writer.WriteLines(false);

            var reader = Factory.CreateReader(Path);

            var producer = Factory.CreateProducer(reader.ReadLine());

            var consumer = Factory.CreateConsumer<string, int?>(producer);

            List<string> actual = new List<string>();

            consumer.Operation = (x) =>
            {
                actual.Add(x);
                return null;
            };

            Task.WaitAll(Task.Run(() => producer.Produce()), Task.Run(() => consumer.Consume()));

            var result = ConcurrentTestHelpers.VerifyCollection(expected, actual);

            Assert.True(result.result, result.message);
        }

        [Fact]
        public void WritesLinesFIFOAsync()
        {
            var dataQueue = GetRandomNumberList();

            string[] expected = dataQueue.ToArray();

            var writer = Factory.CreateWriter(Path, dataQueue);

            Task.WaitAll(writer.WriteLinesAsync(false));

            var reader = Factory.CreateReader(Path);

            var producer = Factory.CreateProducer(reader.ReadLine());

            var consumer = Factory.CreateConsumer<string, int?>(producer);

            List<string> actual = new List<string>();

            consumer.Operation = (x) =>
            {
                actual.Add(x);
                return null;
            };

            Task.WaitAll(producer.ProduceAsync(), consumer.ConsumeAsync());

            var result = ConcurrentTestHelpers.VerifyCollection(expected, actual);

            Assert.True(result.result, result.message);
        }

        [Fact]
        public void CancelThrowsWhenAttemptingToCancelWhenCancelTokenIsManagedByOtherObject()
        {
            var dataQueue = GetRandomNumberList(10);

            var writer = Factory.CreateWriter(Path, dataQueue);

            CancellationTokenSource TokenSource = new CancellationTokenSource();

            Task consumerTask = writer.WriteAsync(TokenSource.Token);

            TaskStatus status = consumerTask.Status;

            Assert.Throws<NotSupportedException>(() => writer.Cancel());
        }

        private ConcurrentQueue<string> GetRandomNumberList(int maxNumbers = 500)
        {
            var numbers = new ConcurrentQueue<string>();
            for (int i = 0; i < maxNumbers; i++)
            {
                numbers.Enqueue($"{Generator.Next()}");
            }
            return numbers;
        }
    }
}
