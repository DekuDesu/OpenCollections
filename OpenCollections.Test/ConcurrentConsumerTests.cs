using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using OpenCollections;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using System.Collections;

namespace OpenCollections.Tests
{
    public class ConcurrentConsumerTests
    {
        Random Generator = new Random();
        [Fact]
        public void FIFO()
        {
            var input = CreateTestBag();

            int[] expected = input.ToArray();

            var consumer = new ConcurrentConsumer<int, int>()
            {
                Collection = input,
                ResultCollection = new ConcurrentQueue<int>(),
                Operation = (x) => x
            };

            consumer.Consume();

            (bool actual, string result) = ConcurrentTestHelpers.VerifyCollection(expected, consumer.ResultCollection.ToArray());

            Assert.True(actual, result);
        }

        [Fact]
        public void FIFOAsync()
        {
            var input = CreateTestBag();

            int[] expected = input.ToArray();

            var consumer = new ConcurrentConsumer<int, int>()
            {
                Collection = input,
                ResultCollection = new ConcurrentQueue<int>(),
                Operation = (x) => x
            };

            consumer.ConsumeAsync().Wait();

            (bool actual, string result) = ConcurrentTestHelpers.VerifyCollection(expected, consumer.ResultCollection.ToArray());

            Assert.True(actual, result);
        }

        [Fact]
        public void ThrowsWhenNoOperationProvided()
        {
            var input = CreateTestBag();

            int[] expected = input.ToArray();

            var consumer = new ConcurrentConsumer<int, int>()
            {
                Collection = input,
                ResultCollection = new ConcurrentQueue<int>(),
            };

            Assert.Throws<NotImplementedException>(() => consumer.Consume());
        }

        [Fact]
        public void ThrowsWhenCollectionEmpty()
        {
            var consumer = Factory.CreateConsumer<int, string>();
            consumer.Operation = null;

            Assert.Throws<ArgumentNullException>(() => consumer.Consume());
        }

        [Fact]
        public void BufferWorks()
        {
            BrokenConcurrentQueue<int> input = new BrokenConcurrentQueue<int>();
            BrokenConcurrentQueue<int> output = new BrokenConcurrentQueue<int>();

            input.AllowAdding = true;

            input.TryAdd(1);
            input.TryAdd(2);
            input.TryAdd(3);
            input.TryAdd(4);

            input.AllowRemoving = true;

            output.AllowAdding = false;

            var consumer = Factory.CreateConsumer(input, output);

            consumer.Operation = (x) => x;

            void PreventAddingForTime(int time)
            {
                Thread.Sleep(time);
                output.AllowAdding = true;
            }

            Task.Run(() =>
            {
                PreventAddingForTime(20);
            });

            consumer.Consume();

            Assert.True(input.Count == 0);
            Assert.True(output.Count == 4);
        }

        [Fact]
        public void CancelThrowsWhenAttemptingToCancelWhenCancelTokenIsManagedByOtherObject()
        {
            var input = CreateTestBag(200000);

            int[] expected = input.ToArray();

            var consumer = new ConcurrentConsumer<int, int>()
            {
                Collection = input,
                ResultCollection = new ConcurrentQueue<int>()
            };

            CancellationTokenSource TokenSource = new CancellationTokenSource();

            Task consumerTask = consumer.ConsumeAsync(TokenSource.Token);

            TaskStatus status = consumerTask.Status;

            Assert.Throws<NotSupportedException>(() => consumer.Cancel());
        }

        [Fact]
        public void CancelDoesNotThrowWhenNoManagedToken()
        {
            var input = CreateTestBag(200000);

            var consumer = new ConcurrentConsumer<int, int>()
            {
                Collection = input,
                ResultCollection = new ConcurrentQueue<int>()
            };

            _ = consumer.ConsumeAsync();

            int? VerifyMethodWorks()
            {
                consumer.Cancel();

                // this only reaches here if no error is thrown in the above functions
                return null;
            }

            Assert.Null(VerifyMethodWorks());
        }


        private ConcurrentQueue<int> CreateTestBag(int maxNumbers = 1000)
        {
            var newBag = new ConcurrentQueue<int>();
            for (int i = 0; i < maxNumbers; i++)
            {
                newBag.Enqueue(Generator.Next(-maxNumbers, maxNumbers));
            }
            return newBag;
        }

    }
}
