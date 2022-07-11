


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DP12MSTClassLibrary
{
    public interface ITBObject
    {
        void Initialize();

        void ProcessDPMessage(DP12MSTMessage DisplayPortEvent);
    }
}
