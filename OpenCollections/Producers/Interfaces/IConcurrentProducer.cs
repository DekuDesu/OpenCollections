using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCollections
{
    public interface IConcurrentProducer<T> : IConcurrentOutput<T>, IProducer<T>, IConcurrentInvokable<T>, IDisposable
    {

    }
}
