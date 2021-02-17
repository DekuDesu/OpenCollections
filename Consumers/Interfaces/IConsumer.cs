using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace OpenCollections
{
    /// <summary>
    /// Defines an object that consumes items&lt;<typeparamref name="T"/>&gt; and produces items&lt;<typeparamref name="TResult"/>&gt; and outputs them to a collection depending on wether this object is an <see cref="IConcurrentConsumer{T, TResult}"/> or not.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IConsumer<T, TResult>
    {
        /// <summary>
        /// The <see cref="Func{T, TResult}"/> operation that should be performed on items that are consumed from <see cref="TCollection"/>
        /// </summary>
        Func<T, TResult> Operation { get; set; }


        /// <summary>
        /// Begins the consumtion of items from <see cref="Collection"/>, runs <see cref="Operation"/> on the item, and adds the <see cref="TResult"/> to <see cref="ResultCollection"/>
        /// </summary>
        void Consume();

        /// <summary>
        /// Begins the asynchronous consumption of items from <see cref="Collection"/>, runs <see cref="Operation"/> on the item, and adds the <see cref="TResult"/> to <see cref="ResultCollection"/>
        /// </summary>
        Task ConsumeAsync(CancellationToken token);

        /// <summary>
        /// Begins the asynchronous consumption of items from <see cref="Collection"/>, runs <see cref="Operation"/> on the item, and adds the <see cref="TResult"/> to <see cref="ResultCollection"/>
        /// </summary>
        Task ConsumeAsync();

        /// <summary>
        /// Cancels the consumption of items and any associated background tasks.
        /// </summary>
        void Cancel();
    }
}
