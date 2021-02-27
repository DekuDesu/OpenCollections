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
        IProducerConsumerCollection<T> Collection { get; set; }
    }
}
