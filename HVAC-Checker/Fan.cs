using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    class Fan
    {
        public Fan(long id)
        {
            Id = id;
        }
        public double? airRateOfCondition_1 { get; set; } = null;
        public double? airRateOfCondition_2 { get; set; } =null;
        public bool? isAntiExplosion { set; get; } = null;
        public long? Id { get; } = null;
    }
}
