using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class assistantFunctions
    {
        public static AirTerminal getAirTerminalOfCertainSystem(List<AirTerminal> airTerminals,string systemType)
        {
            if(airTerminals==null)
            {
                throw new ArgumentException("airTerminals为null");
            }
            if(systemType==null)
            {
                throw new ArgumentException("systemType为null");
            }
            foreach(AirTerminal airTerminal in airTerminals)
            {
                if (airTerminal.systemType == systemType)
                    return airTerminal;
            }
            return null;
        }
    }
}
