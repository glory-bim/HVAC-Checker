using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class UnitAircondition:Element
    {
        public UnitAircondition(long id) : base(id)
        {

        }

        public override string ToString()
        {
            return "UnitAirconditions";
        }
        public string coolingType { get; set; }

        public double? capacity { get; set; }

        public double? EER { get; set; }
    }
}
