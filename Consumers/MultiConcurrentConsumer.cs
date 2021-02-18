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

        public event Action Finished;
        public event Action CollectionChanged;
        public event Action Started;

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

        private CancellationTokenSource TokenSource { get; set; }

        private CancellationToken ManagedToken { get; set; }

        public MultiConcurrentConsumer()
        {
        }


        public void Invoke() => Consume();

        public async Task InvokeAsync() => await ConsumeAsync().ConfigureAwait(false);

        /// <summary>
        /// Automatically hooks into the given producer/consumer and consumes items whenever items are produced by the other object
        /// </summary>
        /// <param name="ProducerCollection"></param>
        public MultiConcurrentConsumer(IEnumerable<IConcurrentOutput<T>> ProducerCollection)
        {
            List<IProducerConsumerCollection<T>> collections = new List<IProducerConsumerCollection<T>>();

            foreach (var item in ProducerCollection)
            {
                collections.Add(item.ResultCollection);
                item.Started += this.Consume;
                item.CollectionChanged += this.Consume;
                item.Finished += this.Consume;
            }

            Collections = collections;
        }

        public MultiConcurrentConsumer(IProducerConsumerCollection<TResult> resultCollection)
        {
            ResultCollection = resultCollection;
        }

        public void Cancel()
        {
            if (ManagedToken != default)
            {
                throw new NotSupportedException(Factory.Messages.ManagedTokenError());
            }
            TokenSource.Cancel();
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

            Started?.Invoke();

            foreach (var Collection in Collections)
            {
                Helpers.Consumer.ConsumeItems(Collection, ResultCollection, Buffer, Operation, CollectionChanged);
            }

            Consuming = false;

            Finished?.Invoke();
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
            ((IDisposable)TokenSource).Dispose();
        }

    }
}
