using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Duct
    {
        public Duct(long id)
        {
            Id = id;
        }
        public double? airVelocity { get; set; } = null;
        public long? Id { get; } =null;
    }
}
