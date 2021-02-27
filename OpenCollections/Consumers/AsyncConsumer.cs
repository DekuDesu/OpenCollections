using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace OpenCollections
{
    public class AsyncConsumer<T, TResult> : IConcurrentInput<T>, IConcurrentOutput<TResult>, IDataModifierAsync<T, TResult>, IConcurrentInvokable<T>, IDisposable
    {
        public IProducerConsumerCollection<T> Collection { get; set; }

        public IProducerConsumerCollection<TResult> ResultCollection { get; set; } = new ConcurrentQueue<TResult>();

        internal IList<TResult> Buffer { get; set; } = new List<TResult>();

        /// <summary>
        /// The time in ms beteween operations
        /// </summary>
        public int OperationCooldown { get; set; } = 1;

        public Func<T, Task<TResult>> AsyncOperation { get; set; }

        public event Action<object, CollectionEventArgs<TResult>> Finished;
        public event Action<object, CollectionEventArgs<TResult>> CollectionChanged;
        public event Action<object, CollectionEventArgs<TResult>> Started;

        private CancellationTokenSource TokenSource = new CancellationTokenSource();

        private CancellationToken ManagedToken;

        public void Invoke(object caller, CollectionEventArgs<T> e) => Consume();

        public async Task InvokeAsync(object caller, CollectionEventArgs<T> e) => await BeginConsuming(e.Token);

        public void Consume()
        {
            BeginConsuming(default).Wait();
        }

        public async Task ConsumeAsync(CancellationToken token) => await BeginConsuming(token);

        public async Task ConsumeAsync() => await BeginConsuming(default);

        private async Task BeginConsuming(CancellationToken token)
        {
            // make sure we have a valid token to determine how to cancel(if appropriate)
            token = SetManagedToken(token);

            Started?.Invoke(this, new CollectionEventArgs<TResult> { Token = token, Item = default });

            // todo
            //await Helpers.Consumer.ConsumeItemsAsync(Collection, ResultCollection, Buffer, AsyncOperation, CollectionChanged, token OperationCooldown).ConfigureAwait(false);

            Finished?.Invoke(this, new CollectionEventArgs<TResult> { Token = token, Item = default });
        }

        private CancellationToken SetManagedToken(CancellationToken token = default)
        {
            if (token != default)
            {
                ManagedToken = token;
                return token;
            }
            return TokenSource?.Token ?? (TokenSource = new CancellationTokenSource()).Token;
        }

        public void Cancel()
        {
            if (ManagedToken == default)
            {
                TokenSource?.Cancel();
            }
            else
            {
                throw new NotSupportedException(Factory.Messages.ManagedTokenError());
            }
        }
        public void Dispose()
        {
            ((IDisposable)TokenSource)?.Dispose();
        }
    }
}
