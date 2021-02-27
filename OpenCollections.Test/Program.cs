using System;
using OpenCollections;

var producer = Factory.CreateProducer(Factory.CreateReader("Primes.txt"));

var consumer = Factory.CreateConsumer<string, int>(producer);

int itemsConsumed = 0;

consumer.Operation = x =>
{
    System.Threading.Interlocked.Increment(ref itemsConsumed);
    Console.WriteLine($"Items Consumed: {itemsConsumed}");
    return int.Parse(x);
};

producer.Started += () => Log("Producer Started");
producer.CollectionChanged += () => Log("Producer Collection Changed");
producer.Finished += () => Log("Producer Finished");

consumer.Started += () => Log("Consumer Started");
consumer.CollectionChanged += () => Log("Consumer Collection Changed");
consumer.Finished += () => Log("Consumer Finished");

Console.WriteLine("Starting Producing");
producer.ProduceAsync();
Console.WriteLine("Ended Producing");

Console.Read();

void Log(string message)
{
    Console.WriteLine(message);
}