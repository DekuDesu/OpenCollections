using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.CompilerServices;

namespace OpenCollections
{
    /// <summary>
    /// Consumes items from multiple <see cref="IProducerConsumerCollection"/>s at once and output items to a single <see cref="IProducerConsumerCollection"/>. This consumer does not comply with FIFO standards, use PriorityMultiConcurrentConsumer if the order of every element consumed matters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class MultiConcurrentConsumer<T, TResult> : IConcurrentMultiConsumer<T, TResult>, IDisposable
    {
        /// <summary>
        /// Whether or not this consumer is currently consuming items.
        /// </summary>
        public bool Consuming { get; private set; }

        public event Action<object, CollectionEventArgs<TResult>> Finished;

        public event Action<object, CollectionEventArgs<TResult>> CollectionChanged;

        public event Action<object, CollectionEventArgs<TResult>> Started;

        /// <summary>
        /// Any <see cref="IEnumerable{IProducerConsumerCollection{T}}"/> object that items of <see cref="Type{T}"/> should be consumed from.
        /// </summary>
        public IList<IProducerConsumerCollection<T>> Collections { get; set; } = new List<IProducerConsumerCollection<T>>();

        /// <summary>
        /// The <see cref="IProducerConsumerCollection{TResult}"/> collection where consumed items should be added to
        /// </summary>
        public IProducerConsumerCollection<TResult> ResultCollection { get; set; } = new ConcurrentBag<TResult>();

        public Func<T, TResult> Operation { get; set; }

        /// <summary>
        /// The buffer where items are stored while the <see cref="ResultCollection"/> is busy
        /// </summary>
        internal List<TResult> Buffer = new List<TResult>();

        private CancellationTokenSource TokenSource = new CancellationTokenSource();

        private CancellationToken ManagedToken { get; set; }

        public void Invoke(object caller, CollectionEventArgs<T> e) => Consume();

        public async Task InvokeAsync(object caller, CollectionEventArgs<T> e)
        {
            await ConsumeAsync(e.Token).ConfigureAwait(false);
        }

        public void Cancel()
        {
            if (ManagedToken != default)
            {
                throw new NotSupportedException(Factory.Messages.ManagedTokenError());
            }
            TokenSource?.Cancel();
        }

        public void Consume()
        {
            if (Operation is null)
            {
                throw new NotImplementedException($"No Func<{typeof(T)},{typeof(TResult)}> assigned to {nameof(MultiConcurrentConsumer<T, TResult>)}.{nameof(Operation)}");
            }

            if (Consuming)
            {
                return;
            }

            Consuming = true;

            Started?.Invoke(this,
                new CollectionEventArgs<TResult>
                {
                    Token = ManagedToken == default ? TokenSource.Token : ManagedToken,
                    Item = default
                });

            foreach (var Collection in Collections)
            {
                Helpers.Consumer.ConsumeItems(
                        caller: this,
                        InCollection: Collection,
                        OutCollection: ResultCollection,
                        BufferCollection: Buffer,
                        Operation: Operation,
                        CollectionChanged: CollectionChanged,
                        token: ManagedToken == default ? TokenSource.Token : ManagedToken
                    );
            }

            Consuming = false;

            Finished?.Invoke(this,
                new CollectionEventArgs<TResult>
                {
                    Token = ManagedToken == default ? TokenSource.Token : ManagedToken,
                    Item = default
                });
        }

        public async Task ConsumeAsync()
        {
            SetManagedToken();

            await Task.Run(Consume, TokenSource.Token).ConfigureAwait(false);
        }

        public async Task ConsumeAsync(CancellationToken token)
        {
            token = SetManagedToken(token);

            await Task.Run(Consume, token).ConfigureAwait(false);
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

        public void Dispose()
        {
            ((IDisposable)TokenSource)?.Dispose();
        }

    }
}
