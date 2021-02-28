using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace OpenCollections
{
    /// <summary>
    /// Defines and OpenCollections object that accepts objects to consume them
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConcurrentInput<T>
    {
        /// <summary>
        /// The <see cref="IProducerConsumerCollection{T}"/> collection this object uses as an Input to consume items from.
        /// </summary>
        IProducerConsumerCollection<T> Collection { get; set; }
    }
}
