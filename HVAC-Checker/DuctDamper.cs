using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{ 
    public class DuctDamper : Element
    {
        public DuctDamper(long id) : base(id)
        {

        }
        public double? airVelocity { get; set; } = null;

    }
}
