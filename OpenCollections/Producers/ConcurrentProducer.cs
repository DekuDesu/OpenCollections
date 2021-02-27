using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCollections
{
    public class ConcurrentProducer<T> : IConcurrentProducer<T>, IDisposable
    {
        /// <summary>
        /// The <see cref="IEnumerable{T}"/> object that this producer should iterate to produce results to the <see cref="ResultCollection"/>.
        /// </summary>
        public IEnumerable<T> Enumerable { get; }

        /// <summary>
        /// The <see cref="IProducerConsumerCollection{T}"/> that items should be added to as <see cref="Enumerable"/> is iterated and results retrieved
        /// </summary>
        public IProducerConsumerCollection<T> ResultCollection { get; set; } = new ConcurrentQueue<T>();

        /// <summary>
        /// Whether or not the producer is currently producing items
        /// </summary>
        public bool Producing { get; private set; }

        public event Action<object, CollectionEventArgs<T>> Started;

        public event Action<object, CollectionEventArgs<T>> CollectionChanged;

        public event Action<object, CollectionEventArgs<T>> Finished;

        /// <summary>
        /// The list where items are temporaily stored while they are waiting to be added to the <see cref="ResultCollection"/>
        /// </summary>
        internal List<T> Buffer { get; private set; } = new List<T>();

        private CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

        private CancellationToken ManagedToken { get; set; }

        /// <summary>
        /// Creates a producer that iterates the <paramref name="enumerableObject"/> and adds the results from each iteration to the <see cref="ResultCollection"/>
        /// </summary>
        /// <param name="enumerableObject"></param>
        public ConcurrentProducer(IEnumerable<T> enumerableObject)
        {
            Enumerable = enumerableObject;
        }

        public void Invoke(object caller, CollectionEventArgs<T> e) => Produce();

        public async Task InvokeAsync(object caller, CollectionEventArgs<T> e)
        {
            await ProduceAsync(e.Token).ConfigureAwait(false);
        }


        public void Produce() => ProduceItems();

        public async Task ProduceAsync()
        {
            SetManagedToken();

            await Task.Run(() => ProduceItems(TokenSource.Token), TokenSource.Token).ConfigureAwait(false);
        }

        public async Task ProduceAsync(CancellationToken token)
        {
            token = SetManagedToken(token);

            await Task.Run(() => ProduceItems(token), token).ConfigureAwait(false);
        }

        private void ProduceItems(CancellationToken token = default)
        {
            if (Producing)
            {
                return;
            }

            Started?.Invoke(this,
                new CollectionEventArgs<T>
                {
                    Token = token,
                    Item = default
                });

            // make sure we dispose of the object
            using (IEnumerator<T> enumerator = Enumerable.GetEnumerator())
            {

                Producing = true;
                // iterate over the enumerator
                while (enumerator.MoveNext())
                {
                    // make sure we can actually cancel the thread
                    token.ThrowIfCancellationRequested();

                    Helpers.Consumer.TryEmptyBuffer(Buffer, ResultCollection, true);

                    T item;

                    if (TryProduceItem(enumerator, out item) == false)
                    {
                        break;
                    }

                    CollectionChanged?.Invoke(this,
                        new CollectionEventArgs<T>
                        {
                            Token = token,
                            Item = item
                        }
                        );
                }
            }

            Helpers.Consumer.TryEmptyBuffer(Buffer, ResultCollection, false);

            Finished?.Invoke(this,
                new CollectionEventArgs<T>
                {
                    Token = token,
                    Item = default
                });

            Producing = false;
        }

        private bool TryProduceItem(IEnumerator<T> Enumerator, out T Item)
        {
            T item = Enumerator.Current;

            if (Equals(item, default) == false)
            {
                if (ResultCollection.TryAdd(item) == false)
                {
                    Buffer.Add(item);
                }
                Item = item;
                return true;
            }

            Item = default;
            return false;
        }

        private CancellationToken SetManagedToken(CancellationToken token = default)
        {
            if (token != default)
            {
                ManagedToken = token;
            }
            else
            {
                TokenSource = new CancellationTokenSource();
                token = TokenSource.Token;
            }
            return token;
        }

        public void Cancel()
        {
            if (ManagedToken != default)
            {
                throw new NotSupportedException($"Cancelling this task when it's token is being managed by a different TokenSource is not supported. Call TokenSource.Cancel() on the object managing the token provided to this object.");
            }
            TokenSource?.Cancel();
        }

        public void Dispose()
        {
            if (Enumerable is IDisposable disposableObject)
            {
                disposableObject?.Dispose();
            }
            ((IDisposable)TokenSource)?.Dispose();
        }


        ~ConcurrentProducer()
        {
            Dispose();
        }
    }
}
