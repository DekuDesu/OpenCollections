using OpenCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCollections
{
    /// <summary>
    /// Defines an object that contains events for most Consumers and Producers within the OpenCollections library.
    /// </summary>
    public interface IConcurrentEvent<T>
    {
        /// <summary>
        /// Invoked when the object is finished with its primary function, for example a <see cref="ConcurrentConsumer{T, TResult}"/>.Finished event will invoke when it's finished consuming.
        /// </summary>
        event Action<object, CollectionEventArgs<T>> Finished;

        /// <summary>
        /// Invoked when the collection on this object has changed, this is normally invoked when a consumer consumes an item, or a producer produces an item for exmaple. This is normally invoked the ResultCollection has changed.
        /// </summary>
        event Action<object, CollectionEventArgs<T>> CollectionChanged;

        /// <summary>
        /// Invoked when the object is starts its primary function, for example a <see cref="ConcurrentConsumer{T, TResult}"/>.Started event will invoke when it's first started consuming, just before it consumes its first item.
        /// </summary>
        event Action<object, CollectionEventArgs<T>> Started;
    }
}
