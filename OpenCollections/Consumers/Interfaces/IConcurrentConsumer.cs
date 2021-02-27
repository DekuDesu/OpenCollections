using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCollections
{
    /// <summary>
    /// Defines an object that consumes items and is thread safe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IConcurrentConsumer<T, TResult> : IConcurrentInput<T>, IConcurrentOutput<TResult>, IConsumer<T, TResult>, IConcurrentInvokable<T>
    {

    }
}
