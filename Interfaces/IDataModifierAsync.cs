using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCollections
{
    public interface IDataModifierAsync<T, TResult>
    {
        /// <summary>
        /// The <see cref="Func{T, TResult}"/> operation that should be performed on items that are consumed from <see cref="TCollection"/>
        /// </summary>
        Func<T, Task<TResult>> AsyncOperation { get; set; }
    }
}
