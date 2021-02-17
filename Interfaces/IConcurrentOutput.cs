using System.Collections.Concurrent;

namespace OpenCollections
{
    /// <summary>
    /// Defines an OpenCollections object that outputs objects to a ResultCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConcurrentOutput<T> : IConcurrentEvent
    {
        IProducerConsumerCollection<T> ResultCollection { get; set; }
    }
}
