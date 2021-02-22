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

            (bool actual, string result) = Helpers.VerifyCollection(expected, consumer.ResultCollection.ToArray());

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

            (bool actual, string result) = Helpers.VerifyCollection(expected, consumer.ResultCollection.ToArray());

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
            FaultyConcurrentQueue<int> output = new FaultyConcurrentQueue<int>();

            output.RandomlyFail = true;

            var bag = CreateTestBag();

            var consumer = Factory.CreateConsumer(bag, output);

            consumer.Operation = (x) => x;

            consumer.Consume();

            Assert.True(consumer.ResultCollection.Count > 0);
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
