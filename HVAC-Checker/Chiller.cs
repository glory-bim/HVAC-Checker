using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Chiller:Element
    {
        public Chiller(long id) : base(id)
        {

        }
        public override string ToString()
        {
            return "冷水机组";
        }

        public string type { get; set; }

        public string coolingType { get; set;}

        public double? capacity { get; set; }

        public double? COP { get; set; }

        public bool? isFrequencyConversion { get; set; }
        
    }
}
