using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Duct:Element
    {
        public Duct(long id):base(id)
        {
            
        }
        public double? airVelocity { get; set; } = null;
        public string systemType { get; set; } = null;
       
    }
}
