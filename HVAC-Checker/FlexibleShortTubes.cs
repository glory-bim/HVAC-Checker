using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class FlexibleShortTube:Element
    {

        public FlexibleShortTube(long id) : base(id)
        {

        }

        public double? m_length { get; set; } = null;

        public override string ToString()
        {
            return "FlexibleShortTubes";
        }
    }
}
