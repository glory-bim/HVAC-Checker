using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Fan:Element
    {
        public Fan(long id):base(id)
        {

        }
        public double? m_flowRate { get; set; } = null;
        public string type { get; set; } = null;
    }
}
