using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCollections
{
    /// <summary>
    /// Defines an object that modifies input data using a delagate operation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IDataModifier<T, TResult>
    {
        /// <summary>
        /// The <see cref="Func{T, TResult}"/> operation that should be performed on items that are consumed from <see cref="TCollection"/>
        /// </summary>
        Func<T, TResult> Operation { get; set; }
    }
}
