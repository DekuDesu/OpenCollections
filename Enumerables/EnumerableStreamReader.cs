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
    /// A basic <see cref="StreamReader"/> wrapper that acts as an <see cref="IEnumerable{string}"/> instead of a File, used to seamlessly import objects from flat files using the <see cref="OpenCollections"/> objects.
    /// </summary>
    public class EnumerableStreamReader : IEnumerableReader<string>, IDisposable
    {
        public string Path { get; }

        private StreamReader Reader = null;

        /// <summary>
        /// Instantiates a basic wrapper for a default <see cref="StreamReader"/> that acts as a <see cref="IEnumerable{string}"/>, that can be used seamlessly with other <see cref="OpenCollections"/> pipeline objects
        /// </summary>
        /// <param name="path"></param>
        public EnumerableStreamReader(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Iterates over the file located at <see cref="Path"/> and returns <see langword="string"/> of each line of the file
        /// </summary>
        /// <returns>
        /// <see langword="string"/> Next Line in the file being read from <see cref="Path"/>
        /// <para>
        /// <see langword="null"/> When End of File is encountered
        /// </para>
        /// </returns>
        /// <exception cref="IOException"></exception>
        public IEnumerable<string> ReadLine()
        {
            using (Reader = File.OpenText(Path))
            {
                string line;
                while ((line = Reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public IEnumerator<string> GetEnumerator() => ReadLine().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ReadLine().GetEnumerator();

        public void Dispose()
        {
            ((IDisposable)Reader).Dispose();
        }

        ~EnumerableStreamReader()
        {
            Reader?.Dispose();
        }
    }
}
