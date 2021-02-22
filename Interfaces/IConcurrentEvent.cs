using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCollections
{
    /// <summary>
    /// Defines an object that contains events for most Consumers and Producers within the OpenCollections library.
    /// </summary>
    public interface IConcurrentEvent
    {
        /// <summary>
        /// Invokes the primary event on the objects some examples:
        /// <para>
        /// <see cref="ConcurrentConsumer{T, TResult}"/>.Invoke() =&gt; <see cref="ConcurrentConsumer{T, TResult}"/>.Consume()
        /// </para>
        /// <para>
        /// <see cref="ConcurrentProducer{T}"/>.Invoke() =&gt; <see cref="ConcurrentProducer{T}"/>.Produce()
        /// </para>
        /// <para>
        /// <see cref="ConcurrentWriter{T}"/>.Invoke() =&gt; <see cref="ConcurrentWriter{T}"/>.WriteLines()
        /// </para>
        /// </summary>
        void Invoke();

        /// <summary>
        /// Invokes the primary event asynchronously on the objects some examples:
        /// <para>
        /// <see cref="ConcurrentConsumer{T, TResult}"/>.InvokeAsync() =&gt; <see cref="ConcurrentConsumer{T, TResult}"/>.ConsumeAsync()
        /// </para>
        /// <para>
        /// <see cref="ConcurrentProducer{T}"/>.InvokeAsync() =&gt; <see cref="ConcurrentProducer{T}"/>.ProduceAsync()
        /// </para>
        /// <para>
        /// <see cref="ConcurrentWriter{T}"/>.InvokeAsync() =&gt; <see cref="ConcurrentWriter{T}"/>.WriteLinesAsync()
        /// </para>
        /// </summary>
        Task InvokeAsync();

        /// <summary>
        /// Invoked when the object is finished with its primary function, for example a <see cref="ConcurrentConsumer{T, TResult}"/>.Finished event will invoke when it's finished consuming.
        /// </summary>
        event Action Finished;

        /// <summary>
        /// Invoked when the collection on this object has changed, this is normally invoked when a consumer consumes an item, or a producer produces an item for exmaple. This is normally invoked the ResultCollection has changed.
        /// </summary>
        event Action CollectionChanged;

        /// <summary>
        /// Invoked when the object is starts its primary function, for example a <see cref="ConcurrentConsumer{T, TResult}"/>.Started event will invoke when it's first started consuming, just before it consumes its first item.
        /// </summary>
        event Action Started;
    }
}
