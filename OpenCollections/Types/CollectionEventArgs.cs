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
        public CancellationToken Token { get; init; } = default;

        public TResult Item { get; init; } = default;
    }
}
