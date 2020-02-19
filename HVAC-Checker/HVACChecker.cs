using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    class HVACChecker
    {
        public string runHVACCheck(string[] xdbPaths)
        {
            HVACFunction.HVACXdbPath = xdbPaths[0];
            HVACFunction.ArchXdbPath = xdbPaths[1];
        }
    }
}
