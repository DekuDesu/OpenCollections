using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using OpenCollections;
using System.Threading;
using System.IO;

namespace OpenCollections.Tests
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

            (bool actual, string result) = ConcurrentTestHelpers.VerifyCollection(expected, producer.ResultCollection);

            _ = result;

            Assert.True(actual, result);
        }
        [Fact]
        public void FIFOAsync()
        {
            string[] expected = CreateTestData();

            var producer = new ConcurrentProducer<string>(TestEnumerable(expected));

            producer.ProduceAsync().Wait();

            (bool actual, string result) = ConcurrentTestHelpers.VerifyCollection(expected, producer.ResultCollection);

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
