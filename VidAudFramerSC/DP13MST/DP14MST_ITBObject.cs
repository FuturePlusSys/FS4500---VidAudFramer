using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DP14MSTClassLibrary
{
    public interface ITBObject
    {
        void Initialize();

        void ProcessDPMessage(DP14MSTMessage DisplayPortEvent);
    }
}
