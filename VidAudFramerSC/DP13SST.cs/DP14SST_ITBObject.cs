using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP14SSTClassLibrary
{
    public interface ITBObject
    {
        void Initialize();

        void ProcessDPMessage(DP14SSTMessage DisplayPortEvent);
    }
}
