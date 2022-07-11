using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP12SSTClassLibrary
{
    public interface ITBObject
    {
        void Initialize();

        void ProcessDPMessage(DP12SSTMessage DisplayPortEvent);
    }
}
