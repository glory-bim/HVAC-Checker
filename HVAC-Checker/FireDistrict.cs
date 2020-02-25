using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class FireDistrict
    {
        FireDistrict(long id)
        {
            Id = id;
        }
        public long Id { get; } = -1;
    }
}
