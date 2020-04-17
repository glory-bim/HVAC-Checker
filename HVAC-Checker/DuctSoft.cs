using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{ 

    public class DuctSoft : Element
    {
        public DuctSoft(long id) : base(id)
        {

        }
        public double? airVelocity { get; set; } = null;

    }
}
