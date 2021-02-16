using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCollections
{
    public interface IEnumerableReader<T> : IEnumerable<T>
    {
        /// <summary>
        /// Iterates over the file(s) located at <see cref="Path"/> and returns <see langword="string"/> of each line of the file
        /// </summary>
        /// <returns>
        /// <see langword="string"/> Next Line in the file being read from <see cref="Path"/>
        /// <para>
        /// <see langword="null"/> When End of File is encountered
        /// </para>
        /// </returns>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        IEnumerable<string> ReadLine();
    }
}
