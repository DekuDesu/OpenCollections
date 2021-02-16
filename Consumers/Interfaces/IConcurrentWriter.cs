using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCollections
{
    public interface IConcurrentWriter<T> : IConcurrentInput<T>, IConcurrentEvent
    {
        string Path { get; }
        Task WriteLinesAsync(bool append = true, CancellationToken token = default);
        Task WriteAsync(bool append = true, CancellationToken token = default);

        void WriteLines(bool append = true);
        void Write(bool append = true);

        void Cancel();
        void Dispose();
    }
}
