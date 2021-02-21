using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Concurrent;
using System.Threading;
using Moq;
using Autofac.Extras.Moq;
using Autofac;

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
        public void CancelDoesntThrowWhenNotManaged()
        {
            var writer = Factory.CreateWriter("", Factory.CreateProducer(new int[] { 1, 2, 3 }));
            void ThrowsOnSucess()
            {
                writer.Cancel();
                throw new System.ArithmeticException();
            }
            Assert.Throws<ArithmeticException>(ThrowsOnSucess);
        }

        [Theory]
        [InlineData(new string[] { "1", "2", "3" }, "123")]
        public void WritesFIFO(object[] data, string expected)
        {
            string path = $"{PathBase}-{nameof(WritesFIFO)}.txt";
            try
            {
                ConcurrentQueue<object> queue = new ConcurrentQueue<object>();
                foreach (var item in data)
                {
                    queue.Enqueue(item);
                }
                using (var writer = Factory.CreateWriter<object>(path))
                {
                    writer.Collection = queue;
                    writer.Write();
                }
                string actual;
                using (var reader = File.OpenText(path))
                {
                    actual = reader.ReadToEnd();
                }

                Assert.Equal(expected, actual);
            }
            finally {
                File.Delete(path);
            }
        }

        [Theory]
        [InlineData(new string[] { "1", "2", "3" }, "123")]
        public void WritesLinesFIFOAsync(object[] data, string expected)
        {
            string path = $"{PathBase}-{nameof(WritesFIFO)}.txt";
            try
            {
                ConcurrentQueue<object> queue = new ConcurrentQueue<object>();
                foreach (var item in data)
                {
                    queue.Enqueue(item);
                }
                using (var writer = Factory.CreateWriter<object>(path))
                {
                    writer.Collection = queue;

                    Task.WaitAll(writer.WriteAsync());
                }
                string actual;
                using (var reader = File.OpenText(path))
                {
                    actual = reader.ReadToEnd();
                }

                Assert.Equal(expected, actual);
            }
            finally
            {
                File.Delete(path);
            }
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
