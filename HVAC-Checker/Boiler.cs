using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Boiler : Element
    {
        public Boiler(long id) : base(id)
        {

        }

        public double thermalPower { get; set; }

        public double ThermalEfficiency { get; set; }

        public double evaporationCapacity { get; set; }

        public string type { get; set; }

        public string fuelType { get; set; }

        public string mediaType { get; set; }

        public override string ToString()
        {
            return "锅炉";
        }
    }
}
