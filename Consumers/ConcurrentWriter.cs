using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace OpenCollections
{
    public class ConcurrentWriter<T> : IConcurrentWriter<T>, IDisposable
    {
        public string Path { get; }

        public ConcurrentWriter(string path)
        {
            Path = path;
        }

        CancellationTokenSource TokenSource;

        CancellationToken ManagedToken;

        private StreamWriter Writer;

        public event Action Finished;
        public event Action CollectionChanged;
        public event Action Started;

        public IProducerConsumerCollection<T> Collection { get; set; } = new ConcurrentQueue<T>();
        public void Invoke() => WriteLines();

        public async Task InvokeAsync() => await WriteLinesAsync().ConfigureAwait(false);

        public async Task WriteLinesAsync()
        {
            SetManagedToken();

            await Task.Run(() => WriteLines(), TokenSource.Token).ConfigureAwait(false);
        }

        public async Task WriteLinesAsync(bool append)
        {
            SetManagedToken();

            await Task.Run(() => WriteLines(append), TokenSource.Token).ConfigureAwait(false);
        }

        public async Task WriteLinesAsync(CancellationToken token)
        {
            token = SetManagedToken(token);

            await Task.Run(() => WriteLines(), token).ConfigureAwait(false);
        }

        public async Task WriteLinesAsync(bool append, CancellationToken token)
        {
            token = SetManagedToken(token);

            await Task.Run(() => WriteLines(append), token).ConfigureAwait(false);
        }

        public async Task WriteAsync()
        {
            SetManagedToken();

            await Task.Run(() => Write(), TokenSource.Token).ConfigureAwait(false);
        }

        public async Task WriteAsync(bool append)
        {
            SetManagedToken();

            await Task.Run(() => Write(append), TokenSource.Token).ConfigureAwait(false);
        }

        public async Task WriteAsync(CancellationToken token)
        {
            token = SetManagedToken(token);

            await Task.Run(() => Write(), token).ConfigureAwait(false);
        }

        public async Task WriteAsync(bool append, CancellationToken token)
        {
            token = SetManagedToken(token);

            await Task.Run(() => Write(append), token).ConfigureAwait(false);
        }

        public void WriteLines()
        {
            ConsumeLinesAndWrite();
        }

        public void WriteLines(bool append)
        {
            ConsumeLinesAndWrite(append);
        }

        public void Write()
        {
            ConsumeLinesAndWrite(writeLines: false);
        }

        public void Write(bool append)
        {
            ConsumeLinesAndWrite(append, writeLines: false);
        }

        private void ConsumeLinesAndWrite(bool append = true, bool writeLines = true)
        {
            if (append == false)
            {
                CreateNewFile();
            }

            using (Writer = File.CreateText(Path))
            {
                Started?.Invoke();

                // this has two seperate while loops to avoid a boolean check of writeLines every loop
                if (writeLines)
                {
                    while (Collection.Count() > 0)
                    {
                        T line;

                        if (Collection.TryTake(out line))
                        {
                            CollectionChanged?.Invoke();

                            Writer.WriteLine(line);
                        }
                    }
                }
                else
                {
                    while (Collection.Count() > 0)
                    {
                        T line;

                        if (Collection.TryTake(out line))
                        {
                            CollectionChanged?.Invoke();

                            Writer.Write(line);
                        }
                    }
                }
            }

            Finished?.Invoke();
        }

        private void CreateNewFile()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }

        private CancellationToken SetManagedToken(CancellationToken token = default)
        {
            if (token != default)
            {
                ManagedToken = token;
            }
            else
            {
                TokenSource = new CancellationTokenSource();
                token = TokenSource.Token;
            }
            return token;
        }

        public void Cancel()
        {
            if (ManagedToken != default)
            {
                throw new NotSupportedException(Factory.Messages.ManagedTokenError());
            }
            TokenSource.Cancel();
        }

        public void Dispose()
        {
            TokenSource.Cancel();
            ((IDisposable)Writer).Dispose();
            ((IDisposable)TokenSource).Dispose();
        }

        ~ConcurrentWriter()
        {
            Dispose();
        }
    }
}
