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

        public async Task WriteLinesAsync(bool append = true, CancellationToken token = default)
        {
            SetManagedToken(token);

            await Task.Run(() => WriteLines(append), token);
        }

        public async Task WriteAsync(bool append = true, CancellationToken token = default)
        {
            SetManagedToken(token);

            await Task.Run(() => Write(append), token);
        }

        public void WriteLines(bool append = true)
        {
            ConsumeLinesAndWrite(append, writeLines: true);
        }

        public void Write(bool append = true)
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

        private CancellationToken SetManagedToken(CancellationToken token)
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

        public void Invoke() => WriteLines(true);

        public async Task InvokeAsync() => await WriteLinesAsync(true);

        ~ConcurrentWriter()
        {
            Dispose();
        }
    }
}
