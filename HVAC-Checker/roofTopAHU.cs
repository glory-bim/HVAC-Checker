using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class RoofTopAHU:UnitAircondition
    {
        public RoofTopAHU(long id) : base(id)
        {

        }
        public override string ToString()
        {
            return "屋顶空调机组";
        }
    }
}
