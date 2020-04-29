using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class AbsorptionChiller : Element
    {
        public AbsorptionChiller(long id) : base(id)
        {

        }
        public override string ToString()
        {
            return "直燃机";
        }

        public double? coolingCoefficient { get; set; }

        public double? heatingCoefficient { get; set; }

    }
}
