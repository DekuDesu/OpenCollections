using System.Collections.Concurrent;

namespace OpenCollections
{
    /// <summary>
    /// Defines and OpenCollections object that accepts objects to consume them
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConcurrentInput<T> : IConcurrentEvent
    {
        IProducerConsumerCollection<T> Collection { get; set; }
    }
}
