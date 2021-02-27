using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace OpenCollections
{
    /// <summary>
    /// Defines an OpenCollections object that outputs objects to a ResultCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConcurrentOutput<T> : IConcurrentEvent<T>
    {
        IProducerConsumerCollection<T> ResultCollection { get; set; }
    }
}
