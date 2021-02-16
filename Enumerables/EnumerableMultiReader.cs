using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCollections
{
    /// <summary>
    /// A basic <see cref="StreamReader"/> wrapper that acts as an <see cref="IEnumerable{string}"/> instead of a File. Can accept many files as constructor parameters and iterates the files in order provided. Used to seamlessly import objects from flat files using the <see cref="OpenCollections"/> objects.
    /// </summary>
    public class EnumerableMultiReader : IEnumerableReader<string>, IDisposable
    {
        /// <summary>
        /// The current path that this reader is reading from.
        /// </summary>
        public int CurrentPath { get; private set; }

        /// <summary>
        /// Whether or not this reader is currently reading from a file.
        /// </summary>
        public bool Reading { get; private set; }

        /// <summary>
        /// The paths that this reader reads from.
        /// </summary>
        public string[] Paths { get; }

        private StreamReader Reader = null;

        /// <summary>
        /// Instantiates a basic wrapper for a default <see cref="StreamReader"/> that acts as a <see cref="IEnumerable{string}"/>, that can be used seamlessly with other <see cref="OpenCollections"/> pipeline objects
        /// </summary>
        /// <param name="path"></param>
        public EnumerableMultiReader(params string[] paths)
        {
            Paths = paths;
        }

        public IEnumerable<string> ReadLine()
        {
            CurrentPath = 0;
            foreach (var path in Paths)
            {
                using (Reader = File.OpenText(path))
                {
                    CurrentPath++;
                    Reading = true;
                    string line;
                    while ((line = Reader.ReadLine()) != null)
                    {
                        yield return line;
                    }
                }
                Reading = false;
            }
        }

        public IEnumerator<string> GetEnumerator() => ReadLine().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ReadLine().GetEnumerator();

        public void Dispose()
        {
            ((IDisposable)Reader).Dispose();
        }

        ~EnumerableMultiReader()
        {
            Reader?.Dispose();
        }
    }
}
