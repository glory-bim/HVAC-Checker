using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class assistantFunctions
    {
        public static AirTerminal GetAirTerminalOfCertainSystem(List<AirTerminal> airTerminals,string systemType)
        {
            if(systemType==null)
            {
                throw new ArgumentException("systemType为null");
            }
            if (airTerminals == null)
                return null;

            foreach(AirTerminal airTerminal in airTerminals)
            {
                if (airTerminal.systemType == systemType)
                    return airTerminal;
            }
            return null;
        }

        public static Windows GetOpenableOuterWindow(List<Windows> windows)
        {
            if (windows == null)
                return null;
            foreach(Windows window in windows)
            {
                if (window.isExternalWindow.Value&& window.openMode != Windows.WindowOpenMode.FixWindow)
                    return window;
            }

            return null;
        }
    }
}
