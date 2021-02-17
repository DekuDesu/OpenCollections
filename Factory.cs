using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace OpenCollections
{
    public static class Factory
    {
        // ///////////////////////// WRITERS
        public static IConcurrentWriter<T> CreateWriter<T>(string Path)
        {
            return new ConcurrentWriter<T>(Path);

        }

        public static IConcurrentWriter<T> CreateWriter<T>(string Path, IProducerConsumerCollection<T> Collection)
        {
            return new ConcurrentWriter<T>(Path)
            {
                Collection = Collection
            };
        }

        public static IConcurrentWriter<T> CreateWriter<T>(string Path, IConcurrentOutput<T> Producer)
        {
            var writer = new ConcurrentWriter<T>(Path);
            writer.InputFrom(Producer);
            writer.ObserveCollection(Producer);
            return writer;
        }

        // //////////////////////// READERS

        public static IEnumerableReader<string> CreateMultiReader(params string[] FilePaths)
        {
            return new EnumerableMultiReader(FilePaths);
        }

        public static IEnumerableReader<string> CreateReader(string FilePath)
        {
            return new EnumerableStreamReader(FilePath);
        }

        // /////////////////////// PRODUCERS
        public static IConcurrentProducer<T> CreateProducer<T>(IEnumerable<T> EnumerableObject)
        {
            return new ConcurrentProducer<T>(EnumerableObject);
        }

        // /////////////////////// CONSUMER
        public static IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>(IConcurrentOutput<T> ObjectToConsumeFrom)
        {
            var consumer = new ConcurrentConsumer<T, TResult>();
            consumer.InputFrom(ObjectToConsumeFrom);
            consumer.ObserveCollection(ObjectToConsumeFrom);
            return consumer;
        }

        public static IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>(IConcurrentOutput<T> ObjectToConsumeFrom, IConcurrentInput<TResult> ObjectToOutputTo)
        {
            var consumer = new ConcurrentConsumer<T, TResult>();
            consumer.InputFrom(ObjectToConsumeFrom);
            consumer.OutputTo(ObjectToOutputTo);
            consumer.ObserveCollection(ObjectToConsumeFrom);
            return consumer;
        }

        public static IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>()
        {
            return new ConcurrentConsumer<T, TResult>();
        }

        public static IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>(IProducerConsumerCollection<T> InputCollection, IProducerConsumerCollection<TResult> OutputCollection)
        {
            return new ConcurrentConsumer<T, TResult>()
            {
                Collection = InputCollection,
                ResultCollection = OutputCollection
            };
        }

        public static IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>(IProducerConsumerCollection<TResult> OutputCollection)
        {
            return new ConcurrentConsumer<T, TResult>()
            {
                ResultCollection = OutputCollection
            };
        }

        public static IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>(IProducerConsumerCollection<T> InputCollection)
        {
            return new ConcurrentConsumer<T, TResult>()
            {
                Collection = InputCollection
            };
        }

        public static class Messages
        {
            public static string ManagedTokenError()
            {
                return $"Cancelling this Task when it's CancellationToken is being managed by a different CancellationTokenSource is not supported. Call [TokenSource].Cancel() on the object managing the token provided to this object instead.";
            }
        }
    }
}
