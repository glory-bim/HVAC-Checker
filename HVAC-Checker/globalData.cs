using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public struct globalData
    {       
        public static double buildingHeight { get; set; }
        public static string buildingType { get; set; } 
          
        public static bool haveSubentryMeasures { get; set; }

        public static string climateZone { get; set; }
    }
}
