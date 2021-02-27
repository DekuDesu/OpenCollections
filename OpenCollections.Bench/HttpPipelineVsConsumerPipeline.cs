using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace OpenCollections.Bench
{
    public class HttpPipelineVsConsumerPipeline
    {
        private static Random Generator = new Random();

        int[] testData = { 0, 1, 2, 3 };
        string[] retrievedData = new string[4];

        public HttpPipelineVsConsumerPipeline()
        {
            retrievedData[0] = GenerateDataSet(10000);
            retrievedData[1] = GenerateDataSet(1000);
            retrievedData[2] = GenerateDataSet(5000);
            retrievedData[3] = GenerateDataSet(10000);
        }

        private string GenerateRequest(int num)
        {
            return @$"{num}";
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public async Task<int> Control()
        {
            DateTime lastQuery = DateTime.MinValue;
            int cooldown = 100;
            int largestNumber = int.MinValue;
            foreach (var item in testData)
            {
                int difference = DateTime.UtcNow.Millisecond - lastQuery.Millisecond;
                if (difference < cooldown)
                {
                    Thread.Sleep(cooldown - difference);
                }
                string result = await GetStringAsync(item);

                lastQuery = DateTime.UtcNow;

                string[] lines = result.Split('\n');

                int[] numbers = lines.Where(x => x.Length > 1).Select(x => int.Parse(x)).ToArray();

                foreach (var item1 in numbers)
                {
                    if (item1 > largestNumber)
                    {
                        largestNumber = item1;
                    }
                }
            }

            Console.WriteLine("//////////////////////////// = " + largestNumber);

            return largestNumber;
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public async Task<int> Test()
        {
            int largestNumber = int.MinValue;

            var producer = Factory.CreateProducer(testData);

            var webReader = new AsyncConsumer<int, int>()
            {
                OperationCooldown = 100,
                AsyncOperation = async (x) =>
                {

                    string result = await GetStringAsync(x);

                    string[] lines = result.Split('\n');

                    int[] numbers = lines.Where(x => x.Length > 1).Select(x => int.Parse(x)).ToArray();

                    foreach (var item1 in numbers)
                    {
                        if (item1 > largestNumber)
                        {
                            largestNumber = item1;
                        }
                    }

                    return largestNumber;
                }
            };

            webReader.InputFrom(producer);
            webReader.ObserveCollection(producer);

            await Task.WhenAll(producer.ProduceAsync(), webReader.ConsumeAsync());

            Console.WriteLine("//////////////////////////// = " + largestNumber);

            return largestNumber;
        }

        private async Task<string> GetStringAsync(int Request)
        {
            await Task.Run(() => Thread.Sleep(100));

            return retrievedData[Request];
        }

        string GenerateDataSet(int Request)
        {
            string result = string.Empty;
            for (int i = 0; i < Request; i++)
            {
                result += $"{Generator.Next(-100000, 100000)}{'\n'}";
            }

            return result;
        }
    }
}
