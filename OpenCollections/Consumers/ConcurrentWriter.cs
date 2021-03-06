﻿using System;
using System.Collections.Generic;
using System.IO;
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

        CancellationTokenSource TokenSource = new CancellationTokenSource();

        CancellationToken ManagedToken;

        private StreamWriter Writer;

        public event Action<object, CollectionEventArgs<T>> Finished;

        public event Action<object, CollectionEventArgs<T>> CollectionChanged;

        public event Action<object, CollectionEventArgs<T>> Started;

        public IProducerConsumerCollection<T> Collection { get; set; } = new ConcurrentQueue<T>();

        public void Invoke(object caller, CollectionEventArgs<T> e) => WriteLines();

        public async Task InvokeAsync(object caller, CollectionEventArgs<T> e)
        {
            await WriteLinesAsync(e.Token).ConfigureAwait(false);
        }

        public async Task WriteLinesAsync() => await BeginWriteLinesAsync(true, default).ConfigureAwait(false);

        public async Task WriteLinesAsync(bool append) => await BeginWriteLinesAsync(append, default).ConfigureAwait(false);

        public async Task WriteLinesAsync(CancellationToken token) => await BeginWriteLinesAsync(true, token).ConfigureAwait(false);

        public async Task WriteLinesAsync(bool append, CancellationToken token) => await BeginWriteLinesAsync(append, token).ConfigureAwait(false);

        public async Task WriteAsync() => await BeginWriteAsync(true, default).ConfigureAwait(false);

        public async Task WriteAsync(bool append) => await BeginWriteAsync(append, default).ConfigureAwait(false);

        public async Task WriteAsync(CancellationToken token) => await BeginWriteAsync(true, token).ConfigureAwait(false);

        public async Task WriteAsync(bool append, CancellationToken token) => await BeginWriteAsync(append, token).ConfigureAwait(false);

        public void WriteLines() => ConsumeLinesAndWrite();

        public void WriteLines(bool append) => ConsumeLinesAndWrite(append);

        public void Write() => ConsumeLinesAndWrite(writeLines: false);

        public void Write(bool append) => ConsumeLinesAndWrite(append, writeLines: false);

        private async Task BeginWriteAsync(bool append, CancellationToken token)
        {
            token = SetManagedToken(token);

            await Task.Run(() => Write(append), token).ConfigureAwait(false);
        }

        private async Task BeginWriteLinesAsync(bool append, CancellationToken token)
        {
            token = SetManagedToken(token);

            await Task.Run(() => WriteLines(append), token).ConfigureAwait(false);
        }

        private void ConsumeLinesAndWrite(bool append = true, bool writeLines = true)
        {
            if (append == false)
            {
                CreateNewFile();
            }

            using (Writer = File.CreateText(Path))
            {
                Started?.Invoke(this, new CollectionEventArgs<T>() { Token = ManagedToken == default ? TokenSource.Token : ManagedToken, Item = default });

                // this has two seperate while loops to avoid a boolean check of writeLines every loop
                if (writeLines)
                {
                    while (Collection.Count > 0)
                    {
                        T line;

                        if (Collection.TryTake(out line))
                        {
                            CollectionChanged?.Invoke(this,
                                new CollectionEventArgs<T>
                                {
                                    Token = ManagedToken == default ? TokenSource.Token : ManagedToken,
                                    Item = line
                                });
                            Writer.WriteLine(line);
                        }
                    }
                }
                else
                {
                    while (Collection.Count > 0)
                    {
                        T line;

                        if (Collection.TryTake(out line))
                        {
                            CollectionChanged?.Invoke(
                                this,
                                new CollectionEventArgs<T>
                                {
                                    Token = ManagedToken == default ? TokenSource.Token : ManagedToken,
                                    Item = line
                                });
                            Writer.Write(line);
                        }
                    }
                }
            }

            Finished?.Invoke(this, new CollectionEventArgs<T>() { Token = ManagedToken == default ? TokenSource.Token : ManagedToken, Item = default });
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
            TokenSource?.Cancel();
        }

        public void Dispose()
        {
            TokenSource?.Cancel();
            ((IDisposable)Writer)?.Dispose();
            ((IDisposable)TokenSource)?.Dispose();
        }

        ~ConcurrentWriter()
        {
            Dispose();
        }
    }
}
