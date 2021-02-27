using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;

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

        public static IEnumerableReader<string> CreateReader(in string FilePath)
        {
            return new EnumerableStreamReader(FilePath);
        }

        // /////////////////////// PRODUCERS
        public static IConcurrentProducer<T> CreateProducer<T>(IEnumerable<T> EnumerableObject)
        {
            return new ConcurrentProducer<T>(EnumerableObject);
        }

        public static IConcurrentProducer<T> CreateProducer<T>(IEnumerable<T> EnumerableObject, IConcurrentInput<T> ObjectToOutputTo)
        {
            var producer = new ConcurrentProducer<T>(EnumerableObject);
            producer.OutputTo(ObjectToOutputTo);
            return producer;
        }

        public static IConcurrentProducer<T> CreateProducer<T>(IEnumerable<T> EnumerableObject, IProducerConsumerCollection<T> CollectionToOutPutTo)
        {
            return new ConcurrentProducer<T>(EnumerableObject)
            {
                ResultCollection = CollectionToOutPutTo
            };
        }

        // /////////////////////// CONSUMER
        public static IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>(IConcurrentOutput<T> ObjectToConsumeFrom)
        {
            var consumer = new ConcurrentConsumer<T, TResult>();
            consumer.InputFrom(ObjectToConsumeFrom);
            consumer.ObserveCollection<T>(ObjectToConsumeFrom);
            return consumer;
        }

        public static IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>(IConcurrentOutput<T> ObjectToConsumeFrom = default, IConcurrentInput<TResult> ObjectToOutputTo = default)
        {
            var consumer = new ConcurrentConsumer<T, TResult>();
            if (ObjectToConsumeFrom != default)
            {
                consumer.InputFrom(ObjectToConsumeFrom);
                consumer.ObserveCollection(ObjectToConsumeFrom);
            }
            if (ObjectToOutputTo != default)
            {
                consumer.OutputTo(ObjectToOutputTo);
            }
            return consumer;
        }

        public static IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>()
        {
            return new ConcurrentConsumer<T, TResult>();
        }

        public static IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>(IProducerConsumerCollection<T> InputCollection, IProducerConsumerCollection<TResult> OutputCollection)
        {
            return new ConcurrentConsumer<T, TResult>
            {
                Collection = InputCollection,
                ResultCollection = OutputCollection
            };
        }

        public static IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>(IProducerConsumerCollection<TResult> OutputCollection)
        {
            return new ConcurrentConsumer<T, TResult>
            {
                ResultCollection = OutputCollection
            };
        }

        public static IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>(IProducerConsumerCollection<T> InputCollection)
        {
            return new ConcurrentConsumer<T, TResult>
            {
                Collection = InputCollection
            };
        }

        public static IConcurrentMultiConsumer<T, TResult> CreateMultiConsumer<T, TResult>()
        {
            return new MultiConcurrentConsumer<T, TResult>();
        }

        public static IConcurrentMultiConsumer<T, TResult> CreateMultiConsumer<T, TResult>(params IConcurrentOutput<T>[] ObjectsToConsumeFrom)
        {
            if (ObjectsToConsumeFrom.Length == 0)
            {
                return CreateMultiConsumer<T, TResult>();
            }
            if (ObjectsToConsumeFrom.Length > 1)
            {
                List<IProducerConsumerCollection<T>> collections = new List<IProducerConsumerCollection<T>>();
                var multiConsumer = new MultiConcurrentConsumer<T, TResult>()
                {
                    Collections = collections
                };
                foreach (var item in ObjectsToConsumeFrom)
                {
                    collections.Add(item.ResultCollection);
                }
                return multiConsumer;
            }
            else
            {
                var multiConsumer = new MultiConcurrentConsumer<T, TResult>()
                {
                    Collections = { ObjectsToConsumeFrom[0].ResultCollection },
                };
                return multiConsumer;
            }

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
