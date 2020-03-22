using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
   
    public class Pipe : Element
    {
        public Pipe(long id) : base(id)
        {

        }
        public double? m_DN { get; set; } = null;
        public double? m_velocity { get; set; } = null;
    }
}
