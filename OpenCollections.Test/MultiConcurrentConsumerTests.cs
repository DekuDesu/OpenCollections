using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using OpenCollections;
using System.Collections.Concurrent;
using System.Threading;

namespace OpenCollections.Test
{
    public class MultiConcurrentConsumerTests
    {
        Random Generator = new Random();
        [Fact]
        public void AllItemsPresent()
        {
            var input = CreateMultipleBags();

            List<int[]> expected = input.expected;

            Func<int, int> op = (x) => x + 1;

            var consumer = new MultiConcurrentConsumer<int, int>()
            {
                Collections = input.bags,
                ResultCollection = new ConcurrentQueue<int>(),
                Operation = op
            };

            consumer.Consume();

            List<int> expectedNumbers = new List<int>();
            foreach (var item in expected)
            {
                foreach (var item1 in item)
                {
                    expectedNumbers.Add(op(item1));
                }
            }

            var actual = Helpers.VerifyAllValuesPresent(consumer.ResultCollection, expectedNumbers);

            _ = "";

            Assert.True(actual.result, actual.message);
        }

        [Fact]
        public void AllItemsPresentAsync()
        {
            var input = CreateMultipleBags();

            List<int[]> expected = input.expected;

            Func<int, int> op = (x) => x + 1;

            var consumer = new MultiConcurrentConsumer<int, int>()
            {
                Collections = input.bags,
                ResultCollection = new ConcurrentQueue<int>(),
                Operation = op
            };

            Task.WaitAll(consumer.ConsumeAsync());

            List<int> expectedNumbers = new List<int>();
            foreach (var item in expected)
            {
                foreach (var item1 in item)
                {
                    expectedNumbers.Add(op(item1));
                }
            }

            var actual = Helpers.VerifyAllValuesPresent(consumer.ResultCollection, expectedNumbers);

            _ = "";

            Assert.True(actual.result, actual.message);
        }


        [Fact]
        public void CancelThrowsWhenAttemptingToCancelWhenCancelTokenIsManagedByOtherObject()
        {
            var input = CreateMultipleBags();

            List<int[]> expected = input.expected;

            var consumer = new MultiConcurrentConsumer<int, int>()
            {
                Collections = input.bags,
                ResultCollection = new ConcurrentQueue<int>()
            };

            CancellationTokenSource TokenSource = new CancellationTokenSource();

            Task consumerTask = consumer.ConsumeAsync(TokenSource.Token);

            TaskStatus status = consumerTask.Status;

            Assert.Throws<NotSupportedException>(() => consumer.Cancel());
        }

        [Fact]
        public void ThrowsWhenNoOperationProvided()
        {
            var input = CreateMultipleBags();

            List<int[]> expected = input.expected;

            var consumer = new MultiConcurrentConsumer<int, int>()
            {
                Collections = input.bags,
                ResultCollection = new ConcurrentQueue<int>(),
            };

            Assert.Throws<NotImplementedException>(() => consumer.Consume());
        }

        private (List<IProducerConsumerCollection<int>> bags, List<int[]> expected) CreateMultipleBags(int min = 2, int max = 10)
        {
            List<IProducerConsumerCollection<int>> bags = new List<IProducerConsumerCollection<int>>();
            List<int[]> expectedNumbers = new List<int[]>();
            int numOfBags = Generator.Next(min, max + 1);
            for (int i = 0; i < numOfBags; i++)
            {
                var bag = CreateTestBag();
                bags.Add(bag);
                expectedNumbers.Add(bag.ToArray());
            }
            return (bags, expected: expectedNumbers);
        }

        [Fact]
        private void DoesntThrowWhenCancelAndNoManagedToken()
        {
            var multi = Factory.CreateMultiConsumer<int, int>();
            multi.InvokeAsync(default);
            void ThrowsIfSuccess()
            {
                multi.Cancel();
                throw new Exception();
            }
            Assert.Throws<Exception>(ThrowsIfSuccess);
        }

        [Fact]
        private void CantConsumeWhileAlreadyConsuming()
        {
            FaultyConcurrentQueue<int> input = new FaultyConcurrentQueue<int>();
            FaultyConcurrentQueue<int> queue = new FaultyConcurrentQueue<int>();

            input.TryAdd(1);
            input.TryAdd(2);
            input.TryAdd(3);

            queue.AllowAdding = false;

            var producer = Factory.CreateProducer(input);

            var multiConsumer = Factory.CreateMultiConsumer<int, int?>(producer);

            multiConsumer.Operation = (x) => x;

            Task.Run(() => { Thread.Sleep(20); queue.AllowAdding = true; });

            Task.Run(multiConsumer.Consume);
            Task.Run(multiConsumer.Consume);

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
