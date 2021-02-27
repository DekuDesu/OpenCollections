using System.Threading;
using System.Threading.Tasks;

namespace OpenCollections
{
    public interface IProducer<out T>
    {
        bool Producing { get; }
        /// <summary>
        /// Begin producing items by iterating <see cref="Enumerable"/> and adding the results from each iteration to <see cref="ResultCollection"/>
        /// </summary>
        void Produce();

        /// <summary>
        /// Begin producing items asynchronously by iterating <see cref="Enumerable"/> and adding the results from each iteration to <see cref="ResultCollection"/> from a background thread. Use Cancel() to cancel the background thread
        /// </summary>
        Task ProduceAsync(CancellationToken token);

        /// <summary>
        /// Begin producing items asynchronously by iterating <see cref="Enumerable"/> and adding the results from each iteration to <see cref="ResultCollection"/> from a background thread. Use Cancel() to cancel the background thread
        /// </summary>
        Task ProduceAsync();

        void Cancel();
    }
}
