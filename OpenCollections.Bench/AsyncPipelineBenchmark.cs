using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet;
using Newtonsoft.Json;
using OpenCollections;

namespace OpenCollections.Benchmark
{
    public class AsyncPipelineBenchmark
    {
        private readonly Random Generator = new Random();
        private readonly List<int> TestData;

        public AsyncPipelineBenchmark()
        {
            TestData = CreateTestData();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public async Task<bool[]> Consumer()
        {
            var producer = Factory.CreateProducer(TestData);

            var consumer = Factory.CreateConsumer<int, bool>(producer);

            consumer.Operation = (x) => IsPrime(x);

            await producer.ProduceAsync();

            return consumer.ResultCollection.ToArray();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public async Task<bool[]> Control()
        {
            List<bool> primes = new List<bool>();
            foreach (var item in TestData)
            {
                primes.Add(await Task.Run(() => IsPrime(item)));
            }
            return primes.ToArray();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public async Task<bool[]> ControlAlternate()
        {
            return await Task.Run(() =>
            {
                List<bool> primes = new List<bool>();
                foreach (var item in TestData)
                {
                    primes.Add(IsPrime(item));
                }
                return primes.ToArray();
            });
        }

        // this is innefficient so as to take a signifigant time to complete a single opertaion to test async deviance in the test, ignore how inifficient this is, i would normally create a prime(eratosthenis or whatever his name is) seive to determine all primes in an arbitrary number that can be as large as int.max
        private bool IsPrime(int number)
        {
            if (number % 2 == 0)
            {
                if (number == 2)
                {
                    return true;
                }
                return false;
            }
            for (int i = number - 2; i > 3; i -= 2)
            {
                if (number % i == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public List<int> CreateTestData(int amount = 100000)
        {
            List<int> data = new List<int>();
            for (int i = 0; i < amount; i++)
            {
                data.Add(Generator.Next(0, 50));
            }
            return data;
        }
    }
}
