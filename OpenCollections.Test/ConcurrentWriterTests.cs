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

namespace OpenCollections.Test
{
    public class ConcurrentWriterTests
    {
        Random Generator = new Random();

        string PathBase = $"Exmaple{nameof(ConcurrentWriterTests)}";

        string Path => $"{PathBase}.txt";

        [Fact]
        public void InvokesAllEvents()
        {
            string path = $"{PathBase}.{nameof(InvokesAllEvents)}.txt";
            try
            {
                bool started = false;

                bool collectionChanged = false;

                bool finished = false;

                ConcurrentBag<int> ints = new ConcurrentBag<int>() { 1, 2, 3, 4, 5 };

                using (var writer = Factory.CreateWriter(path, ints))
                {
                    writer.Started += () => { started = true; };

                    writer.CollectionChanged += () => { collectionChanged = true; };

                    writer.Finished += () => { finished = true; };

                    writer.Write();
                }

                Assert.True(started);

                Assert.True(collectionChanged);

                Assert.True(finished);

                started = false;

                collectionChanged = false;

                finished = false;

                ints = new ConcurrentBag<int>() { 1, 2, 3, 4, 5 };

                using (var writer = Factory.CreateWriter(path, ints))
                {
                    writer.Started += () => { started = true; };

                    writer.CollectionChanged += () => { collectionChanged = true; };

                    writer.Finished += () => { finished = true; };

                    writer.WriteLines();
                }

                Assert.True(started);

                Assert.True(collectionChanged);

                Assert.True(finished);
            }
            finally
            {

            }
        }

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

                string actual = ReadFile(path);

                Assert.Equal(expected, actual);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("null", "null")]
        [InlineData("1", "1")]
        [InlineData("Hello World", "Hello World")]
        [InlineData("_", "_")]
        [InlineData(" ", " ")]
        [InlineData("®", "®")]
        [InlineData("0_0_0", "0_0_0")]
        [InlineData(@"!@#$%^&*()_+", @"!@#$%^&*()_+")]
        [InlineData(@"\", @"\")]
        public void WriteLineDoesntTranspose(string data, string expected)
        {
            string path = $"{PathBase}.{nameof(WriteLineDoesntTranspose)}.txt";

            // we shouldnt be able to write literal nulls to a file, "null" should be written though as its a string
            try
            {

                // note: custom extension in this namespace that allows overriding "Add" for collection init to this collection
                // see OpenCollections.Tests.Helpers
                ConcurrentBag<string> bag = new ConcurrentBag<string>() {
                    data
                };

                using (var writer = Factory.CreateWriter(path, bag))
                {
                    writer.WriteLines();
                }

                string actual = ReadFileLines(path)?[0];

                Assert.True(actual is string);

                Assert.Equal(expected, actual);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Theory]
        [InlineData(new string[] { "1", "2", "3" }, "")]
        [InlineData(new string[] { "null", "null", "null" }, "")]
        public void WritesLinesFIFOAsync(string[] data, string dummy)
        {
            // this is a workaround to use non-constants in the attribute tag
            _ = dummy;

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

                    Task.WaitAll(writer.WriteLinesAsync(true));
                }

                List<string> lines = ReadFileLines(path);

                var result = Helpers.VerifyCollection(data, lines);

                Assert.True(result.result, result.message);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void WritesCorrectlyEvenWithFaultyCollection()
        {
            string path = $"{PathBase}.{nameof(WritesLinesCorrectlyEvenWithFaultyCollection)}.txt";


            CancellationTokenSource tokenSource = new CancellationTokenSource();

            try
            {
                FaultyConcurrentQueue<string> queue = new FaultyConcurrentQueue<string>
                {
                    RandomlyFail = false
                };

                string expected = "";

                for (int i = 100; i-- > 0;)
                {
                    string num = i.ToString();

                    expected += num;

                    queue.TryAdd(num);
                }

                queue.RandomlyFail = true;

                using (var writer = Factory.CreateWriter(path, queue))
                {
                    Task.WaitAll(writer.WriteAsync(true, tokenSource.Token));
                }

                string actual = ReadFile(path);

                Assert.Equal(expected, actual);
            }
            finally
            {
                tokenSource?.Dispose();
                File.Delete(path);
            }
        }

        [Fact]
        public void WritesLinesCorrectlyEvenWithFaultyCollection()
        {
            string path = $"{PathBase}.{nameof(WritesLinesCorrectlyEvenWithFaultyCollection)}.txt";


            CancellationTokenSource tokenSource = new CancellationTokenSource();

            try
            {
                FaultyConcurrentQueue<string> queue = new FaultyConcurrentQueue<string>
                {
                    RandomlyFail = false
                };

                List<string> expected = new List<string>();

                for (int i = 100; i-- > 0;)
                {
                    string num = i.ToString();

                    expected.Add(num);

                    queue.TryAdd(num);
                }

                queue.RandomlyFail = true;

                using (var writer = Factory.CreateWriter(path, queue))
                {
                    Task.WaitAll(writer.WriteLinesAsync(true, tokenSource.Token));
                }

                List<string> actual = ReadFileLines(path);

                var result = Helpers.VerifyCollection(expected, actual);

                Assert.True(result.result, result.message);
            }
            finally
            {
                tokenSource?.Dispose();
                File.Delete(path);
            }
        }

        [Fact]
        public void WriteLinesOverloadsHaveExpectedBehaviour()
        {
            string path = $"{PathBase}.{nameof(WriteLinesOverloadsHaveExpectedBehaviour)}";

            CancellationTokenSource TokenSource = new CancellationTokenSource();

            try
            {
                ConcurrentQueue<string> NewQueue()
                {
                    // see OpenCollections.Tests.Helpers for the extenstion that this used to initialize the collection
                    return new ConcurrentQueue<string>()
                    {
                        "1",
                        "2",
                        "3",
                        "4",
                        "5"
                    };
                }

                string[] expected = NewQueue().ToArray();

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    Task.WaitAll(writer.WriteLinesAsync());
                }

                var result = Helpers.VerifyCollection(expected, ReadFileLines(path));

                Assert.True(result.result, result.message);

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    Task.WaitAll(writer.WriteLinesAsync(true));
                }

                result = Helpers.VerifyCollection(expected, ReadFileLines(path));

                Assert.True(result.result, result.message);

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    Task.WaitAll(writer.WriteLinesAsync(true, TokenSource.Token));
                }

                result = Helpers.VerifyCollection(expected, ReadFileLines(path));

                Assert.True(result.result, result.message);

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    Task.WaitAll(writer.WriteLinesAsync(TokenSource.Token));
                }

                result = Helpers.VerifyCollection(expected, ReadFileLines(path));

                Assert.True(result.result, result.message);

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    writer.WriteLines();
                }

                result = Helpers.VerifyCollection(expected, ReadFileLines(path));

                Assert.True(result.result, result.message);

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    writer.Invoke();
                }

                result = Helpers.VerifyCollection(expected, ReadFileLines(path));

                Assert.True(result.result, result.message);

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    Task.WaitAll(writer.InvokeAsync(default));
                }

                result = Helpers.VerifyCollection(expected, ReadFileLines(path));

                Assert.True(result.result, result.message);
            }
            finally
            {
                TokenSource?.Cancel();
                TokenSource?.Dispose();
                File.Delete(path);
            }
        }

        [Fact]
        public void WriteOverloadsHaveExpectedBehaviour()
        {
            string path = $"{PathBase}.{nameof(WriteOverloadsHaveExpectedBehaviour)}";

            CancellationTokenSource TokenSource = new CancellationTokenSource();

            try
            {
                ConcurrentQueue<string> NewQueue()
                {
                    // see OpenCollections.Tests.Helpers for the extenstion that this used to initialize the collection
                    return new ConcurrentQueue<string>()
                    {
                        "1",
                        "2",
                        "3",
                        "4",
                        "5"
                    };
                }

                string expected = "12345";

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    Task.WaitAll(writer.WriteAsync());
                }

                Assert.Equal(expected, ReadFile(path));

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    Task.WaitAll(writer.WriteAsync(true));
                }

                Assert.Equal(expected, ReadFile(path));

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    Task.WaitAll(writer.WriteAsync(true, TokenSource.Token));
                }

                Assert.Equal(expected, ReadFile(path));

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    Task.WaitAll(writer.WriteAsync(TokenSource.Token));
                }

                Assert.Equal(expected, ReadFile(path));

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    writer.Write();
                }

                Assert.Equal(expected, ReadFile(path));

                using (var writer = Factory.CreateWriter(path, NewQueue()))
                {
                    writer.Write(true);
                }

                Assert.Equal(expected, ReadFile(path));
            }
            finally
            {
                TokenSource?.Cancel();
                TokenSource?.Dispose();
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

            writer.Dispose();
        }

        private List<string> ReadFileLines(string path)
        {
            List<string> lines = new List<string>();

            using (var reader = File.OpenText(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }

        private string ReadFile(string path)
        {
            string result;

            using (var reader = File.OpenText(path))
            {
                result = reader.ReadToEnd();
            }

            return result;
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
