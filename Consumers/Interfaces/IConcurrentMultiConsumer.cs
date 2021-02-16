using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCollections
{
    public interface IConcurrentMultiConsumer<T, TResult> : IConcurrentOutput<TResult>, IConsumer<T, TResult>, IConcurrentEvent
    {
        /// <summary>
        /// The list that contains all of the collections that this consumes from
        /// </summary>
        IList<IProducerConsumerCollection<T>> Collections { get; }
    }
}
