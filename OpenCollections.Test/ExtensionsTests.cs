using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OpenCollections.Tests
{
    public class ExtensionsTests
    {
        [Fact]
        public void OutputToSetsCorrectly()
        {
            var writer = Factory.CreateWriter<int>("");
            var consumer = Factory.CreateConsumer<int, int>(ObjectToOutputTo: writer);
            Assert.True(writer.Collection == consumer.ResultCollection);
        }

        [Fact]
        public void InputFromWorksCorrectly()
        {
            var consumer = Factory.CreateConsumer<int, int>();

            var consumer1 = Factory.CreateConsumer<int, int>(ObjectToConsumeFrom: consumer);

            Assert.True(consumer1.Collection == consumer.ResultCollection);
        }

        [Fact]
        public void InputFromMultiConsumerWorks()
        {
            var producer = Factory.CreateProducer<int?>(null);
            var consumer = Factory.CreateMultiConsumer<int?, int>();
            consumer.InputFrom(producer);

            Assert.True(consumer.Collections.Contains(producer.ResultCollection));

            consumer = Factory.CreateMultiConsumer<int?, int>(producer);

            Assert.True(consumer.Collections.Contains(producer.ResultCollection));

            var alternateProducer = Factory.CreateProducer<int?>(null);

            consumer = Factory.CreateMultiConsumer<int?, int>(producer, alternateProducer);

            Assert.True(consumer.Collections.Contains(producer.ResultCollection) && consumer.Collections.Contains(alternateProducer.ResultCollection));
        }

        [Fact]
        public void StopObservingWorks()
        {
            int[] numbers = { 0, 1, 2 };

            var producer = Factory.CreateProducer(numbers);

            var consumer = Factory.CreateConsumer<int, int>(producer);

            consumer.StopObserving(producer);

            producer.Produce();

            Assert.True(producer.ResultCollection.Count == numbers.Length);
        }

        [Fact]
        public void OberserveAsyncWorks()
        {
            var producer = Factory.CreateProducer(new int[] { 1 });

            var consumer = Factory.CreateConsumer<int, int>();
            consumer.Operation += x => x;

            consumer.InputFrom(producer);
            consumer.ObserveCollectionAsync(producer);

            Action expected = () => consumer.ConsumeAsync();

            bool started = false;

            consumer.Started += () => started = true;

            producer.Produce();

            void wait()
            {
                Thread.Sleep(10);
            }

            Task.WaitAll(Task.Run(wait));

            Assert.True(started);

            consumer.Cancel();
        }

        [Fact]
        public void UnObservceAsyncWorks()
        {
            var producer = Factory.CreateProducer(new int[] { 1 });

            var consumer = Factory.CreateConsumer<int, int>();
            consumer.Operation += x => x;

            consumer.InputFrom(producer);

            consumer.ObserveCollectionAsync(producer);

            consumer.StopObservingAsync(producer);

            bool consumerStarted = false;

            consumer.Started += () => { consumerStarted = true; };

            producer.Produce();

            try
            {
                Assert.False(consumerStarted);
            }
            finally
            {
                consumer.Cancel();
            }
        }
    }
}
