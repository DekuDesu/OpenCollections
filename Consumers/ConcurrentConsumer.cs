using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.CompilerServices;

namespace OpenCollections
{
    public class ConcurrentConsumer<T, TResult> : IConcurrentConsumer<T, TResult>, IDisposable
    {
        public bool Consuming { get; private set; }

        /// <summary>
        /// The <typeparamref name="TCollection"/>  that this consumer should consume items from
        /// </summary>
        public IProducerConsumerCollection<T> Collection { get; set; }

        /// <summary>
        /// Gets/Sets the <typeparamref name="TResultCollection"/> that this consumer outputs consumed items to.
        /// </summary>
        public IProducerConsumerCollection<TResult> ResultCollection { get; set; } = new ConcurrentQueue<TResult>();

        public Func<T, TResult> Operation { get; set; } = (T input) => throw new NotImplementedException($"No Func<{typeof(T)},{typeof(TResult)}> assigned to {nameof(ConcurrentConsumer<T, TResult>)}.{nameof(Operation)}");

        internal List<TResult> Buffer = new List<TResult>();

        /// <summary>
        /// Invoked when the consumer runs out of items to consume from ther Collection
        /// </summary>
        public event Action Finished;

        /// <summary>
        /// Invoked every time the consumer successfully consumes and item and adds it to the result collection
        /// </summary>
        public event Action CollectionChanged;

        /// <summary>
        /// Invoked every time the consumer begins consuming items
        /// </summary>
        public event Action Started;

        private CancellationTokenSource TokenSource;

        private CancellationToken ManagedToken;

        public void Invoke() => Consume();
        public async Task InvokeAsync() => await ConsumeAsync().ConfigureAwait(false);

        public void Consume()
        {
            if (Collection == null)
            {
                throw new ArgumentNullException($"You must assign a value to {nameof(ConcurrentConsumer<T, TResult>)}.{nameof(Collection)} before attempting to consume items.");
            }

            Consuming = true;

            Started?.Invoke();

            Helpers.Consumer.ConsumeItems(Collection, ResultCollection, Buffer, Operation, CollectionChanged);

            Consuming = false;

            Finished?.Invoke();
        }

        public async Task ConsumeAsync()
        {
            SetManagedToken();

            await Task.Run(() =>
            {
                Consume();
            }, TokenSource.Token).ConfigureAwait(false);
        }

        public async Task ConsumeAsync(CancellationToken token)
        {
            token = SetManagedToken(token);

            await Task.Run(() =>
            {
                Consume();
            }, token).ConfigureAwait(false);
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
                throw new NotSupportedException(Factory.Messages.ManagedTokenError());
            }
            TokenSource.Cancel();
        }

        public void Dispose()
        {
            ((IDisposable)TokenSource).Dispose();
        }

    }
}
