using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OpenCollection.Tests")]

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

        public Func<T, TResult> Operation { get; set; }

        internal List<TResult> Buffer = new List<TResult>();

        public event Action<object, CollectionEventArgs<TResult>> Finished;

        public event Action<object, CollectionEventArgs<TResult>> CollectionChanged;

        public event Action<object, CollectionEventArgs<TResult>> Started;

        private CancellationTokenSource TokenSource = new CancellationTokenSource();

        private CancellationToken ManagedToken;

        public void Invoke(object caller, CollectionEventArgs<T> e) => Consume();

        public async Task InvokeAsync(object caller, CollectionEventArgs<T> e)
        {
            await ConsumeAsync(e.Token).ConfigureAwait(false);
        }

        public void Consume()
        {
            if (Collection is null)
            {
                throw new ArgumentNullException($"You must assign a value to {nameof(ConcurrentConsumer<T, TResult>)}.{nameof(Collection)} before attempting to consume items.");
            }

            if (Operation is null)
            {
                throw new NotImplementedException($"No Func<{typeof(T)},{typeof(TResult)}> assigned to {nameof(ConcurrentConsumer<T, TResult>)}.{nameof(Operation)}");
            }

            Consuming = true;

            Started?.Invoke(
                this,
                new CollectionEventArgs<TResult>
                {
                    Token = ManagedToken == default ? TokenSource.Token : ManagedToken,
                    Item = default
                }
            );

            Helpers.Consumer.ConsumeItems(
                    caller: this,
                    InCollection: Collection,
                    OutCollection: ResultCollection,
                    BufferCollection: Buffer,
                    Operation: Operation,
                    CollectionChanged: CollectionChanged,
                    token: ManagedToken == default ? TokenSource.Token : ManagedToken
                );

            Consuming = false;

            Finished?.Invoke(
                this,
                new CollectionEventArgs<TResult>
                {
                    Token = ManagedToken == default ? TokenSource.Token : ManagedToken,
                    Item = default
                }
            );
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
            TokenSource?.Cancel();
        }

        public void Dispose()
        {
            ((IDisposable)TokenSource)?.Dispose();
        }
    }
}
