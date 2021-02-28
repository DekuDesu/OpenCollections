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

        public IProducerConsumerCollection<T> Collection { get; set; }

        public IProducerConsumerCollection<TResult> ResultCollection { get; set; } = new ConcurrentQueue<TResult>();

        public Func<T, TResult> Operation { get; set; } = x => default;

        internal List<TResult> Buffer = new List<TResult>();

        public event Action<object, CollectionEventArgs<TResult>> Finished;

        public event Action<object, CollectionEventArgs<TResult>> CollectionChanged;

        public event Action<object, CollectionEventArgs<TResult>> Started;

        private CancellationTokenSource TokenSource = new CancellationTokenSource();

        private CancellationToken ManagedToken;

        public void Invoke(object caller, CollectionEventArgs<T> e) => Consume(e.Token);

        public async Task InvokeAsync(object caller, CollectionEventArgs<T> e) => await ConsumeAsync(e.Token).ConfigureAwait(false);

        public async Task ConsumeAsync() => await ConsumeAsync(default).ConfigureAwait(false);

        public async Task ConsumeAsync(CancellationToken token)
        {
            // this allows setting the token synchronously instead of waiting until the taskscheduler starts the task, this will allow the Cancel(); method to work immediately when this method is called
            SetManagedToken(token);

            await Task.Run(() => Consume(token), token).ConfigureAwait(false);
        }

        public void Consume() => Consume(default);

        private void Consume(CancellationToken token)
        {
            token = SetManagedToken(token);

            if (Collection is null)
            {
                throw new ArgumentNullException($"You must assign a value to {nameof(ConcurrentConsumer<T, TResult>)}.{nameof(Collection)} before attempting to consume items.");
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
