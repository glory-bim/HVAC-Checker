using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{


    public class HeatMeter : Element
    {
        public HeatMeter(long id) : base(id)
        {

        }

        public override string ToString()
        {
            return "HeatMeters";
        }
    }
}
