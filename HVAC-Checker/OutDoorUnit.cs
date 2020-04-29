using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class OutDoorUnit:UnitAircondition
    {

        public OutDoorUnit(long id) : base(id)
        {

        }
        public override string ToString()
        {
            return "室外机";
        }

        public double? IPLV { get; set; }
    }
}
