using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCollections
{
    public interface IConcurrentEvent
    {
        void Invoke();
        Task InvokeAsync();
        event Action Finished;
        event Action CollectionChanged;
        event Action Started;
    }
}
