using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCollections
{
    public class CollectionEventArgs<TResult>
    {
        /// <summary>
        /// The cancellation token of the returning event.
        /// </summary>
        public CancellationToken Token { get; init; } = default;

        /// <summary>
        /// The <see cref="T"/> item that raised the event, if there is one. Defaults to <see langword="default"/> when no item is associated with the call.
        /// </summary>
        public TResult Item { get; init; } = default;
    }
}
