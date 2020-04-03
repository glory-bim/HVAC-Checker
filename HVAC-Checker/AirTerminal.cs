using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class AirTerminal:Element
    {
        public string systemType { get; set; }= null;
        public AirTerminal(long id):base(id)
        {
            
        }
        public double? airVelocity { get; set; } = null;

        public double? elevation { get; set; } = null;

        public double? airFlowRate { get; set; } = null;
    }
}
