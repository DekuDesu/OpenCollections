using System;
using System.Diagnostics;
using OpenCollections;

// test 1: Finished Test: 108,341ms
// test 2: Finished Test: 210,358ms
// test 3: Finished Test:  90,182ms

var producer = Factory.CreateProducer(Factory.CreateReader("Primes.txt"));

var consumer = Factory.CreateConsumer<string, int>();

consumer.InputFrom(producer);

consumer.ObserveCollection(producer);

int itemsConsumed = 0;

Stopwatch watch = new();

consumer.Operation = x =>
{
    System.Threading.Interlocked.Increment(ref itemsConsumed);
    Console.WriteLine($"Items Consumed: {itemsConsumed}");
    if (itemsConsumed >= 1_000_000)
    {
        watch.Stop();
        Console.WriteLine($"-------------------------------Finished Test: {watch.ElapsedMilliseconds}ms");
    }
    return int.Parse(x);
};

producer.Started += (x, y) => Log("Producer Started");
producer.CollectionChanged += (x, y) => Log("Producer Collection Changed");
producer.Finished += (x, y) => Log("Producer Finished");

consumer.Started += (x, y) => Log("Consumer Started");
consumer.CollectionChanged += (x, y) => Log("Consumer Collection Changed");
consumer.Finished += (x, y) => Log("Consumer Finished");

Console.WriteLine("Starting Producing");
watch.Start();
producer.ProduceAsync();
Console.WriteLine("Ended Producing");

Console.Read();

void Log(string message)
{
    Console.WriteLine(message);
}