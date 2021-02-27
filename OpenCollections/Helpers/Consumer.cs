using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCollections.Helpers
{
    /// <summary>
    /// Helper methods for the <see cref="OpenCollections"/> Concurrent <see cref="IConsumer{T, TResult}"/> and <see cref="IProducer"/> classes
    /// </summary>
    internal static class Consumer
    {
        /// <summary>
        /// Attempts to add the first item from <paramref name="Buffer"/> and add it to the <paramref name="ResultCollection"/>, and subsequently remove the first item from <paramref name="Buffer"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Buffer"></param>
        /// <param name="ResultCollection"></param>
        /// <returns>
        /// <see langword="true"/>: When item was removed from the <paramref name="Buffer"/> and successfully added to <paramref name="ResultCollection"/>
        /// <para>
        /// <see langword="false"/>: When the item was not sucessfully added. The buffer item is not removed when this method return false.
        /// </para>
        /// </returns>
        internal static bool TryAddBufferItem<T>(IList<T> Buffer, IProducerConsumerCollection<T> ResultCollection)
        {
            // try to add the first item in the buffer
            if (ResultCollection.TryAdd(Buffer[0]))
            {
                // since we were able to add the item remove it from the buffer and move on
                Buffer.RemoveAt(0);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to empty the <paramref name="Buffer"/> into the <paramref name="ResultCollection"/>, until either all items have been removed from the <paramref name="Buffer"/> or, the adding of the item to <paramref name="ResultCollection"/> fails.
        /// </summary>
        /// <param name="returnOnFail">
        /// Whether or not when attempting to empty the <paramref name="Buffer"/> this method returns when it fails to add any item to the <paramref name="ResultCollection"/> at any time.
        /// </param>
        internal static void TryEmptyBuffer<T>(IList<T> Buffer, IProducerConsumerCollection<T> ResultCollection, bool returnOnFail = true)
        {
            // attempt to add the items from the buffer, if it fails continue consuming items
            while (Buffer?.Count > 0)
            {
                if (TryAddBufferItem(Buffer, ResultCollection) == false && returnOnFail)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Loops over <paramref name="InCollection"/> and for each item attempts to consume the item and output the item into the <paramref name="OutCollection"/>, if the consumption fails the item is placed in <paramref name="BufferCollection"/>. This method will loop over <paramref name="BufferCollection"/> automatically and attempt to add the items to the <paramref name="OutCollection"/> at the next opportunity. Complies with data FIFO(First in First Out).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="InCollection"></param>
        /// <param name="OutCollection"></param>
        /// <param name="BufferCollection"></param>
        /// <param name="Operation"></param>
        internal static void ConsumeItems<T, TResult>(object caller, IProducerConsumerCollection<T> InCollection, IProducerConsumerCollection<TResult> OutCollection, IList<TResult> BufferCollection, Func<T, TResult> Operation, Action<object, CollectionEventArgs<TResult>> CollectionChanged, CancellationToken token)
        {
            // attempt to consume items until there are no more items to consume
            while (InCollection.Count > 0)
            {
                token.ThrowIfCancellationRequested();

                // If the buffer has items we should attempt to add them first to keep FIFO standard
                TryEmptyBuffer(BufferCollection, OutCollection);

                // Attempt to consume the items from the collection
                if (TryConsumeItem(InCollection, OutCollection, BufferCollection, Operation, out TResult item))
                {
                    CollectionChanged?.Invoke(
                        caller,
                        new CollectionEventArgs<TResult>
                        {
                            Token = token,
                            Item = item
                        }
                        );
                }
            }

            // One last check to make sure no items remain in the buffer
            TryEmptyBuffer(BufferCollection, OutCollection, returnOnFail: false);
        }

        /// <summary>
        /// Attempts to consume items from <paramref name="InCollection"/>, run <paramref name="Operation"/> on the item, and output the result to <paramref name="OutCollection"/>, if adding the item to <paramref name="OutCollection"/> fails the item is added to the <paramref name="OutCollection"/> instead.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="InCollection"></param>
        /// <param name="OutCollection"></param>
        /// <param name="OutCollection"></param>
        /// <param name="Operation"></param>
        /// <returns></returns>
        internal static bool TryConsumeItem<T, TResult>(IProducerConsumerCollection<T> InCollection, IProducerConsumerCollection<TResult> OutCollection, IList<TResult> BufferCollection, in Func<T, TResult> Operation, out TResult Item)
        {
            //attempt to take an available item
            if (InCollection.TryTake(out T item))
            {
                // run the operation on the item
                TResult result = Operation(item);

                // attempt to send the result to the output collection
                if (OutCollection.TryAdd(result) == false)
                {
                    BufferCollection.Add(result);
                    Item = result;
                    return false;
                }
                Item = default;
                return true;
            }
            Item = default;
            return false;
        }

        /// <summary>
        /// Loops over <paramref name="InCollection"/> and for each item attempts to consume the item and output the item into the <paramref name="OutCollection"/>, if the consumption fails the item is placed in <paramref name="BufferCollection"/>. This method will loop over <paramref name="BufferCollection"/> automatically and attempt to add the items to the <paramref name="OutCollection"/> at the next opportunity. Complies with data FIFO(First in First Out).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="InCollection"></param>
        /// <param name="OutCollection"></param>
        /// <param name="BufferCollection"></param>
        /// <param name="Operation"></param>
        internal static async Task ConsumeItemsAsync<T, TResult>(IProducerConsumerCollection<T> InCollection, IProducerConsumerCollection<TResult> OutCollection, IList<TResult> BufferCollection, Func<T, Task<TResult>> Operation, Action CollectionChanged, CancellationToken token, Func<CancellationToken, Task> CollectionChangedAsync, int RequestCooldown)
        {
            DateTime lastRequest = DateTime.MinValue;
            // attempt to consume items until there are no more items to consume
            while (InCollection.Count > 0)
            {
                token.ThrowIfCancellationRequested();

                // If the buffer has items we should attempt to add them first to keep FIFO standard
                TryEmptyBuffer(BufferCollection, OutCollection);

                int timeSinceLastRequest = DateTime.UtcNow.Millisecond - lastRequest.Millisecond;

                if (timeSinceLastRequest < RequestCooldown)
                {
                    Thread.Sleep(RequestCooldown - timeSinceLastRequest);
                }

                // Attempt to consume the items from the collection
                if (await TryConsumeItemAsync(InCollection, OutCollection, BufferCollection, Operation))
                {
                    CollectionChanged?.Invoke();
                    CollectionChangedAsync?.Invoke(token);
                }
            }

            // One last check to make sure no items remain in the buffer
            TryEmptyBuffer(BufferCollection, OutCollection, returnOnFail: false);
        }

        /// <summary>
        /// Attempts to consume items from <paramref name="InCollection"/>, run <paramref name="Operation"/> on the item, and output the result to <paramref name="OutCollection"/>, if adding the item to <paramref name="OutCollection"/> fails the item is added to the <paramref name="OutCollection"/> instead.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="InCollection"></param>
        /// <param name="OutCollection"></param>
        /// <param name="OutCollection"></param>
        /// <param name="Operation"></param>
        /// <returns></returns>
        internal static async Task<bool> TryConsumeItemAsync<T, TResult>(IProducerConsumerCollection<T> InCollection, IProducerConsumerCollection<TResult> OutCollection, IList<TResult> BufferCollection, Func<T, Task<TResult>> Operation)
        {
            //attempt to take an available item
            if (InCollection.TryTake(out T item))
            {
                // run the operation on the item
                TResult result = await Operation(item);

                // attempt to send the result to the output collection
                if (OutCollection.TryAdd(result) == false)
                {
                    BufferCollection.Add(result);
                    return false;
                }

                return true;
            }
            return false;
        }
    }
}
