using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using OpenCollections;
using System.Threading;
using System.IO;

namespace OpenCollections.Test
{
    public class ConcurrentProducerTsts
    {
        Random Generator = new Random();
        [Fact]
        public void FIFO()
        {
            string[] expected = CreateTestData();

            var producer = new ConcurrentProducer<string>(TestEnumerable(expected));

            producer.Produce();

            (bool actual, string result) = Helpers.VerifyCollection(expected, producer.ResultCollection);

            _ = result;

            Assert.True(actual, result);
        }
        [Fact]
        public void FIFOAsync()
        {
            string[] expected = CreateTestData();

            var producer = new ConcurrentProducer<string>(TestEnumerable(expected));

            producer.ProduceAsync().Wait();

            (bool actual, string result) = Helpers.VerifyCollection(expected, producer.ResultCollection);

            _ = result;

            Assert.True(actual, result);
        }

        [Fact]
        public void CancelThrowsWhenCancellationTokenIsManagedToken()
        {
            string[] expected = CreateTestData();

            var producer = new ConcurrentProducer<string>(TestEnumerable(expected));

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            _ = producer.ProduceAsync(tokenSource.Token);

            Assert.Throws<NotSupportedException>(() => producer.Cancel());
        }

        [Fact]
        public void GracefullyHandlesNullIteratorYield()
        {
            IEnumerable<int?> Test()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                yield return default;
            }

            var producer = Factory.CreateProducer(Test());

            producer.Produce();

            Assert.True(producer.ResultCollection.Count == 3);
        }

        [Fact]
        public void DisposeTraverses()
        {
            int[] numbers = { 1, 2, 3 };
            using (var producer = new ConcurrentProducer<int>(numbers))
            {
                producer.Produce();
                Assert.True(producer.ResultCollection.Count == 3);
            }
        }

        [Fact]
        private void DoesntThrowWhenCancelAndNoManagedToken()
        {
            string[] expected = CreateTestData();
            var multi = Factory.CreateProducer(expected);
            multi.ProduceAsync();
            void ThrowsIfSuccess()
            {
                multi.Cancel();
                throw new Exception();
            }
            Assert.Throws<Exception>(ThrowsIfSuccess);
        }

        [Fact]
        public void BufferWorks()
        {
            FaultyConcurrentQueue<string> brokenQueue = new FaultyConcurrentQueue<string>();

            brokenQueue.RandomlyFail = true;

            string[] data = CreateTestData(10000);

            var producer = Factory.CreateProducer(data, brokenQueue);

            producer.Produce();
        }

        private string[] CreateTestData(int number = 500)
        {
            string[] stringarray = new string[number];
            for (int i = 0; i < number; i++)
            {
                stringarray[i] = $"{Generator.Next()}";
            }
            return stringarray;
        }

        private IEnumerable<string> TestEnumerable(IEnumerable<string> enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return item;
            }
        }
    }
}
