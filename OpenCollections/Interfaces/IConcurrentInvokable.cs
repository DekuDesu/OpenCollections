using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCollections
{
    public interface IConcurrentInvokable<T>
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
        void Invoke(object caller, CollectionEventArgs<T> e);

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
        Task InvokeAsync(object caller, CollectionEventArgs<T> e);
    }
}
