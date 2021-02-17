using System.Threading;
using System.Threading.Tasks;

namespace OpenCollections
{
    public interface IConcurrentWriter<T> : IConcurrentInput<T>, IConcurrentEvent
    {
        string Path { get; }

        Task WriteLinesAsync();
        Task WriteLinesAsync(bool append);
        Task WriteLinesAsync(CancellationToken token);
        Task WriteLinesAsync(bool append, CancellationToken token);

        Task WriteAsync();
        Task WriteAsync(bool append);
        Task WriteAsync(CancellationToken token);
        Task WriteAsync(bool append, CancellationToken token);

        void WriteLines();
        void WriteLines(bool append);

        void Write();
        void Write(bool append);

        void Cancel();
        void Dispose();
    }
}
